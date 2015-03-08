using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles the top
	/// controls for the conversation node editor.
	/// </summary>
	public partial class DialogueEditorWindow {

		[SerializeField]
		private string[] conversationTitles;
		[SerializeField]
		private int conversationIndex;

		private DialogueEntry nodeToDrag = null;

		private void ResetConversationNodeEditor() {
			conversationTitles = null;
			conversationIndex = -1;
			ResetConversationNodeSection();
		}

		private void ResetConversationNodeSection() {
			isMakingLink = false;
			multinodeSelection.nodes.Clear();
		}

		private void ActivateOutlineMode() {
			showNodeEditor = false;
		}
		
		private void ActivateNodeEditorMode() {
			showNodeEditor = true;
			ResetNodeEditorConversationList();
			if (currentConversation != null) OpenConversation(currentConversation);
			isMakingLink = false;
		}

		private void ResetNodeEditorConversationList() {
			conversationTitles = GetConversationTitles();
			conversationIndex = GetCurrentConversationIndex();
		}

		private void DrawNodeEditorTopControls() {
			Rect rect = new Rect(0, 0f, 200, 60);
			GUI.Label(rect, "Conversations", "flow overlay header upper left");
			if (GUI.Button(new Rect(180, 2, 20, 58), new GUIContent("+", "Create a new conversation"), EditorStyles.boldLabel)) {
				AddNewConversationToNodeEditor();
			}
			EditorGUILayout.BeginHorizontal();
			DrawNodeEditorConversationPopup();
			GUILayout.FlexibleSpace();
			DrawNodeEditorMenu();
			EditorGUILayout.EndHorizontal();
		}

		private void AddNewConversationToNodeEditor() {
			AddNewConversation();
			ActivateNodeEditorMode();
		}

		private void DrawNodeEditorMenu() {
			if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56))) {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("New Conversation"), false, AddNewConversationToNodeEditor);
				menu.AddItem(new GUIContent("Sort/By Title"), false, SortConversationsByTitle);
				menu.AddItem(new GUIContent("Sort/By ID"), false, SortConversationsByID);
				menu.AddItem(new GUIContent("Actor Names"), showActorNames, ToggleShowActorNames);
				menu.AddItem(new GUIContent("Outline Mode"), false, ActivateOutlineMode);
				menu.ShowAsContext();
			}
		}

		private void ToggleShowActorNames() {
			showActorNames = !showActorNames;
			dialogueEntryNodeText.Clear();
		}

		private void DrawNodeEditorConversationPopup() {
			if (conversationTitles == null) conversationTitles = GetConversationTitles();
			int newIndex = EditorGUILayout.Popup(conversationIndex, conversationTitles, GUILayout.Width(160f));
			if (newIndex != conversationIndex) {
				conversationIndex = newIndex;
				OpenConversation(GetConversationByTitleIndex(conversationIndex));
				InitializeDialogueTree();
			}
		}

		private string[] GetConversationTitles() {
			List<string> titles = new List<string>();
			foreach (var conversation in database.conversations) {
				titles.Add(conversation.Title);
			}
			return titles.ToArray();
		}

		private int GetCurrentConversationIndex() {
			if (currentConversation != null) {
				if (conversationTitles == null) conversationTitles = GetConversationTitles();
				for (int i = 0; i < conversationTitles.Length; i++) {
					if (string.Equals(currentConversation.Title, conversationTitles[i])) return i;
				}
			}
			return -1;
		}

		private Conversation GetConversationByTitleIndex(int index) {
			if (conversationTitles == null) conversationTitles = GetConversationTitles();
			if (0 <= index && index < conversationTitles.Length) {
				return database.GetConversation(conversationTitles[index]);
			} else {
				return null;
			}
		}

		public void UpdateConversationTitles() {
			conversationTitles = GetConversationTitles();
		}

	}

}