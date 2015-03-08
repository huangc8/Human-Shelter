using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	[CustomPropertyDrawer(typeof(QuestPopupAttribute))]
	public class QuestPopupDrawer : PropertyDrawer {

		public QuestPicker questPicker = null;

		public override void OnGUI (Rect position, SerializedProperty prop, GUIContent label) {
			// Set up quest picker:
			if (questPicker == null) {
				questPicker = new QuestPicker(EditorTools.FindInitialDatabase(), prop.stringValue, true);
			}
			if (EditorTools.selectedDatabase != questPicker.database) {
				questPicker.database = EditorTools.selectedDatabase;
				questPicker.UpdateTitles();
			}

			// Set up property drawer:
			EditorGUI.BeginProperty(position, GUIContent.none, prop);
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			questPicker.Draw(position);

			EditorGUI.EndProperty();
		}
		
	}

}
