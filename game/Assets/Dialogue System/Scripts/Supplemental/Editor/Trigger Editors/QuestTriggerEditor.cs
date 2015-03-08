using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem {

	[CustomEditor(typeof(QuestTrigger))]
	public class QuestTriggerEditor : Editor {

		private QuestPicker questPicker = null;
		private LuaScriptWizard luaScriptWizard = null;

		public void OnEnable() {
			var trigger = target as QuestTrigger;
			if (trigger != null) {
				if (EditorTools.selectedDatabase == null) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();
				luaScriptWizard = new LuaScriptWizard(EditorTools.selectedDatabase);
				questPicker = new QuestPicker(trigger.selectedDatabase, trigger.questName, trigger.useQuestNamePicker);
			}
		}
		
		public override void OnInspectorGUI() {
			var trigger = target as QuestTrigger;
			if (trigger == null) return;

			trigger.trigger = (DialogueTriggerEvent) EditorGUILayout.EnumPopup(new GUIContent("Trigger", "The event that triggers the quest change"), trigger.trigger);
			if (questPicker != null) {
				questPicker.Draw();
				trigger.questName = questPicker.currentQuest;
				trigger.useQuestNamePicker = questPicker.usePicker;
				trigger.selectedDatabase = questPicker.database;
				if (EditorTools.selectedDatabase == null) EditorTools.selectedDatabase = trigger.selectedDatabase;
			}
			trigger.questState = (QuestState) EditorGUILayout.EnumPopup(new GUIContent("Quest State", "The new quest state"), trigger.questState);

			EditorWindowTools.StartIndentedSection();
			
			// Lua code / wizard:
			if (EditorTools.selectedDatabase != luaScriptWizard.database) {
				luaScriptWizard.database = EditorTools.selectedDatabase;
				luaScriptWizard.RefreshWizardResources();
			}
			trigger.luaCode = luaScriptWizard.Draw(new GUIContent("Lua Code", "The Lua code to run when the condition is true"), trigger.luaCode);

			trigger.alertMessage = EditorGUILayout.TextField(new GUIContent("Alert Message", "Optional alert message to display when triggered"), trigger.alertMessage);
			trigger.localizedTextTable = EditorGUILayout.ObjectField(new GUIContent("Localized Text Table", "The localized text table to use for the alert message text"), trigger.localizedTextTable, typeof(LocalizedTextTable), true) as LocalizedTextTable;

			// Send Messages list:
			EditorTools.DrawSerializedProperty(serializedObject, "sendMessages");

			trigger.once = EditorGUILayout.Toggle(new GUIContent("Only Once", "Only trigger once, then destroy this component"), trigger.once);

			EditorWindowTools.EndIndentedSection();

			EditorTools.DrawSerializedProperty(serializedObject, "condition");

		}

	}

}
