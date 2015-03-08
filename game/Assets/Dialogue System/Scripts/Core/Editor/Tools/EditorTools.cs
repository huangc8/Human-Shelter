using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	public static class EditorTools {

		public static DialogueDatabase selectedDatabase = null;

		public static DialogueDatabase FindInitialDatabase() {
			var dialogueSystemController = Object.FindObjectOfType<DialogueSystemController>();
			return (dialogueSystemController == null) ? null : dialogueSystemController.initialDatabase;
		}

		public static void SetInitialDatabaseIfNull() {
			if (selectedDatabase == null) {
				selectedDatabase = FindInitialDatabase();
			}
		}

		public static void DrawReferenceDatabase() {
			selectedDatabase = EditorGUILayout.ObjectField(new GUIContent("Reference Database", "Database to use for pop-up menus"), selectedDatabase, typeof(DialogueDatabase), true) as DialogueDatabase;
		}

		public static void DrawSerializedProperty(SerializedObject serializedObject, string propertyName) {
			serializedObject.Update();
			var property = serializedObject.FindProperty(propertyName);
			if (property == null) return;
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(property, true);
			if (EditorGUI.EndChangeCheck()) {
				serializedObject.ApplyModifiedProperties();
			}
		}

	}

}
