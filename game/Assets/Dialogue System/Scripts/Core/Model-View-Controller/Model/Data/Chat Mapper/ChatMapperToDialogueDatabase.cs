using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This static utility class converts Chat Mapper projects to dialogue databases.
	/// </summary>
	public static class ChatMapperToDialogueDatabase {

		/// <summary>
		/// Converts a Chat Mapper project to a dialogue database.
		/// </summary>
		/// <returns>The dialogue database, or <c>null</c> if a conversation error occurred.</returns>
		/// <param name="chatMapperProject">Chat Mapper project.</param>
		public static DialogueDatabase ConvertToDialogueDatabase(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject) {
			DialogueDatabase database = ScriptableObject.CreateInstance<DialogueDatabase>();
			if (database == null) {
				if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Couldn't convert Chat Mapper project '{1}'.", new System.Object[] { DialogueDebug.Prefix, chatMapperProject.Title }));
			} else {
				ConvertProjectAttributes(chatMapperProject, database);
				ConvertActors(chatMapperProject, database);
				ConvertItems(chatMapperProject, database);
				ConvertLocations(chatMapperProject, database);
				ConvertVariables(chatMapperProject, database);
				ConvertConversations(chatMapperProject, database);
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Converted Chat Mapper project '{1}' containing {2} actors, {3} conversations, {4} items (quests), {5} variables, and {6} locations.", 
				                                                   new System.Object[] { DialogueDebug.Prefix, chatMapperProject.Title, database.actors.Count, database.conversations.Count, database.items.Count, database.variables.Count, database.locations.Count }));
			}
			return database;
		}

		private static void ConvertProjectAttributes(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database) {
			database.version = chatMapperProject.Version;
			database.author = chatMapperProject.Author;
			database.description = chatMapperProject.Description;
			database.emphasisSettings = new EmphasisSetting[4];
			database.emphasisSettings[0] = new EmphasisSetting(chatMapperProject.EmphasisColor1, chatMapperProject.EmphasisStyle1);
			database.emphasisSettings[1] = new EmphasisSetting(chatMapperProject.EmphasisColor2, chatMapperProject.EmphasisStyle2);
			database.emphasisSettings[2] = new EmphasisSetting(chatMapperProject.EmphasisColor3, chatMapperProject.EmphasisStyle3);
			database.emphasisSettings[3] = new EmphasisSetting(chatMapperProject.EmphasisColor4, chatMapperProject.EmphasisStyle4);
		}
		
		private static void ConvertActors(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database) {
			database.actors = new List<Actor>();
			foreach (var chatMapperActor in chatMapperProject.Assets.Actors) {
				database.actors.Add (new Actor (chatMapperActor));
			}
		}
		
		private static void ConvertItems(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database) {
			database.items = new List<Item>();
			foreach (var chatMapperItem in chatMapperProject.Assets.Items) {
				database.items.Add (new Item (chatMapperItem));
			}
		}
		
		private static void ConvertLocations(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database) {
			database.locations = new List<Location>();
			foreach (var chatMapperLocation in chatMapperProject.Assets.Locations) {
				database.locations.Add (new Location (chatMapperLocation));
			}
		}
		
		private static void ConvertVariables(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database) {
			database.variables = new List<Variable>();
			int id = 0;
			foreach (var chatMapperVariable in chatMapperProject.Assets.UserVariables) {
				Variable variable = new Variable(chatMapperVariable);
				variable.id = id;
				id++;
				database.variables.Add(variable);
			}
		}
		
		private static void ConvertConversations(PixelCrushers.DialogueSystem.ChatMapper.ChatMapperProject chatMapperProject, DialogueDatabase database) {
			database.conversations = new List<Conversation>();
			foreach (var chatMapperConversation in chatMapperProject.Assets.Conversations) {
				Conversation conversation = new Conversation(chatMapperConversation);
				SetConversationStartCutsceneToNone(conversation);
				foreach (DialogueEntry entry in conversation.dialogueEntries) {
					foreach (Link link in entry.outgoingLinks) {
						if (link.destinationConversationID == 0) link.destinationConversationID = conversation.id;
						if (link.originConversationID == 0) link.originConversationID = conversation.id;
					}
				}
				database.conversations.Add(conversation);
			}
		}
		
		private static void SetConversationStartCutsceneToNone(Conversation conversation) {
			DialogueEntry entry = conversation.GetFirstDialogueEntry();
			if (entry == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Conversation '{1}' doesn't have a START dialogue entry.", new System.Object[] { DialogueDebug.Prefix, conversation.Title }));
			} else {
				if (string.IsNullOrEmpty(entry.Sequence)) {
					if (Field.FieldExists(entry.fields, "Sequence")) {
						entry.Sequence = "None()";
					} else {
						entry.fields.Add(new Field("Sequence", "None()", FieldType.Text));
					}
				}
			}
		}
		
	}
}
