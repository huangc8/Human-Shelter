using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem {

	[CustomEditor(typeof(AlertTrigger))]
	public class AlertTriggerEditor : Editor {

		public void OnEnable() {
			EditorTools.SetInitialDatabaseIfNull();
		}

		public override void OnInspectorGUI() {
			var trigger = target as AlertTrigger;
			if (trigger == null) return;

			trigger.trigger = (DialogueTriggerEvent) EditorGUILayout.EnumPopup(new GUIContent("Trigger", "The event that triggers the Lua code"), trigger.trigger);
			trigger.localizedTextTable = EditorGUILayout.ObjectField(new GUIContent("Localized Text Table", "Optional localized text table; if assigned, Message is the field in the table"), trigger.localizedTextTable, typeof(LocalizedTextTable), true) as LocalizedTextTable;
			trigger.message = EditorGUILayout.TextField(new GUIContent("Message", "The message to display, which may contain tags such as [var=varName]"), trigger.message);
			trigger.duration = EditorGUILayout.FloatField(new GUIContent("Duration", "The duration in seconds to display the message"), trigger.duration);
			trigger.once = EditorGUILayout.Toggle(new GUIContent("Only Once", "Only trigger once, then destroy this component"), trigger.once);

			EditorTools.DrawReferenceDatabase();
			EditorTools.DrawSerializedProperty(serializedObject, "condition");
		}

	}

}
