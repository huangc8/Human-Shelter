using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles the Conversations tab. If the user
	/// has selected the node editor (default), it uses the node editor part. Otherwise
	/// it uses the outline-style dialogue tree part.
	/// </summary>
	public partial class DialogueEditorWindow {

		private const int NoID = -1;

		private bool showNodeEditor = true;

		//private Conversation currentConversation = null;
		[SerializeField]
		private Conversation currentConversation;

		private bool conversationFieldsFoldout = false;
		private Field actorField = null;
		private Field conversantField = null;
		private int actorID = NoID;
		private int conversantID = NoID;
		private bool areParticipantsValid = false;
		private DialogueEntry startEntry = null;

		private void ResetConversationSection() {
			currentConversation = null;
			conversationFieldsFoldout = false;
			actorField = null;
			conversantField = null;
			areParticipantsValid = false;
			startEntry = null;
			ResetDialogueTreeSection();
			ResetConversationNodeSection();
		}

		private void OpenConversation(Conversation conversation) {
			ResetConversationSection();
			currentConversation = conversation;
			startEntry = GetOrCreateFirstDialogueEntry();
			if (showNodeEditor) CheckNodeArrangement();
		}

		private void DrawConversationSection() {
			if (showNodeEditor) {
				DrawConversationSectionNodeStyle();
			} else {
				DrawConversationSectionOutlineStyle();
			}
		}

		private void DrawConversationSectionOutlineStyle() {
			inspectorSelection = null;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Conversations", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			DrawOutlineEditorMenu();
			EditorGUILayout.EndHorizontal();
			DrawConversations();
		}

		private void DrawOutlineEditorMenu() {
			if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56))) {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("New Conversation"), false, AddNewConversationToOutlineEditor);
				menu.AddItem(new GUIContent("Sort/By Title"), false, SortConversationsByTitle);
				menu.AddItem(new GUIContent("Sort/By ID"), false, SortConversationsByID);
				menu.AddItem(new GUIContent("Search Bar"), isSearchBarOpen, ToggleDialogueTreeSearchBar);
				menu.AddItem(new GUIContent("Nodes"), false, ActivateNodeEditorMode);
				menu.ShowAsContext();
			}
		}

		private void AddNewConversationToOutlineEditor() {
			AddNewConversation();
		}
		
		private Conversation AddNewConversation() {
			Conversation newConversation = AddNewAsset<Conversation>(database.conversations);
			if (newConversation != null) OpenConversation(newConversation);
			return newConversation;
		}

		private void SortConversationsByTitle() {
			database.conversations.Sort((x, y) => x.Title.CompareTo(y.Title));
			ResetNodeEditorConversationList();
		}
		
		private void SortConversationsByID() {
			database.conversations.Sort((x, y) => x.id.CompareTo(y.id));
			ResetNodeEditorConversationList();
		}
		
		private void DrawConversations() {
			EditorWindowTools.StartIndentedSection();
			showStateFieldAsQuest = false;
			Conversation conversationToRemove = null;
			for (int index = 0; index < database.conversations.Count; index++) {
				Conversation conversation = database.conversations[index];
				EditorGUILayout.BeginHorizontal();
				bool isCurrentConversation = (conversation == currentConversation);
				bool foldout = isCurrentConversation;
				foldout = EditorGUILayout.Foldout(foldout, conversation.Title);
				if (GUILayout.Button(new GUIContent(" ", string.Format("Delete {0}.", conversation.Title)), "OL Minus", GUILayout.Width(16))) conversationToRemove = conversation;
				EditorGUILayout.EndHorizontal();
				if (foldout) {
					if (!isCurrentConversation) OpenConversation(conversation);
					DrawConversation();
				} else if (isCurrentConversation) {
					ResetConversationSection();
				}
			}
			if (conversationToRemove != null) {
				if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", conversationToRemove.Title), "Are you sure you want to delete this conversation?", "Delete", "Cancel")) {
					if (conversationToRemove == currentConversation) ResetConversationSection();
					database.conversations.Remove(conversationToRemove);
				}
			}
			EditorWindowTools.EndIndentedSection();
		}

		private void DrawConversation() {
			if (currentConversation == null) return;
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.BeginVertical("HelpBox");
			DrawConversationProperties();
			DrawConversationFieldsFoldout();
			DrawDialogueTreeFoldout();
			EditorGUILayout.EndVertical();
			EditorWindowTools.EndIndentedSection();
		}

		public bool DrawConversationProperties() {
			if (currentConversation == null) return false;
			int newID = EditorGUILayout.IntField(new GUIContent("ID", "Internal ID. Change at your own risk."), currentConversation.id);
			if (newID != currentConversation.id) SetNewConversationID(newID);

			bool changed = false;

			string newTitle = EditorGUILayout.TextField(new GUIContent("Title", "Conversation triggers reference conversations by this."), currentConversation.Title);
			if (!string.Equals(newTitle, currentConversation.Title)) {
				currentConversation.Title = newTitle;
				changed = true;
			}

			actorField = DrawConversationParticipant(new GUIContent("Actor", "Primary actor, usually the PC."), actorField);
			conversantField = DrawConversationParticipant(new GUIContent("Conversant", "Other actor, usually an NPC."), conversantField);

			if (changed) SetDatabaseDirty();
			return changed;
		}

		private Field DrawConversationParticipant(GUIContent fieldTitle, Field participantField) {
			EditorGUILayout.BeginHorizontal();
			if (participantField == null) participantField = LookupConversationParticipantField(fieldTitle.text);

			string originalValue = participantField.value;
			DrawField(participantField, false);
			if (!string.Equals(originalValue, participantField.value)) {
				int newParticipantID = Tools.StringToInt(participantField.value);
				UpdateConversationParticipant(Tools.StringToInt(originalValue), newParticipantID);
				startEntry = GetOrCreateFirstDialogueEntry();
				if (string.Equals(fieldTitle.text, "Actor")) {
					if (startEntry != null) startEntry.ActorID = newParticipantID;
					actorID = newParticipantID;
				} else {
					if (startEntry != null) startEntry.ConversantID = newParticipantID;
					conversantID = newParticipantID;
				}
				areParticipantsValid = false;
				ResetDialogueEntryText();
			}
			EditorGUILayout.EndHorizontal();
			return participantField;
		}

		private DialogueEntry GetOrCreateFirstDialogueEntry() {
			if (currentConversation.ActorID == 0) currentConversation.ActorID = GetFirstPCID();
			if (currentConversation.ConversantID == 0) currentConversation.ConversantID = GetFirstNPCID();
			DialogueEntry entry = currentConversation.GetFirstDialogueEntry();
			if (entry == null) {
				entry = CreateNewDialogueEntry("START");
				entry.ActorID = currentConversation.ActorID;
				entry.ConversantID = currentConversation.ConversantID;
			}
			if (entry.ActorID == 0) entry.ActorID = currentConversation.ActorID;
			if (entry.conversationID == 0) entry.ConversantID = currentConversation.ConversantID;
			return entry;
		}

		private int GetFirstPCID() {
			foreach (var actor in database.actors) {
				if (actor.IsPlayer) return actor.id;
			}
			int id = GetHighestActorID() + 1;
			database.actors.Add(template.CreateActor(id, "Player", true));
			return id;
		}

		private int GetFirstNPCID() {
			foreach (var actor in database.actors) {
				if (!actor.IsPlayer) return actor.id;
			}
			int id = GetHighestActorID() + 1;
			database.actors.Add(template.CreateActor(id, "NPC", false));
			return id;
		}

		private int GetHighestActorID() {
			int highestID = 0;
			foreach (var actor in database.actors) {
				highestID = Mathf.Max(highestID, actor.id);
			}
			return highestID;
		}

		private Field LookupConversationParticipantField(string fieldTitle) {
			Field participantField = Field.Lookup(currentConversation.fields, fieldTitle);
			if (participantField == null) {
				participantField = new Field(fieldTitle, NoIDString, FieldType.Actor);
				currentConversation.fields.Add(participantField);
			}
			return participantField;
		}
		
		private void UpdateConversationParticipant(int oldID, int newID) {
			if (newID != oldID) {
				foreach (var entry in currentConversation.dialogueEntries) {
					if (entry.ActorID == oldID) entry.ActorID = newID;
					if (entry.ConversantID == oldID) entry.ConversantID = newID;
				}
				ResetDialogueTreeCurrentEntryParticipants();
				ResetDialogueEntryText();
			}
		}

		private void SetNewConversationID(int newID) {
			foreach (var entry in currentConversation.dialogueEntries) {
				foreach (var link in entry.outgoingLinks) {
					if (link.originConversationID == currentConversation.id) link.originConversationID = newID;
					if (link.destinationConversationID == currentConversation.id) link.destinationConversationID = newID;
				}
			}
			currentConversation.id = newID;
		}

		public void DrawConversationFieldsFoldout() {
			EditorGUILayout.BeginHorizontal();
			conversationFieldsFoldout = EditorGUILayout.Foldout(conversationFieldsFoldout, "Fields");
			if (GUILayout.Button(new GUIContent(" ", "Add new field."), "OL Plus", GUILayout.Width(16))) currentConversation.fields.Add(new Field());
			EditorGUILayout.EndHorizontal();
			if (conversationFieldsFoldout) {
				if (actorID == NoID) actorID = currentConversation.ActorID;
				if (conversantID == NoID) conversantID = currentConversation.ConversantID;
				int oldActorID = actorID;
				int oldConversantID = conversantID;
				EditorGUI.BeginChangeCheck();
				DrawFieldsSection(currentConversation.fields);
				if (EditorGUI.EndChangeCheck()) {
					actorID = currentConversation.ActorID;
					conversantID = currentConversation.ConversantID;
					UpdateConversationParticipant(oldActorID, actorID);
					UpdateConversationParticipant(oldConversantID, conversantID);
				}
			}
		}

		private bool AreConversationParticipantsValid() {
			if (!areParticipantsValid) {
				areParticipantsValid = (database.GetActor(currentConversation.ActorID) != null) && (database.GetActor(currentConversation.ConversantID) != null);
			}
			return areParticipantsValid;
		}

	}

}