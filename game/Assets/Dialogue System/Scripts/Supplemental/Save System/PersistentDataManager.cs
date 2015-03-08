using UnityEngine;
using System.Text;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	public delegate string GetCustomSaveDataDelegate();
	
	/// <summary>
	/// A static class for saving and loading game data using the
	/// dialogue system's Lua environment. It allows you to save or load a game with a single line 
	/// of code.
	/// 
	/// For more information, see @ref saveLoadSystem
	/// </summary>
	public static class PersistentDataManager {

		/// <summary>
		/// Set this <c>true</c> to include all item fields in saved-game data. This is
		/// <c>false</c> by default to minimize the size of the saved-game data by only
		/// recording State and Track (for quests).
		/// </summary>
		public static bool includeAllItemData = false;
		
		/// <summary>
		/// Set this <c>true</c> to exclude Conversation[#].Dialog[#].SimStatus values from
		/// saved-game data. If you don't use SimStatus in your Lua conditions, there's no
		/// need to save it.
		/// </summary>
		public static bool includeSimStatus = false;

		/// <summary>
		/// PersistentDataManager will call this delegate (if set) to add custom data
		/// to the saved-game data string. The custom data should be valid Lua code.
		/// </summary>
		public static GetCustomSaveDataDelegate GetCustomSaveData = null;

		/// <summary>
		/// Resets the Lua environment -- for example, when starting a new game.
		/// </summary>
		/// <param name='databaseResetOptions'>
		/// The database reset options can be:
		/// 
		/// - RevertToDefault: Removes all but the default database, then resets it.
		/// - KeepAllLoaded: Keeps all loaded databases in memory and just resets them.
		/// </param>
		public static void Reset(DatabaseResetOptions databaseResetOptions) {
			DialogueManager.ResetDatabase(databaseResetOptions);
		}

		/// <summary>
		/// Resets the Lua environment -- for example, when starting a new game -- keeping all loaded database
		/// in memory and just resetting them.
		/// </summary>
		public static void Reset() {
			Reset(DatabaseResetOptions.KeepAllLoaded);
		}

		/// <summary>
		/// Sends the OnRecordPersistentData message to all game objects in the scene to give them 
		/// an opportunity to record their state in the Lua environment.
		/// </summary>
		public static void Record() {
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Recording persistent data to Lua environment.", new System.Object[] { DialogueDebug.Prefix }));
			Tools.SendMessageToEveryone("OnRecordPersistentData");
		}

		/// <summary>
		/// Sends the OnApplyPersistentData message to all game objects in the scene to give them an 
		/// opportunity to retrieve their state from the Lua environment.
		/// </summary>
		public static void Apply() {
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Applying persistent data from Lua environment.", new System.Object[] { DialogueDebug.Prefix }));
			Tools.SendMessageToEveryone("OnApplyPersistentData");
		}
		
		/// <summary>
		/// Sends the OnLevelWillBeUnloaded message to all game objects in the scene in case they
		/// need to change their behavior. For example, scripts that do something special when 
		/// destroyed during play may not want to do the same thing when being destroyed by a 
		/// level unload.
		/// </summary>
		public static void LevelWillBeUnloaded() {
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Broadcasting that level will be unloaded.", new System.Object[] { DialogueDebug.Prefix }));
			Tools.SendMessageToEveryone("OnLevelWillBeUnloaded");
		}
		
		/// <summary>
		/// Loads a saved game by applying a saved-game string.
		/// </summary>
		/// <param name='saveData'>
		/// A saved-game string previously returned by GetSaveData().
		/// </param>
		/// <param name='databaseResetOptions'>
		/// Database reset options.
		/// </param>
		public static void ApplySaveData(string saveData, DatabaseResetOptions databaseResetOptions = DatabaseResetOptions.KeepAllLoaded) {
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Resetting Lua environment.", new System.Object[] { DialogueDebug.Prefix }));
			DialogueManager.ResetDatabase(databaseResetOptions);
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Updating Lua environment with saved data.", new System.Object[] { DialogueDebug.Prefix }));
			Lua.Run(saveData, DialogueDebug.LogInfo);
			RefreshRelationshipAndStatusTablesFromLua();
			Apply();
		}
		
		private const int stringDataStartCapacity = 10240;
		private const int stringDataMaxCapacity = 1048576;
		
		/// <summary>
		/// Saves a game by retrieving the Lua environment and returning it as a saved-game string. 
		/// This method calls Record() to allow all game objects in the scene to record their state 
		/// to the Lua environment first. The returned string is human-readable Lua code.
		/// </summary>
		/// <returns>
		/// The saved-game data.
		/// </returns>
		/// <remarks>
		/// To reduce saved-game data size, only the following information is recorded from the 
		/// Chat Mapper tables (Item[], Actor[], etc):
		/// 
		/// - <c>Actor[]</c>: all data
		/// - <c>Item[]</c>: only <c>State</c> (for quest log system)
		/// - <c>Location[]</c>: nothing
		/// - <c>Variable[]</c>: current value of each variable
		/// - <c>Conversation[]</c>: SimStatus
		/// - Relationship and status information is recorded
		/// </remarks>
		public static string GetSaveData() {
			Record();
			StringBuilder sb = new StringBuilder(stringDataStartCapacity, stringDataMaxCapacity);
			AppendVariableData(sb);
			AppendItemData(sb);
			AppendLocationData(sb);
			AppendActorData(sb);
			AppendConversationData(sb);
			AppendRelationshipAndStatusTables(sb);
			if (GetCustomSaveData != null) sb.Append(GetCustomSaveData());
			string saveData = sb.ToString();
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Saved data: {1}", new System.Object[] { DialogueDebug.Prefix, saveData }));
			return saveData;
		}
		
		/// <summary>
		/// Appends the user variable table to a saved-game string.
		/// </summary>
		private static void AppendVariableData(StringBuilder sb) {
			try {
				LuaTableWrapper variableTable = Lua.Run("return Variable").AsTable;
				if (variableTable == null) {
					if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Persistent Data Manager couldn't access Lua Variable[] table", new System.Object[] { DialogueDebug.Prefix }));
					return;
				}
				foreach (var key in variableTable.Keys) {
					var value = variableTable[key.ToString()];
					sb.AppendFormat("Variable[\"{0}\"]={1}; ", new System.Object[] { DialogueLua.StringToTableIndex(key), GetFieldValueString(value) });
				}
			} catch (System.Exception e) {
				Debug.LogError(string.Format("{0}: GetSaveData() failed to get variable data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
			}
		}
		
		/// <summary>
		/// Appends the item table to a saved-game string.
		/// </summary>
		private static void AppendItemData(StringBuilder sb) {
			try {
				LuaTableWrapper itemTable = Lua.Run("return Item").AsTable;
				if (itemTable == null) {
					if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Persistent Data Manager couldn't access Lua Item[] table", new System.Object[] { DialogueDebug.Prefix }));
					return;
				}
				foreach (var title in itemTable.Keys) {
					LuaTableWrapper fields = itemTable[title.ToString()] as LuaTableWrapper;
					bool onlySaveQuestData = !includeAllItemData && (DialogueManager.MasterDatabase.items.Find(i => string.Equals(DialogueLua.StringToTableIndex(i.Name), title)) != null);
					if (fields != null) {
						if (onlySaveQuestData) {
							// If in the database, just record quest statuses and tracking:
							foreach (var fieldKey in fields.Keys) {
								string fieldTitle = fieldKey.ToString();
								if (fieldTitle.EndsWith("State")) {
									sb.AppendFormat("Item[\"{0}\"].{1}=\"{2}\"; ", new System.Object[] { DialogueLua.StringToTableIndex(title), (System.Object) fieldTitle, (System.Object) fields[fieldTitle] });
								} else if (string.Equals(fieldTitle, "Track")) {
									sb.AppendFormat("Item[\"{0}\"].Track={1}; ", new System.Object[] { DialogueLua.StringToTableIndex(title), fields[fieldTitle].ToString().ToLower() });
								}
							}
						} else {
							// If not in the database, record all fields:
							sb.AppendFormat("Item[\"{0}\"]=", new System.Object[] { DialogueLua.StringToTableIndex(title) });
							AppendFields(sb, fields);
						}
					}
				}
			} catch (System.Exception e) {
				Debug.LogError(string.Format("{0}: GetSaveData() failed to get item data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
			}
		}

		private static void AppendFields(StringBuilder sb, LuaTableWrapper fields) {
			sb.Append("{");
			try {
				if (fields != null) {
					foreach (var key in fields.Keys) {
						var value = fields[key.ToString()];
						sb.AppendFormat("{0}={1}, ", new System.Object[] { DialogueLua.StringToTableIndex(key), GetFieldValueString(value) });
					}
				}
			} finally {
				sb.Append("}; ");
			}

		}

		private static string GetFieldValueString(object o) {
			if (o == null) {
				return "nil";
			} else {
				System.Type type = o.GetType();
				if (type == typeof(string)) {
					//return DialogueLua.DoubleQuotesToSingle(string.Format("\"{0}\"", new System.Object[] { o.ToString().Replace("\n", "\\n") }));
					//return string.Format("\"{0}\"", new System.Object[] { o.ToString().Replace("\n", "\\n") });
					return string.Format("\"{0}\"", new System.Object[] { DialogueLua.DoubleQuotesToSingle(o.ToString().Replace("\n", "\\n")) });
				} else if (type == typeof(bool)) {
					return o.ToString().ToLower();
				} else {
					return o.ToString();
				}
			}
		}

		/// <summary>
		/// Appends the location table to a saved-game string. Currently doesn't save anything.
		/// </summary>
		private static void AppendLocationData(StringBuilder sb) {
		}
		
		/// <summary>
		/// Appends the actor table to a saved-game string.
		/// </summary>
		private static void AppendActorData(StringBuilder sb) {
			try {
				LuaTableWrapper actorTable = Lua.Run("return Actor").AsTable;
				if (actorTable == null) {
					if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Persistent Data Manager couldn't access Lua Actor[] table", new System.Object[] { DialogueDebug.Prefix }));
					return;
				}
				foreach (var key in actorTable.Keys) {
					LuaTableWrapper fields = actorTable[key.ToString()] as LuaTableWrapper;
					sb.AppendFormat("Actor[\"{0}\"]={1}", new System.Object[] { DialogueLua.StringToTableIndex(key), '{' });
					try {
						AppendActorFieldData(sb, fields);
					} finally {
						sb.Append("}; ");
					}
				}
			} catch (System.Exception e) {
				Debug.LogError(string.Format("{0}: GetSaveData() failed to get actor data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
			}
		}
		
		/// <summary>
		/// Appends an actor record to a saved-game string.
		/// </summary>
		private static void AppendActorFieldData(StringBuilder sb, LuaTableWrapper fields) {
			if (fields == null) return;
			foreach (var key in fields.Keys) {
				var value = fields[key.ToString()];
				sb.AppendFormat("{0}={1}, ", new System.Object[] { DialogueLua.StringToTableIndex(key), GetFieldValueString(value) });
			}
		}
		
		/// <summary>
		/// Appends the conversation table to a saved-game string. To conserve space, only the
		/// SimStatus is recorded. If includeSimStatus is <c>false</c>, nothing is recorded.
		/// </summary>
		private static void AppendConversationData(StringBuilder sb) {
			if (!(includeSimStatus && DialogueManager.Instance.includeSimStatus)) return;
			foreach (var conversation in DialogueManager.MasterDatabase.conversations) {
				AppendDialogData(sb, conversation);
			}
		}

		/// <summary>
		/// Appends a conversation's dialog table to a saved-game string. To conserve space, this method only 
		/// records each dialogue entry's SimStatus (whether the entry has been offered to the player, or
		/// been spoken by a character).
		/// </summary>
		private static void AppendDialogData(StringBuilder sb, Conversation conversation) {
			if (conversation == null) return;
			try {
				sb.AppendFormat("Conversation[{0}].Dialog = {{ ", conversation.id);
				foreach (var entry in conversation.dialogueEntries) {
					var simStatus = Lua.Run(string.Format("return Conversation[" + conversation.id + "].Dialog[" + entry.id + "].SimStatus"), false, false).AsString;
					sb.AppendFormat("[{0}]={{SimStatus=\"{1}\"}},", new System.Object[] { entry.id, simStatus });
				}
				sb.Append("}; ");
			} catch (System.Exception e) {
				Debug.LogError(string.Format("{0}: GetSaveData() failed to get conversation data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
			}
		}
		
		private static void AppendRelationshipAndStatusTables(StringBuilder sb) {
			try {
				sb.Append(DialogueLua.GetStatusTableAsLua());
				sb.Append(DialogueLua.GetRelationshipTableAsLua());
			} catch (System.Exception e) {
				Debug.LogError(string.Format("{0}: GetSaveData() failed to get relationship and status data: {1}", new System.Object[] { DialogueDebug.Prefix, e.Message }));
			}
		}

		private static void RefreshRelationshipAndStatusTablesFromLua() {
			DialogueLua.RefreshStatusTableFromLua();
			DialogueLua.RefreshRelationshipTableFromLua();
		}

	}

}
