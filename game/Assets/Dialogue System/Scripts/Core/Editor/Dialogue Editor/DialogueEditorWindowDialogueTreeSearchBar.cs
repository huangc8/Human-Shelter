using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window provides a Search bar in the 
	/// outline-style dialogue tree editor.
	/// </summary>
	public partial class DialogueEditorWindow {

		private bool isSearchBarOpen = false;
		private string searchString = string.Empty;

		private bool IsSearchBarVisible { get { return isSearchBarOpen && (toolbar.Current == Toolbar.Tab.Conversations) && !showNodeEditor; } }

		private void ToggleDialogueTreeSearchBar() {
			isSearchBarOpen = !isSearchBarOpen;
		}

		private void DrawDialogueTreeSearchBar() {
			EditorWindowTools.DrawHorizontalLine();
			EditorGUILayout.BeginHorizontal();
			GUI.SetNextControlName("SearchEmptyLabel");
			EditorGUILayout.LabelField(string.Empty, GUILayout.Width(8));
			searchString = EditorGUILayout.TextField("Search", searchString, "ToolbarSeachTextField");
			if (GUILayout.Button("Clear", "ToolbarSeachCancelButton")) {
				searchString = string.Empty;
				GUI.FocusControl("SearchEmptyLabel"); // Need to deselect search field to clear text field's display.
			}
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(searchString));
			if (GUILayout.Button("↓", EditorStyles.miniButtonLeft, GUILayout.Width(22))) SearchDialogueTree(1);
			if (GUILayout.Button("↑", EditorStyles.miniButtonMid, GUILayout.Width(22))) SearchDialogueTree(-1);
			EditorGUI.EndDisabledGroup();
			if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.Width(22))) isSearchBarOpen = false;
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField(string.Empty, GUILayout.Height(1));
		}

		private void SearchDialogueTree(int direction) {
			// Assumes dialogue tree has already been built. Otherwise just exits.
			DialogueEntry entry = (currentEntry != null) 
				? currentEntry 
				: ((dialogueTree != null) ? dialogueTree.entry : null);
			if (entry == null) return;
			int start = GetValidSearchIndex(currentConversation.dialogueEntries.IndexOf(entry));
			int current = GetValidSearchIndex(start + direction);
			while (current != start) {
				if (ContainsSearchString(currentConversation.dialogueEntries[current])) {
					currentEntry = currentConversation.dialogueEntries[current];
					return;
				} else {
					current = GetValidSearchIndex(current + direction);
				}
			}
		}

		private int GetValidSearchIndex(int index) {
			if (index < 0) {
				return currentConversation.dialogueEntries.Count - 1;
			} else if (index >= currentConversation.dialogueEntries.Count) {
				return 0;
			} else {
				return index;
			}
		}

		private bool ContainsSearchString(DialogueEntry entry) {
			foreach (var field in entry.fields) {
				if (ContainsSearchStringCaseInsensitive(field.value)) return true;
			}
			if (ContainsSearchStringCaseInsensitive(entry.conditionsString)) return true;
			if (ContainsSearchStringCaseInsensitive(entry.userScript)) return true;
			return false;
		}

		private bool ContainsSearchStringCaseInsensitive(string s) {
			return !string.IsNullOrEmpty(s) && (CultureInfo.InvariantCulture.CompareInfo.IndexOf(s, searchString, CompareOptions.IgnoreCase) >= 0);

		}

	}
	
}