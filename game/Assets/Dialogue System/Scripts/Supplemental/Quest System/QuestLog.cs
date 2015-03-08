using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// A static class that manages a quest log. It uses the Lua "Item[]" table, where each item in 
	/// the table represents a quest. This makes it easy to manage quests in Chat Mapper by adding,
	/// removing, and modifying items in the built-in Item[] table. The name of the item is the
	/// title of the quest. (Note that the Chat Mapper simulator doesn't have a quest system, so it
	/// treats elements of the Item[] table as items.)
	/// 
	/// This class uses the following fields in the Item[] table:
	/// 
	/// - <b>State</b> (if using Chat Mapper, add this custom field or use the Dialogue System template 
	/// project)
	/// 	- Valid values (case-sensitive): <c>unassigned</c>, <c>active</c>, <c>success</c>, 
	/// <c>failure</c>, or <c>done</c>
	/// - <b>Description</b>: The description of the quest
	/// - <b>Success Description</b> (optional): The description to be displayed when the quest has been 
	/// successfully completed
	/// - <b>Failure Description</b> (optional): The description to be displayed when the quest has ended 
	/// in failure
	/// 
	/// Note: <c>done</c> is essentially equivalent to </c>success</c>. In the remainder of the Dialogue 
	/// System's documentation,	either <c>done</c> or <c>success</c> may be used in examples, but when 
	/// using the QuestLog class, they both	correspond to the same enum state, QuestState.Success
	/// 
	/// As an example, you might define a simple quest like this:
	/// 
	/// - Item["Kill 5 Rats"]
	/// 	- State = "unassigned"
	/// 	- Description = "The baker asked me to bring him 5 rat corpses to make a pie."
	/// 	- Success Description = "I brought the baker 5 dead rats, and we ate a delicious pie!"
	/// 	- Failure Description = "I freed the Pied Piper from jail. He took all the rats. No pie for me...."
	/// 
	/// This class provides methods to add and delete quests, get and set their state, and get 
	/// their descriptions.
	/// 
	/// Note that quest states are usually updated during conversations. In most cases, you will 
	/// probably set quest states in Lua code during conversations, so you may never need to use
	/// many of the methods in this class.
	/// 
	/// The UnityQuestLogWindow provides a quest log window using Unity GUI. You can use it as-is 
	/// or use it as a template for implementing your own quest log window in another GUI system 
	/// such as NGUI.
	/// </summary>
	public static class QuestLog {

		/// <summary>
		/// Constant state string for unassigned quests.
		/// </summary>
		public const string UnassignedStateString = "unassigned";
		
		/// <summary>
		/// Constant state string for active quests.
		/// </summary>
		public const string ActiveStateString = "active";
		
		/// <summary>
		/// Constant state string for successfully-completed quests.
		/// </summary>
		public const string SuccessStateString = "success";
		
		/// <summary>
		/// Constant state string for quests ending in failure.
		/// </summary>
		public const string FailureStateString = "failure";

		/// <summary>
		/// Constant state string for quests that were abandoned.
		/// </summary>
		public const string AbandonedStateString = "abandoned";
		
		/// <summary>
		/// Constant state string for quests that are done, if you want to track done instead of success/failure.
		/// This is essentially the same as success, and corresponds to the same enum value, QuestState.Success
		/// </summary>
		public const string DoneStateString = "done";
		
		/// <summary>
		/// Adds a quest to the Lua Item[] table.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='description'>
		/// Description of the quest when active.
		/// </param>
		/// <param name='successDescription'>
		/// Description of the quest when successfully completed.
		/// </param>
		/// <param name='failureDescription'>
		/// Description of the quest when completed in failure.
		/// </param>
		/// <param name='state'>
		/// Quest state.
		/// </param>
		/// <example>
		/// QuestLog.AddQuest("Kill 5 Rats", "The baker asked me to bring 5 rat corpses.", QuestState.Unassigned);
		/// </example>
		public static void AddQuest(string title, string description, string successDescription, string failureDescription, QuestState state) {
			if (!string.IsNullOrEmpty(title)) {
				Lua.Run(string.Format("Item[\"{0}\"] = {{ Name = \"{1}\", Description = \"{2}\", Success_Description = \"{3}\", Failure_Description = \"{4}\", State = \"{5}\" }}", 
				                      new System.Object[] { DialogueLua.StringToTableIndex(title), 
				                      DialogueLua.DoubleQuotesToSingle(title), 
				                      DialogueLua.DoubleQuotesToSingle(description), 
				                      DialogueLua.DoubleQuotesToSingle(successDescription), 
				                      DialogueLua.DoubleQuotesToSingle(failureDescription), 
					 				  StateToString(state) }), 
				        DialogueDebug.LogInfo);
			}
		}

		/// <summary>
		/// Adds a quest to the Lua Item[] table.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='description'>
		/// Description of the quest.
		/// </param>
		/// <param name='state'>
		/// Quest state.
		/// </param>
		/// <example>
		/// QuestLog.AddQuest("Kill 5 Rats", "The baker asked me to bring 5 rat corpses.", QuestState.Unassigned);
		/// </example>
		public static void AddQuest(string title, string description, QuestState state) {
			if (!string.IsNullOrEmpty(title)) {
				Lua.Run(string.Format("Item[\"{0}\"] = {{ Name = \"{1}\", Description = \"{2}\", State = \"{3}\" }}", 
				                      new System.Object[] { DialogueLua.StringToTableIndex(title),
				                      DialogueLua.DoubleQuotesToSingle(title), 
				                      DialogueLua.DoubleQuotesToSingle(description), 
				                      StateToString(state) }), 
				        DialogueDebug.LogInfo);
			}
		}
		
		/// <summary>
		/// Adds a quest to the Lua Item[] table, and sets the state to Unassigned.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='description'>
		/// Description of the quest.
		/// </param>
		/// <example>
		/// QuestLog.AddQuest("Kill 5 Rats", "The baker asked me to bring 5 rat corpses.");
		/// </example>
		public static void AddQuest(string title, string description) {
			AddQuest(title, description, QuestState.Unassigned);
		}
		
		/// <summary>
		/// Deletes a quest from the Lua Item[] table. Use this method if you want to remove a quest entirely.
		/// If you just want to set the state of a quest, use SetQuestState.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <example>
		/// QuestLog.RemoveQuest("Kill 5 Rats");
		/// </example>
		public static void DeleteQuest(string title) {
			if (!string.IsNullOrEmpty(title)) {
				Lua.Run(string.Format("Item[\"{0}\"] = nil", new System.Object[] { DialogueLua.StringToTableIndex(title) }), DialogueDebug.LogInfo);
			}
		}
		
		/// <summary>
		/// Gets the quest state.
		/// </summary>
		/// <returns>
		/// The quest state.
		/// </returns>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <example>
		/// if (QuestLog.QuestState("Kill 5 Rats") == QuestState.Active) {
		///     Smith.Say("Killing rats, eh? Here, take this hammer.");
		/// }
		/// </example>
		public static QuestState GetQuestState(string title) {
			return StringToState(DialogueLua.GetQuestField(title, "State").AsString);
		}
		
		/// <summary>
		/// Sets the quest state.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='state'>
		/// New state.
		/// </param>
		/// <example>
		/// if (PiedPiperIsFree) {
		///     QuestLog.SetQuestState("Kill 5 Rats", QuestState.Failure);
		/// }
		/// </example>
		public static void SetQuestState(string title, QuestState state) {
			DialogueLua.SetQuestField(title, "State", StateToString(state));
			SendUpdateTracker();
		}

		private static void SendUpdateTracker() {
			if (DialogueManager.Instance != null) DialogueManager.Instance.BroadcastMessage("UpdateTracker", SendMessageOptions.DontRequireReceiver);
		}
		
		/// <summary>
		/// Reports whether a quest is unassigned.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the quest is unassigned; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		public static bool IsQuestUnassigned(string title) {
			return GetQuestState(title) == QuestState.Unassigned;
		}
		
		/// <summary>
		/// Reports whether a quest is active.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the quest is active; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		public static bool IsQuestActive(string title) {
			return GetQuestState(title) == QuestState.Active;
		}
		
		/// <summary>
		/// Reports whether a quest was successfully completed.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the quest was successfully completed; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		public static bool IsQuestSuccessful(string title) {
			return GetQuestState(title) == QuestState.Success;
		}
		
		/// <summary>
		/// Reports whether a quest ended in failure.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the quest ended in failure; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		public static bool IsQuestFailed(string title) {
			return GetQuestState(title) == QuestState.Failure;
		}

		/// <summary>
		/// Reports whether a quest was abandoned (i.e., in the Abandoned state).
		/// </summary>
		/// <returns><c>true</c> if the quest was abandoned; otherwise, <c>false</c>.</returns>
		/// <param name="title">Title of the quest.</param>
		public static bool IsQuestAbandoned(string title) {
			return GetQuestState(title) == QuestState.Abandoned;
		}

		/// <summary>
		/// Reports whether a quest is done, either successful or failed.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the quest is done; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		public static bool IsQuestDone(string title) {
			QuestState state = GetQuestState(title);
			return ((state == QuestState.Success) || (state == QuestState.Failure));
		}

		/// <summary>
		/// Reports whether a quest's current state is one of the states marked in a state bit mask.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the quest's current state is in the state bit mask.
		/// </returns>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='stateMask'>
		/// A QuestState bit mask (e.g., <c>QuestState.Success | QuestState.Failure</c>).
		/// </param>
		public static bool IsQuestInStateMask(string title, QuestState stateMask) {
			QuestState state = GetQuestState(title);
			return ((stateMask & state) == state);
		}
		
		/// <summary>
		/// Starts a quest by setting its state to active.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <example>
		/// StartQuest("Kill 5 Rats");
		/// </example>
		public static void StartQuest(string title) {
			SetQuestState(title, QuestState.Active);
		}
		
		/// <summary>
		/// Marks a quest successful.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		public static void CompleteQuest(string title) {
			SetQuestState(title, QuestState.Success);
		}
		
		/// <summary>
		/// Marks a quest as failed.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		public static void FailQuest(string title) {
			SetQuestState(title, QuestState.Failure);
		}

		/// <summary>
		/// Marks a quest as abandoned (i.e., in the Abandoned state).
		/// </summary>
		/// <param name="title">Title of the quest.</param>
		public static void AbandonQuest(string title) {
			SetQuestState(title, QuestState.Abandoned);
		}
		/// <summary>
		/// Converts a string representation into a state enum value.
		/// </summary>
		/// <returns>
		/// The state (e.g., <c>QuestState.Active</c>).
		/// </returns>
		/// <param name='s'>
		/// The string representation (e.g., "active").
		/// </param>
		public static QuestState StringToState(string s) {
			if (string.Equals(s, ActiveStateString)) return QuestState.Active;
			if (string.Equals(s, SuccessStateString) || string.Equals(s, DoneStateString)) return QuestState.Success;
			if (string.Equals(s, FailureStateString)) return QuestState.Failure;
			if (string.Equals(s, AbandonedStateString)) return QuestState.Abandoned;
			return QuestState.Unassigned;
		}
		
		/// <summary>
		/// Converts a state to its string representation.
		/// </summary>
		/// <returns>
		/// The string representation (e.g., "active").
		/// </returns>
		/// <param name='state'>
		/// The state (e.g., <c>QuestState.Active</c>).
		/// </param>
		public static string StateToString(QuestState state) {
			switch (state) {
			case QuestState.Unassigned: return UnassignedStateString;
			case QuestState.Active: return ActiveStateString;
			case QuestState.Success: return SuccessStateString;
			case QuestState.Failure: return FailureStateString;
			case QuestState.Abandoned: return AbandonedStateString;
			default: return UnassignedStateString;
			}
		}

		/// <summary>
		/// Gets the localized quest title.
		/// </summary>
		/// <returns>
		/// The quest title in the current language.
		/// </returns>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		public static string GetQuestTitle(string title) {
			return DialogueLua.GetLocalizedQuestField(title, "Name").AsString;
		}
		
		/// <summary>
		/// Gets a quest description, based on the current state of the quest (i.e., SuccessDescription, FailureDescription, or just Description).
		/// </summary>
		/// <returns>
		/// The quest description.
		/// </returns>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <example>
		/// GUILayout.Label("Objective: " + QuestLog.GetQuestDescription("Kill 5 Rats"));
		/// </example>
		public static string GetQuestDescription(string title) {
			switch (GetQuestState(title)) {
			case QuestState.Success: 
				return GetQuestDescription(title, QuestState.Success) ?? GetQuestDescription(title, QuestState.Active);
			case QuestState.Failure:
				return GetQuestDescription(title, QuestState.Failure) ?? GetQuestDescription(title, QuestState.Active);
			default:
				return GetQuestDescription(title, QuestState.Active);
			}
		}
		
		/// <summary>
		/// Gets the localized quest description for a specific state.
		/// </summary>
		/// <returns>
		/// The quest description.
		/// </returns>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='state'>
		/// State to check.
		/// </param>
		public static string GetQuestDescription(string title, QuestState state) {
			string descriptionFieldName = GetDefaultDescriptionFieldForState(state);
			string result = DialogueLua.GetLocalizedQuestField(title, descriptionFieldName).AsString;
			return (string.Equals(result, "nil") || string.IsNullOrEmpty(result)) ? null : result;
		}
		
		private static string GetDefaultDescriptionFieldForState(QuestState state) {
			switch (state) {
			case QuestState.Success:
				return "Success_Description";
			case QuestState.Failure:
				return "Failure_Description";
			default:
				return "Description";
			}
		}
		
		/// <summary>
		/// Sets the quest description for a specified state.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='state'>
		/// Set the description for this state (i.e., regular, success, or failure).
		/// </param>
		/// <param name='description'>
		/// The description.
		/// </param>
		public static void SetQuestDescription(string title, QuestState state, string description) {
			DialogueLua.SetQuestField(title, GetDefaultDescriptionFieldForState(state), description);
		}

		/// <summary>
		/// Gets the quest abandon sequence. The QuestLogWindow plays this sequence when the player
		/// abandons a quest.
		/// </summary>
		/// <returns>The quest abandon sequence.</returns>
		/// <param name="title">Quest title.</param>
		public static string GetQuestAbandonSequence(string title) {
			return DialogueLua.GetLocalizedQuestField(title, "Abandon Sequence").AsString;
		}

		/// <summary>
		/// Sets the quest abandon sequence. The QuestLogWindow plays this sequence when the 
		/// player abandons a quest.
		/// </summary>
		/// <param name="title">Quest title.</param>
		/// <param name="sequence">Sequence to play when the quest is abandoned.</param>
		public static void SetQuestAbandonSequence(string title, string sequence) {
			DialogueLua.SetLocalizedQuestField(title, "Abandon Sequence", sequence);
		}

		/// <summary>
		/// Gets the quest entry count.
		/// </summary>
		/// <returns>The quest entry count.</returns>
		/// <param name="questTitle">Title of the quest.</param>
		public static int GetQuestEntryCount(string questTitle) {
			return DialogueLua.GetQuestField(questTitle, "Entry_Count").AsInt;
		}

		/// <summary>
		/// Adds a quest entry to a quest.
		/// </summary>
		/// <param name="questTitle">Title of the quest.</param>
		/// <param name="entryNumber">Entry number.</param>
		/// <param name="description">The quest entry description.</param>
		public static void AddQuestEntry(string questTitle, string description) {
			int entryCount = GetQuestEntryCount(questTitle);
			entryCount++;
			DialogueLua.SetQuestField(questTitle, "Entry_Count", entryCount);
			string entryFieldName = GetEntryFieldName(entryCount);
			DialogueLua.SetQuestField(questTitle, entryFieldName, DialogueLua.DoubleQuotesToSingle(description));
			string entryStateFieldName = GetEntryStateFieldName(entryCount);
			DialogueLua.SetQuestField(questTitle, entryStateFieldName, "unassigned");
		}
		
		/// <summary>
		/// Gets the localized quest entry description.
		/// </summary>
		/// <returns>The quest entry description.</returns>
		/// <param name="questTitle">Title of the quest.</param>
		/// <param name="entryNumber">Entry number.</param>
		public static string GetQuestEntry(string questTitle, int entryNumber) {
			string entryFieldName = GetEntryFieldName(entryNumber);
			return DialogueLua.GetLocalizedQuestField(questTitle, entryFieldName).AsString;
		}

		/// <summary>
		/// Sets the localized quest entry description.
		/// </summary>
		/// <param name="questTitle">Title of the quest.</param>
		/// <param name="entryNumber">Entry number.</param>
		/// <param name="description">The quest entry description.</param>
		public static void SetQuestEntry(string questTitle, int entryNumber, string description) {
			string entryFieldName = GetEntryFieldName(entryNumber);
			DialogueLua.SetLocalizedQuestField(questTitle, entryFieldName, DialogueLua.DoubleQuotesToSingle(description));
		}

		/// <summary>
		/// Gets the state of the quest entry.
		/// </summary>
		/// <returns>The quest entry state.</returns>
		/// <param name="questTitle">Title of the quest.</param>
		/// <param name="entryNumber">Entry number.</param>
		public static QuestState GetQuestEntryState(string questTitle, int entryNumber) {
			string s = DialogueLua.GetQuestField(questTitle, GetEntryStateFieldName(entryNumber)).AsString;
			return StringToState(s);
		}

		/// <summary>
		/// Sets the state of the quest entry.
		/// </summary>
		/// <param name="questTitle">Title of the quest.</param>
		/// <param name="entryNumber">Entry number.</param>
		/// <param name="state">State.</param>
		public static void SetQuestEntryState(string questTitle, int entryNumber, QuestState state) {
			DialogueLua.SetQuestField(questTitle, GetEntryStateFieldName(entryNumber), StateToString(state));
			SendUpdateTracker();
		}

		private static string GetEntryFieldName(int entryNumber) {
			return string.Format("Entry_{0}", new System.Object[] { entryNumber });
		}
		
		private static string GetEntryStateFieldName(int entryNumber) {
			return string.Format("Entry_{0}_State", new System.Object[] { entryNumber });
		}
		
		/// <summary>
		/// Determines if quest tracking is available (that is, if the quest has a "Track" field).
		/// </summary>
		/// <returns><c>true</c> if quest tracking is available; otherwise, <c>false</c>.</returns>
		/// <param name="questTitle">Quest title.</param>
		public static bool IsQuestTrackingAvailable(string questTitle) {
			return DialogueLua.GetQuestField(questTitle, "Trackable").AsBool;
		}

		/// <summary>
		/// Determines if tracking is enabled for a quest.
		/// </summary>
		/// <returns><c>true</c> if tracking enabled on the specified quest; otherwise, <c>false</c>.</returns>
		/// <param name="questTitle">Quest title.</param>
		public static bool IsQuestTrackingEnabled(string questTitle) {
			return IsQuestTrackingAvailable(questTitle)
				? DialogueLua.GetQuestField(questTitle, "Track").AsBool
				: false;
		}

		/// <summary>
		/// Sets quest tracking on or off.
		/// </summary>
		/// <param name="questTitle">Quest title.</param>
		/// <param name="value">If set to <c>true</c>, tracking is enabled.</param>
		public static void SetQuestTracking(string questTitle, bool value) {
			DialogueLua.SetQuestField(questTitle, "Track", value);
		}

		/// <summary>
		/// Determines if a quest is abandonable (that is, is has a field named "Abandonable" that's true.)
		/// </summary>
		/// <returns><c>true</c> if the quest is abandonable; otherwise, <c>false</c>.</returns>
		/// <param name="questTitle">Quest title.</param>
		public static bool IsQuestAbandonable(string questTitle) {
			return DialogueLua.GetQuestField(questTitle, "Abandonable").AsBool;
		}

		/// <summary>
		/// Gets an array of all quests matching the specified state bitmask.
		/// </summary>
		/// <returns>
		/// The titles of all quests matching the specified state bitmask.
		/// </returns>
		/// <param name='flags'>
		/// A bitmask of QuestState values.
		/// </param>
		/// <example>
		/// string[] completedQuests = QuestLog.GetAllQuests( QuestState.Success | QuestState.Failure );
		/// </example>
		public static string[] GetAllQuests(QuestState flags) {
			List<string> titles = new List<string>();
			LuaTableWrapper itemTable = Lua.Run("return Item").AsTable;
			if (!itemTable.IsValid) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Quest Log couldn't access Lua Item[] table. Has the Dialogue Manager loaded a database yet?", new System.Object[] { DialogueDebug.Prefix }));
				return titles.ToArray();
			}
			foreach (LuaTableWrapper fields in itemTable.Values) {
				string title = null;
				bool isItem = false;
				try { 
					object titleObject = fields["Name"];
					title = (titleObject != null) ? titleObject.ToString() : string.Empty; 
					isItem = false;
					object isItemObject = fields["Is_Item"];
					if (isItemObject != null) {
						if (isItemObject.GetType() == typeof(bool)) {
							isItem = (bool) isItemObject;
						} else {
							isItem = Tools.StringToBool(isItemObject.ToString());
						}
					}
				} catch {}
				if (!isItem) {
					if (string.IsNullOrEmpty(title)) {
						if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: A quest title (item name in Item[] table) is null or empty", new System.Object[] { DialogueDebug.Prefix }));
					} else if (IsQuestInStateMask(title, flags)) {
						titles.Add(title);
					}
				}
			}
			titles.Sort();
			return titles.ToArray();
		}

		/// <summary>
		/// Gets an array of all active quests.
		/// </summary>
		/// <returns>
		/// The titles of all active quests.
		/// </returns>
		/// <example>
		/// string[] activeQuests = QuestLog.GetAllQuests();
		/// </example>
		public static string[] GetAllQuests() {
			return GetAllQuests(QuestState.Active);
		}

		/// <summary>
		/// Quest changed delegate.
		/// </summary>
		public delegate void QuestChangedDelegate(string title, QuestState newState);
		
		/// <summary>
		/// The quest watch item class is used internally by the QuestLog class to manage
		/// Lua observers on quest states.
		/// </summary>
		public class QuestWatchItem {
			
			private string title;
			private int entryNumber;
			private LuaWatchFrequency frequency;
			private string luaExpression;
			private QuestChangedDelegate questChangedHandler;
			
			public QuestWatchItem(string title, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler) {
				this.title = title;
				this.entryNumber = 0;
				this.frequency = frequency;
				this.luaExpression = string.Format("return Item[\"{0}\"].State", new System.Object[] { DialogueLua.StringToTableIndex(title) });
				this.questChangedHandler = questChangedHandler;
				DialogueManager.AddLuaObserver(luaExpression, frequency, OnLuaChanged);
			}
			
			public QuestWatchItem(string title, int entryNumber, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler) {
				this.title = title;
				this.entryNumber = entryNumber;
				this.frequency = frequency;
				this.luaExpression = string.Format("return Item[\"{0}\"].Entry_{1}_State", new System.Object[] { DialogueLua.StringToTableIndex(title), entryNumber });
				this.questChangedHandler = questChangedHandler;
				DialogueManager.AddLuaObserver(luaExpression, frequency, OnLuaChanged);
			}
			
			public bool Matches(string title, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler) {
				return string.Equals(title, this.title) && (frequency == this.frequency) && (questChangedHandler == this.questChangedHandler);
			}
			
			public bool Matches(string title, int entryNumber, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler) {
				return string.Equals(title, this.title) && (entryNumber == this.entryNumber) && (frequency == this.frequency) && (questChangedHandler == this.questChangedHandler);
			}
			
			public void StopObserving() {
				DialogueManager.RemoveLuaObserver(luaExpression, frequency, OnLuaChanged);
			}
			
			private void OnLuaChanged(LuaWatchItem luaWatchItem, Lua.Result newResult) {
				if (string.Equals(luaWatchItem.LuaExpression, this.luaExpression) && (questChangedHandler != null)) {
					questChangedHandler(title, StringToState(newResult.AsString));
				}
			}
		}
		
		/// <summary>
		/// The quest watch list.
		/// </summary>
		private static readonly List<QuestWatchItem> questWatchList = new List<QuestWatchItem>();
		
		/// <summary>
		/// Adds a quest state observer.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='frequency'>
		/// Frequency to check the quest state.
		/// </param>
		/// <param name='questChangedHandler'>
		/// Delegate to call when the quest state changes. This should be in the form:
		/// <code>void MyDelegate(string title, QuestState newState) {...}</code>
		/// </param>
		public static void AddQuestStateObserver(string title, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler) {
			questWatchList.Add(new QuestWatchItem(title, frequency, questChangedHandler));
		}
		
		/// <summary>
		/// Adds a quest state observer.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='entryNumber'>
		/// The entry number (1...Entry Count) in the quest.
		/// </param>
		/// <param name='frequency'>
		/// Frequency to check the quest state.
		/// </param>
		/// <param name='questChangedHandler'>
		/// Delegate to call when the quest state changes. This should be in the form:
		/// <code>void MyDelegate(string title, QuestState newState) {...}</code>
		/// </param>
		public static void AddQuestStateObserver(string title, int entryNumber, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler) {
			questWatchList.Add(new QuestWatchItem(title, entryNumber, frequency, questChangedHandler));
		}
		
		/// <summary>
		/// Removes a quest state observer. To be removed, the title, frequency, and delegate must
		/// all match.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='frequency'>
		/// Frequency that the quest state is being checked.
		/// </param>
		/// <param name='questChangedHandler'>
		/// Quest changed handler delegate.
		/// </param>
		public static void RemoveQuestStateObserver(string title, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler) {
			foreach (var questWatchItem in questWatchList) {
				if (questWatchItem.Matches(title, frequency, questChangedHandler)) questWatchItem.StopObserving();
			}
			questWatchList.RemoveAll(questWatchItem => questWatchItem.Matches(title, frequency, questChangedHandler));
		}
		
		/// <summary>
		/// Removes a quest state observer. To be removed, the title, frequency, and delegate must
		/// all match.
		/// </summary>
		/// <param name='title'>
		/// Title of the quest.
		/// </param>
		/// <param name='entryNumber'>
		/// The entry number (1...Entry Count) in the quest.
		/// </param>
		/// <param name='frequency'>
		/// Frequency that the quest state is being checked.
		/// </param>
		/// <param name='questChangedHandler'>
		/// Quest changed handler delegate.
		/// </param>
		public static void RemoveQuestStateObserver(string title, int entryNumber, LuaWatchFrequency frequency, QuestChangedDelegate questChangedHandler) {
			foreach (var questWatchItem in questWatchList) {
				if (questWatchItem.Matches(title, entryNumber, frequency, questChangedHandler)) questWatchItem.StopObserving();
			}
			questWatchList.RemoveAll(questWatchItem => questWatchItem.Matches(title, entryNumber, frequency, questChangedHandler));
		}
		
		/// <summary>
		/// Removes all quest state observers.
		/// </summary>
		public static void RemoveAllQuestStateObservers() {
			foreach (var questWatchItem in questWatchList) {
				questWatchItem.StopObserving();
			}
			questWatchList.Clear();
		}
		
	}

}
