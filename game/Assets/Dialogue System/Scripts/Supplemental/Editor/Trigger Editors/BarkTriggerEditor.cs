using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem {

	[CustomEditor(typeof(BarkTrigger))]
	public class BarkTriggerEditor : Editor {

		private ConversationPicker conversationPicker = null;

		public void OnEnable() {
			var trigger = target as BarkTrigger;
			if (trigger != null) {
				conversationPicker = new ConversationPicker(trigger.selectedDatabase, trigger.conversation, trigger.useConversationTitlePicker);
			}
		}
		
		public override void OnInspectorGUI() {
			var trigger = target as BarkTrigger;
			if (trigger == null) return;

			trigger.trigger = (DialogueTriggerEvent) EditorGUILayout.EnumPopup(new GUIContent("Trigger", "The event that triggers the bark"), trigger.trigger);
			if (conversationPicker != null) {
				conversationPicker.Draw();
				trigger.conversation = conversationPicker.currentConversation;
				trigger.useConversationTitlePicker = conversationPicker.usePicker;
				trigger.selectedDatabase = conversationPicker.database;
				if (EditorTools.selectedDatabase == null) EditorTools.selectedDatabase = trigger.selectedDatabase;
			}
			trigger.barkOrder = (BarkOrder) EditorGUILayout.EnumPopup(new GUIContent("Bark Order", "The order in which to bark dialogue entries"), trigger.barkOrder);
			trigger.conversant = EditorGUILayout.ObjectField(new GUIContent("Barker", "The actor speaking the bark. If unassigned, this GameObject"), trigger.conversant, typeof(Transform), true) as Transform;
			trigger.target = EditorGUILayout.ObjectField(new GUIContent("Target", "The GameObject being barked at"), trigger.target, typeof(Transform), true) as Transform;
			trigger.once = EditorGUILayout.Toggle(new GUIContent("Only Once", "Only trigger once, then destroy this component"), trigger.once);
			trigger.skipIfNoValidEntries = EditorGUILayout.Toggle(new GUIContent("Skip If No Valid Entries", "Only trigger if at least one entry's Conditions are currently true"), trigger.skipIfNoValidEntries);
			trigger.allowDuringConversations = EditorGUILayout.Toggle(new GUIContent("Allow During Conversations", "Allow barks during active conversations"), trigger.allowDuringConversations);
			trigger.cacheBarkLines = EditorGUILayout.Toggle(new GUIContent("Cache Bark Lines", "Cache all bark lines on first bark. Faster, but loses dynamic barks"), trigger.cacheBarkLines);
			EditorTools.DrawSerializedProperty(serializedObject, "condition");
		}

	}

}
