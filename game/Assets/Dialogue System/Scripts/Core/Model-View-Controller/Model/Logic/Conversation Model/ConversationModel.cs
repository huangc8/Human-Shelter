using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This defines the syntax of the optional delegate that conversations can use
	/// to double-check that a dialogue entry is valid to use.
	/// </summary>
	public delegate bool IsDialogueEntryValidDelegate(DialogueEntry dialogueEntry);
	
	/// <summary>
	/// Handles logic for the data model of a conversation. This class retrieves dialogue entries
	/// and updates the data state, which includes the dialogue database and Lua environment. It 
	/// does not keep track of state or handle the user interface. The ConversationController 
	/// controls the current state of the conversation, and ConversationView handles the user 
	/// interface.
	/// </summary>
	public class ConversationModel {
		
		/// <summary>
		/// The dialogue database used in this conversation.
		/// </summary>
		private DialogueDatabase database = null;
		
		/// <summary>
		/// Info about the character that is specified as the actor in the conversation. This is 
		/// typically the character that initiates the conversation, such as the PC.
		/// </summary>
		private CharacterInfo actorInfo = null;
		
		/// <summary>
		/// Info about the character that is specified as the conversant in the conversation. This
		/// is the other character in the conversation (e.g., the NPC in a PC-NPC conversation).
		/// </summary>
		private CharacterInfo conversantInfo = null;
		
		/// <summary>
		/// The first state in the conversation, which is the root of the dialogue tree.
		/// </summary>
		/// <value>
		/// The first state.
		/// </value>
		public ConversationState FirstState { get; private set; }
		
		/// <summary>
		/// The current conversation ID. When this changes (in GotoState), the Lua environment
		/// needs to set the Dialog[] table to the new conversation's table.
		/// </summary>
		private int currentConversationID = -1;
		
		/// <summary>
		/// Gets the actor info for this conversation.
		/// </summary>
		/// <value>
		/// The actor info.
		/// </value>
		public CharacterInfo ActorInfo { get { return actorInfo; } }
		
		/// <summary>
		/// Gets the conversant info for this conversation.
		/// </summary>
		/// <value>
		/// The conversant info.
		/// </value>
		public CharacterInfo ConversantInfo { get { return conversantInfo; } }

		/// <summary>
		/// Indicates whether the conversation has any responses linked from the start entry.
		/// </summary>
		/// <value><c>true</c> if the conversation has responses; otherwise, <c>false</c>.</value>
		public bool HasValidEntry { get { return (FirstState != null) && FirstState.HasAnyResponses; } }

		/// <summary>
		/// Gets or sets the IsDialogueEntryValid delegate.
		/// </summary>
		public IsDialogueEntryValidDelegate IsDialogueEntryValid { get; set; }
		
		private bool allowLuaExceptions = false;

		private Dictionary<int, CharacterInfo> characterInfoCache = new Dictionary<int, CharacterInfo>();

		private EntrytagFormat entrytagFormat = EntrytagFormat.ActorName_ConversationID_EntryID;

		private EmTag emTagForOldResponses = EmTag.None;

		/// <summary>
		/// Initializes a new ConversationModel.
		/// </summary>
		/// <param name='database'>
		/// The database to use.
		/// </param>
		/// <param name='title'>
		/// The title of the conversation in the database.
		/// </param>
		/// <param name='actor'>
		/// Actor.
		/// </param>
		/// <param name='conversant'>
		/// Conversant.
		/// </param>
		public ConversationModel(DialogueDatabase database, string title, Transform actor, Transform conversant, 
		                         bool allowLuaExceptions, IsDialogueEntryValidDelegate isDialogueEntryValid) {
			this.allowLuaExceptions = allowLuaExceptions;
			this.database = database;
			this.IsDialogueEntryValid = isDialogueEntryValid;
			Conversation conversation = database.GetConversation(title);
			if (conversation != null) {
				SetParticipants(conversation, actor, conversant);
				FirstState = GetState(conversation.GetFirstDialogueEntry());
				FixFirstStateSequence();
			} else {
				FirstState = null;
				if (DialogueDebug.LogErrors) Debug.LogWarning(string.Format("{0}: Conversation '{1}' not found in database.", new System.Object[] { DialogueDebug.Prefix, title }));
			}
			DisplaySettings displaySettings = DialogueManager.DisplaySettings;
			if (displaySettings != null) {
				if (displaySettings.cameraSettings != null) entrytagFormat = displaySettings.cameraSettings.entrytagFormat;
				if (displaySettings.inputSettings != null) emTagForOldResponses = displaySettings.inputSettings.emTagForOldResponses;
			}
		}
		
		/// <summary>
		/// If the START entry's sequence is empty and there's no subtitle text, don't
		/// use the default sequence. Instead, use None().
		/// </summary>
		private void FixFirstStateSequence() {
			if ((FirstState != null) && 
				(FirstState.subtitle != null) &&
				string.IsNullOrEmpty(FirstState.subtitle.sequence) &&
				string.IsNullOrEmpty(FirstState.subtitle.formattedText.text)) {
				FirstState.subtitle.sequence = "None()";
			}				
		}

		/// <summary>
		/// Sends a message to the actor and conversant. Used to send OnConversationStart and 
		/// OnConversationEnd messages.
		/// </summary>
		/// <param name='message'>
		/// The message (e.g., OnConversationStart or OnConversationEnd).
		/// </param>
		public void InformParticipants(string message) {
			Transform actor = (actorInfo == null) ? null : actorInfo.transform;
			Transform conversant = (conversantInfo == null) ? null : conversantInfo.transform;
			if (actor != null) actor.BroadcastMessage(message, conversant ?? actor, SendMessageOptions.DontRequireReceiver);
			if ((conversant != null) && (conversant != actor)) conversant.BroadcastMessage(message, actor ?? conversant, SendMessageOptions.DontRequireReceiver);
		}
		
		/// <summary>
		/// "Follows" a dialogue entry and returns its full conversation state. This method updates 
		/// the Lua environment (marking the entry as visited). If includeLinks is <c>true</c>, 
		/// it evaluates all links from the dialogue entry and records valid links in the state.
		/// </summary>
		/// <returns>
		/// The state representing the dialogue entry.
		/// </returns>
		/// <param name='entry'>
		/// The dialogue entry to "follow."
		/// </param>
		/// <param name='includeLinks'>
		/// If <c>true</c>, records all links from the dialogue entry whose conditions are true.
		/// </param>
		public ConversationState GetState(DialogueEntry entry, bool includeLinks) {
			if (entry != null) {
				DialogueLua.MarkDialogueEntryDisplayed(entry);
				SetDialogTable(entry.conversationID);
				Lua.Run(entry.userScript, DialogueDebug.LogInfo, allowLuaExceptions);
				CharacterInfo actorInfo = GetCharacterInfo(entry.ActorID);
				CharacterInfo listenerInfo = GetCharacterInfo(entry.ConversantID);
				FormattedText formattedText = FormattedText.Parse(entry.SubtitleText, database.emphasisSettings);
				CheckSequenceField(entry);
				string entrytag = database.GetEntrytag(entry.conversationID, entry.id, entrytagFormat);
				Subtitle subtitle = new Subtitle(actorInfo, listenerInfo, formattedText, entry.Sequence, entry.ResponseMenuSequence, entry, entrytag);
				List<Response> npcResponses = new List<Response>();
				List<Response> pcResponses = new List<Response>();
				if (includeLinks) EvaluateLinks(entry, npcResponses, pcResponses, new List<DialogueEntry>());
				return new ConversationState(subtitle, npcResponses.ToArray(), pcResponses.ToArray(), entry.isGroup);
			} else {
				return null;
			}		
		}

		/// <summary>
		/// "Follows" a dialogue entry and returns its full conversation state. This method updates 
		/// the Lua environment (marking the entry as visited) and evaluates all links from the 
		/// dialogue entry and records valid links in the state.
		/// </summary>
		/// <returns>
		/// The state representing the dialogue entry.
		/// </returns>
		/// <param name='entry'>
		/// The dialogue entry to "follow."
		/// </param>
		public ConversationState GetState(DialogueEntry entry) {
			return GetState(entry, true);
		}

		/// <summary>
		/// Updates the responses in the specified state.
		/// </summary>
		/// <param name="state">State to check.</param>
		public void UpdateResponses(ConversationState state) {
			List<Response> npcResponses = new List<Response>();
			List<Response> pcResponses = new List<Response>();
			EvaluateLinks(state.subtitle.dialogueEntry, npcResponses, pcResponses, new List<DialogueEntry>());
			state.npcResponses = npcResponses.ToArray();
			state.pcResponses = pcResponses.ToArray();
		}
			
		private void SetDialogTable(int newConversationID) {
			if (currentConversationID != newConversationID) {
				currentConversationID = newConversationID;
				Lua.Run(string.Format("Dialog = Conversation[{0}].Dialog", new System.Object[] { newConversationID }));
			}
		}
		
		private void CheckSequenceField(DialogueEntry entry) {
			if (string.IsNullOrEmpty(entry.Sequence) && !string.IsNullOrEmpty(entry.VideoFile)) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Dialogue entry '{1}' Video File field is assigned but Sequence is blank. Cutscenes now use Sequence field.", new System.Object[] { DialogueDebug.Prefix, entry.DialogueText }));
			}
		}

		/// <summary>
		/// Evaluates a dialogue entry's links. Evaluation follows the same rules as Chat Mapper:
		/// - Links are evaluated from highest to lowest priority; once links are found in a
		/// priority level, evaluation stops. Lower priority links aren't evaluated.
		/// - If a link evaluates <c>false</c> and the false condition action is "Passthrough",
		/// the link's children are evaluated.
		/// </summary>
		/// <param name='entry'>
		/// Dialogue entry.
		/// </param>
		/// <param name='npcResponses'>
		/// Links from the entry that are NPC responses are added to this list.
		/// </param>
		/// <param name='pcResponses'>
		/// Links from the entry that are PC responses are added to this list.
		/// </param>
		/// <param name='visited'>
		/// Keeps track of links that have already been visited so we don't loop back on ourselves
		/// and get frozen in an infinite loop.
		/// </param>
		private void EvaluateLinks(DialogueEntry entry, List<Response> npcResponses, List<Response> pcResponses, List<DialogueEntry> visited) {
			if ((entry != null) && !visited.Contains(entry)) {
				visited.Add(entry);
				for (int i = (int) ConditionPriority.High; i >= 0; i--) {
					EvaluateLinksAtPriority((ConditionPriority) i, entry, npcResponses, pcResponses, visited);
					if ((npcResponses.Count > 0) || (pcResponses.Count > 0)) return;
				}
			}
		}
		
		private void EvaluateLinksAtPriority(ConditionPriority priority, DialogueEntry entry, List<Response> npcResponses, List<Response> pcResponses, List<DialogueEntry> visited) {
			if (entry != null) {
				foreach (Link link in entry.outgoingLinks) {
					DialogueEntry destinationEntry = database.GetDialogueEntry(link);
					if ((destinationEntry != null) && ((destinationEntry.conditionPriority == priority) || (link.priority == priority))) {
						CharacterType characterType = database.GetCharacterType(destinationEntry.ActorID);
						bool isValid = Lua.IsTrue(destinationEntry.conditionsString, DialogueDebug.LogInfo, allowLuaExceptions) &&
							((IsDialogueEntryValid == null) || IsDialogueEntryValid(destinationEntry));
						if (isValid) {
							
							// Condition is true (or blank), so add this link:
							if (destinationEntry.isGroup) {
								
								// For groups, evaluate their links (after running the group node's Lua code):
								if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Add Group ({1}): ID={2}:{3} '{4}'", new System.Object[] { DialogueDebug.Prefix, GetActorName(database.GetActor(destinationEntry.ActorID)), link.destinationConversationID, link.destinationDialogueID, destinationEntry.Title }));
								Lua.Run(destinationEntry.userScript, DialogueDebug.LogInfo, allowLuaExceptions);
								for (int i = (int) ConditionPriority.High; i >= 0; i--) {
									int originalResponseCount = npcResponses.Count + pcResponses.Count;;
									EvaluateLinksAtPriority((ConditionPriority) i, destinationEntry, npcResponses, pcResponses, visited);
									if ((npcResponses.Count + pcResponses.Count) > originalResponseCount) break;
								}
							} else {
								
								// For regular entries, just add them:
								if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Add Link ({1}): ID={2}:{3} '{4}'", new System.Object[] { DialogueDebug.Prefix, GetActorName(database.GetActor(destinationEntry.ActorID)), link.destinationConversationID, link.destinationDialogueID, GetLinkText(characterType, destinationEntry) }));
								if (characterType == CharacterType.NPC) {

									// Add NPC response:
									npcResponses.Add(new Response(FormattedText.Parse(destinationEntry.SubtitleText, database.emphasisSettings), destinationEntry));
								} else {

									// Add PC response, wrapping old responses in em tags if specified:
									string text = destinationEntry.ResponseButtonText;
									if (emTagForOldResponses != EmTag.None) {
										string simStatus = Lua.Run(string.Format("return Conversation[{0}].Dialog[{1}].SimStatus", new System.Object[] { destinationEntry.conversationID, destinationEntry.id })).AsString;
										bool isOldResponse = string.Equals(simStatus, "WasDisplayed");
										if (isOldResponse) text = string.Format("[em{0}]{1}[/em{0}]", (int) emTagForOldResponses, text);
									}
									pcResponses.Add(new Response(FormattedText.Parse(text, database.emphasisSettings), destinationEntry));
									DialogueLua.MarkDialogueEntryOffered(destinationEntry);
								}
							}
						} else {
							
							// Condition is false, so block or pass through according to destination entry's setting:
							if (LinkTools.IsPassthroughOnFalse(destinationEntry)) {
								if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Passthrough on False Link ({1}): ID={2}:{3} '{4}' Condition='{5}'", new System.Object[] { DialogueDebug.Prefix, GetActorName(database.GetActor(destinationEntry.ActorID)), link.destinationConversationID, link.destinationDialogueID, GetLinkText(characterType, destinationEntry), destinationEntry.conditionsString }));
								List<Response> linkNpcResponses = new List<Response>();
								List<Response> linkPcResponses = new List<Response>();
								EvaluateLinks(destinationEntry, linkNpcResponses, linkPcResponses, visited);
								npcResponses.AddRange(linkNpcResponses);
								pcResponses.AddRange(linkPcResponses);
							} else {
								if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Block on False Link ({1}): ID={2}:{3} '{4}' Condition='{5}'", new System.Object[] { DialogueDebug.Prefix, GetActorName(database.GetActor(destinationEntry.ActorID)), link.destinationConversationID, link.destinationDialogueID, GetLinkText(characterType, destinationEntry), destinationEntry.conditionsString }));
							}
						}
					}
				}
			}
		}

		private string GetActorName(Actor actor) {
			return (actor != null) ? actor.Name : "null";
		}
				
		private string GetLinkText(CharacterType characterType, DialogueEntry entry) {
			return (characterType == CharacterType.NPC) ? entry.SubtitleText : entry.ResponseButtonText;
		}
		
		private void SetParticipants(Conversation conversation, Transform actor, Transform conversant) {
			actorInfo = GetCharacterInfo(conversation.ActorID, actor);
			conversantInfo = GetCharacterInfo(conversation.ConversantID, conversant);
			DialogueLua.SetParticipants(actorInfo.Name, conversantInfo.Name);
		}
		
		/// <summary>
		/// Gets the character info for a character given its actor ID and transform.
		/// </summary>
		/// <returns>
		/// The character info.
		/// </returns>
		/// <param name='id'>
		/// The character's actor ID in the dialogue database.
		/// </param>
		/// <param name='character'>
		/// The transform of the character's GameObject.
		/// </param>
		public CharacterInfo GetCharacterInfo(int id, Transform character) {
			if (!characterInfoCache.ContainsKey(id)) {
				Actor actor = database.GetActor(id);
				string nameInDatabase = (actor != null) ? actor.Name : string.Empty;
				CharacterInfo characterInfo = new CharacterInfo(id, nameInDatabase, character, database.GetCharacterType(id), GetPortrait(character, actor));
				characterInfoCache.Add(id, characterInfo);
			}
			return characterInfoCache[id];
		}
		
		private CharacterInfo GetCharacterInfo(int id) {
			return GetCharacterInfo(id, GetCharacterTransform(id));
		}
		
		private Transform GetCharacterTransform(int id) {
			if (id == actorInfo.id) {
				return actorInfo.transform;
			} else if (id == conversantInfo.id) {
				return conversantInfo.transform;
			} else {
				return null;
			}
		}

		private Texture2D GetPortrait(Transform character, Actor actor) {
			Texture2D portrait = null;
			if (character != null) {
				OverrideActorName overrideActorName = character.GetComponentInChildren<OverrideActorName>();
				if (overrideActorName != null) portrait = GetPortraitByActorName(overrideActorName.GetOverrideName()); 
				if (portrait == null) portrait = GetPortraitByActorName(character.name);
			}
			if ((portrait == null) && (actor != null)) {
				portrait = GetPortraitByActorName(actor.Name);
				if (portrait == null) portrait = actor.portrait;
			}
			return portrait;
		}

		private Texture2D GetPortraitByActorName(string actorName) {
			// Also suppress logging for Lua return Actor[].Current_Portrait.
			var originalDebugLevel = DialogueDebug.Level;
			DialogueDebug.Level = DialogueDebug.DebugLevel.Warning;
			string textureName = DialogueLua.GetActorField(actorName, "Current Portrait").AsString;
			DialogueDebug.Level = originalDebugLevel;
			if (string.IsNullOrEmpty(textureName)) {
				return null;
			} else if (textureName.StartsWith("pic=")) {
				Actor actor = database.GetActor(actorName);
				if (actor == null) {
					return null;
				} else {
					return actor.GetPortraitTexture(Tools.StringToInt(textureName.Substring("pic=".Length)));
				}
			} else {
				return DialogueManager.LoadAsset(textureName) as Texture2D;
			}
		}

		/// <summary>
		/// Updates the actor portrait texture for any cached character info.
		/// </summary>
		/// <param name="actorName">Actor name.</param>
		/// <param name="portraitTexture">Portrait texture.</param>
		public void SetActorPortraitTexture(string actorName, Texture2D portraitTexture) {
			foreach (CharacterInfo characterInfo in characterInfoCache.Values) {
				if (string.Equals(characterInfo.Name, actorName) || string.Equals(characterInfo.nameInDatabase, actorName)) {
					characterInfo.portrait = portraitTexture;
				}
			}
		}

		/// <summary>
		/// Returns the name of the PC in this conversation.
		/// </summary>
		/// <returns>The PC name, or <c>null<c/c> if both are NPCs.</returns>
		public string GetPCName() {
			if (database.IsPlayerID(actorInfo.id)) {
				return actorInfo.Name;
			} else if (database.IsPlayerID(conversantInfo.id)) {
				return conversantInfo.Name;
			} else {
				return null;
			}
		}

		/// <summary>
		/// Returns the portrait texture of the PC in this conversation.
		/// </summary>
		/// <returns>The PC texture, or <c>null<c/c> if both are NPCs.</returns>
		public Texture2D GetPCTexture() {
			if (database.IsPlayerID(actorInfo.id)) {
				return actorInfo.portrait;
			} else if (database.IsPlayerID(conversantInfo.id)) {
				return conversantInfo.portrait;
			} else {
				return null;
			}
		}
		
	}
	
}
