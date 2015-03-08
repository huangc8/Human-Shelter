using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem {

	[CustomEditor(typeof(SequenceTrigger))]
	public class SequenceTriggerEditor : Editor {

		public void OnEnable() {
			EditorTools.SetInitialDatabaseIfNull();
		}
		
		public override void OnInspectorGUI() {
			var trigger = target as SequenceTrigger;
			if (trigger == null) return;

			trigger.trigger = (DialogueTriggerEvent) EditorGUILayout.EnumPopup(new GUIContent("Trigger", "The event that triggers the sequence"), trigger.trigger);
			if (trigger.trigger == DialogueTriggerEvent.OnEnable || trigger.trigger == DialogueTriggerEvent.OnStart) {
				trigger.waitOneFrameOnStartOrEnable = EditorGUILayout.Toggle(new GUIContent("Wait 1 Frame", "Tick to wait one frame to allow other components to finish their OnStart/OnEnable"), trigger.waitOneFrameOnStartOrEnable);
			}
			EditorGUILayout.LabelField(new GUIContent("Sequence", "The sequence to play"));
			EditorWindowTools.StartIndentedSection();
			trigger.sequence = EditorGUILayout.TextArea(trigger.sequence);
			EditorWindowTools.EndIndentedSection();
			trigger.speaker = EditorGUILayout.ObjectField(new GUIContent("Speaker", "The GameObject referenced by 'speaker'. If unassigned, this GameObject"), trigger.speaker, typeof(Transform), true) as Transform;
			trigger.listener = EditorGUILayout.ObjectField(new GUIContent("Listener", "The GameObject referenced by 'listener'. If unassigned, the GameObject that triggered this sequence"), trigger.listener, typeof(Transform), true) as Transform;
			trigger.once = EditorGUILayout.Toggle(new GUIContent("Only Once", "Only trigger once, then destroy this component"), trigger.once);
			EditorTools.DrawReferenceDatabase();
			EditorTools.DrawSerializedProperty(serializedObject, "condition");
		}

	}

}
