using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	[CustomPropertyDrawer(typeof(QuestCondition))]
	public class QuestConditionDrawer : PropertyDrawer {

		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			EditorGUI.BeginProperty(position, label, property);

			// Calculate rects:

			float halfWidth = position.width / 2;
			float questStateWidth = Mathf.Min(halfWidth, 160f) - 32f;
			float questNameWidth = position.width - questStateWidth - 16f;
			Rect questNameRect = new Rect(position.x + 16f, position.y, questNameWidth, position.height);
			Rect questStateRect = new Rect(position.x + questNameWidth + 16, position.y, questStateWidth, position.height);

			// Draw fields - pass GUIContent.none to each so they are drawn without labels
			var questName = property.FindPropertyRelative("questName");
			if (EditorTools.selectedDatabase == null) {
				EditorGUI.PropertyField(questNameRect, questName, GUIContent.none);
			} else {
				int questNameIndex;
				string[] questNames = GetQuestNames(questName.stringValue, out questNameIndex);
				int newQuestNameIndex = EditorGUI.Popup(questNameRect, questNameIndex, questNames);
				if (newQuestNameIndex != questNameIndex) {
					questName.stringValue = GetQuestName(questNames, newQuestNameIndex);
				}
			}

			var questState = property.FindPropertyRelative("questState");
			EditorGUI.PropertyField(questStateRect, questState, GUIContent.none);

			EditorGUI.EndProperty();
		}

		private string[] GetQuestNames(string currentQuestName, out int questNameIndex) {
			questNameIndex = -1;
			var database = EditorTools.selectedDatabase;
			if (database == null || database.items == null) {
				return new string[0];
			} else {
				List<string> questNames = new List<string>();
				foreach (var item in database.items) {
					if (!item.IsItem) {
						string questName = item.Name;
						if (string.Equals(currentQuestName, questName)) {
							questNameIndex = questNames.Count;
						}
						questNames.Add(questName);
					}
				}
				return questNames.ToArray();
			}
		}

		private string GetQuestName(string[] questNames, int questNameIndex) {
			return (0 <= questNameIndex && questNameIndex < questNames.Length) ? questNames[questNameIndex] : string.Empty;
		}

	}

}
