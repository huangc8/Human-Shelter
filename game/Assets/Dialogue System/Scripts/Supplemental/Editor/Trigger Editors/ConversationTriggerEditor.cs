using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem {

	[CustomEditor(typeof(ConversationTrigger))]
	public class ConversationTriggerEditor : Editor {

		private ConversationPicker conversationPicker = null;

		public void OnEnable() {
			var trigger = target as ConversationTrigger;
			if (trigger != null) {
				conversationPicker = new ConversationPicker(trigger.selectedDatabase, trigger.conversation, trigger.useConversationTitlePicker);
			}
		}
		
		public override void OnInspectorGUI() {
			var trigger = target as ConversationTrigger;
			if (trigger == null) return;

			trigger.trigger = (DialogueTriggerEvent) EditorGUILayout.EnumPopup(new GUIContent("Trigger", "The event that triggers the conversation"), trigger.trigger);
			if (conversationPicker != null) {
				conversationPicker.Draw();
				trigger.conversation = conversationPicker.currentConversation;
				trigger.useConversationTitlePicker = conversationPicker.usePicker;
				trigger.selectedDatabase = conversationPicker.database;
				if (EditorTools.selectedDatabase == null) EditorTools.selectedDatabase = trigger.selectedDatabase;
			}
			trigger.actor = EditorGUILayout.ObjectField(new GUIContent("Actor", "The primary actor (e.g., player). If unassigned, the GameObject that triggered the conversation"), trigger.actor, typeof(Transform), true) as Transform;
			trigger.conversant = EditorGUILayout.ObjectField(new GUIContent("Conversant", "The other actor (e.g., NPC). If unassigned, this GameObject"), trigger.conversant, typeof(Transform), true) as Transform;
			trigger.once = EditorGUILayout.Toggle(new GUIContent("Only Once", "Only trigger once, then destroy this component"), trigger.once);
			trigger.skipIfNoValidEntries = EditorGUILayout.Toggle(new GUIContent("Skip If No Valid Entries", "Only trigger if at least one entry's Conditions are currently true"), trigger.skipIfNoValidEntries);
			trigger.stopConversationOnTriggerExit = EditorGUILayout.Toggle(new GUIContent("Stop On Trigger Exit", "Stop the triggered conversation if this GameObject receives OnTriggerExit"), trigger.stopConversationOnTriggerExit);
			EditorTools.DrawSerializedProperty(serializedObject, "condition");
		}

	}

}
