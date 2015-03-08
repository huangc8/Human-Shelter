using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	public class ConversationPicker {

		public DialogueDatabase database = null;
		public string currentConversation = string.Empty;
		public bool usePicker = false;

		private string[] titles = null;
		private int currentIndex = -1;

		public ConversationPicker(DialogueDatabase database, string currentConversation, bool usePicker) {
			this.database = database ?? EditorTools.FindInitialDatabase();
			this.currentConversation = currentConversation;
			this.usePicker = usePicker;
			UpdateTitles();
			bool currentConversationIsInDatabase = (database != null) || (currentIndex >= 0);
			if (usePicker && !string.IsNullOrEmpty(currentConversation) && !currentConversationIsInDatabase) {
				this.usePicker = false;
			}
		}

		public void UpdateTitles() {
			currentIndex = -1;
			if (database == null || database.conversations == null) {
				titles = new string[0];
			} else {
				titles = new string[database.conversations.Count];
				for (int i = 0; i < database.conversations.Count; i++) {
					titles[i] = database.conversations[i].Title;
					if (string.Equals(currentConversation, titles[i])) {
						currentIndex = i;
					}
				}
			}
		}

		public void Draw() {
			EditorGUILayout.BeginHorizontal();
			if (usePicker) {
				var newDatabase = EditorGUILayout.ObjectField("Reference Database", database, typeof(DialogueDatabase), false) as DialogueDatabase;
				if (newDatabase != database) {
					database = newDatabase;
					UpdateTitles();
				}
			} else {
				currentConversation = EditorGUILayout.TextField("Conversation", currentConversation);
			}
			var newToggleValue = EditorGUILayout.Toggle(usePicker, EditorStyles.radioButton, GUILayout.Width(20));
			if (newToggleValue != usePicker) {
				usePicker = newToggleValue;
				if (usePicker && (database == null)) database = EditorTools.FindInitialDatabase();
				UpdateTitles();
			}
			EditorGUILayout.EndHorizontal();
			if (usePicker) {
				currentIndex = EditorGUILayout.Popup("Conversation", currentIndex, titles);
				if (0 <= currentIndex && currentIndex < titles.Length) currentConversation = titles[currentIndex];
			}
		}

	}

}
