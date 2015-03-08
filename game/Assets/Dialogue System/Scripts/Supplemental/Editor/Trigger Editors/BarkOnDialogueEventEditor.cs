using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem {

	[CustomEditor(typeof(BarkOnDialogueEvent))]
	public class BarkOnDialogueEventEditor : Editor {

		public void OnEnable() {
			var trigger = target as BarkOnDialogueEvent;
			if (trigger == null) return;
			trigger.selectedDatabase = trigger.selectedDatabase ?? EditorTools.FindInitialDatabase();
		}
		
		public override void OnInspectorGUI() {
			var trigger = target as BarkOnDialogueEvent;
			if (trigger == null) return;

			trigger.trigger = (DialogueEvent) EditorGUILayout.EnumPopup(new GUIContent("Trigger", "The dialogue event that triggers barks"), trigger.trigger);

			var newDatabase = EditorGUILayout.ObjectField("Reference Database", trigger.selectedDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
			if (newDatabase != trigger.selectedDatabase) {
				trigger.selectedDatabase = newDatabase;
			}
			EditorTools.selectedDatabase = trigger.selectedDatabase;

			trigger.barkOrder = (BarkOrder) EditorGUILayout.EnumPopup(new GUIContent("Bark Order", "The order in which to bark dialogue entries"), trigger.barkOrder);
			trigger.once = EditorGUILayout.Toggle(new GUIContent("Only Once", "Only trigger once, then destroy this component"), trigger.once);

			serializedObject.Update();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("onStart"), true);
			EditorGUILayout.PropertyField(serializedObject.FindProperty("onEnd"), true);
			serializedObject.ApplyModifiedProperties();
		}

	}

}
