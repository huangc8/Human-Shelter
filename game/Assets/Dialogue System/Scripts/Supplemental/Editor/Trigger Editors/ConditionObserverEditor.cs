using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem {

	[CustomEditor(typeof(ConditionObserver))]
	public class ConditionObserverEditor : Editor {

		private bool actionFoldout = true;
		private LuaScriptWizard luaScriptWizard = null;
		private QuestPicker questPicker = null;

		public void OnEnable() {
			var trigger = target as ConditionObserver;
			if (trigger == null) return;
			if (EditorTools.selectedDatabase == null) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();
			luaScriptWizard = new LuaScriptWizard(EditorTools.selectedDatabase);
			questPicker = new QuestPicker(EditorTools.selectedDatabase, trigger.questName, trigger.useQuestNamePicker);
			questPicker.showReferenceDatabase = false;
		}
		
		public override void OnInspectorGUI() {
			var trigger = target as ConditionObserver;
			if (trigger == null) return;

			// Reference database:
			EditorTools.selectedDatabase = EditorGUILayout.ObjectField("Reference Database", EditorTools.selectedDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;

			// Frequency, once, observeGameObject:
			trigger.frequency = EditorGUILayout.FloatField(new GUIContent("Frequency", "Frequency in seconds between checks"), trigger.frequency);
			trigger.once = EditorGUILayout.Toggle(new GUIContent("Only Once", "Destroy after the condition is true"), trigger.once);
			trigger.observeGameObject = EditorGUILayout.ObjectField(new GUIContent("Observe GameObject", "Refer to this GameObject when evaluating the Condition"), trigger.observeGameObject, typeof(GameObject), false) as GameObject;

			// Condition:
			EditorTools.DrawSerializedProperty(serializedObject, "condition");

			actionFoldout = EditorGUILayout.Foldout(actionFoldout, "Action");
			if (!actionFoldout) return;

			EditorWindowTools.StartIndentedSection();

			// Lua code / wizard:
			if (EditorTools.selectedDatabase != luaScriptWizard.database) {
				luaScriptWizard.database = EditorTools.selectedDatabase;
				luaScriptWizard.RefreshWizardResources();
			}
			trigger.luaCode = luaScriptWizard.Draw(new GUIContent("Lua Code", "The Lua code to run when the condition is true"), trigger.luaCode);

			// Quest:
			if (EditorTools.selectedDatabase != questPicker.database) {
				questPicker.database = EditorTools.selectedDatabase;
				questPicker.UpdateTitles();
			}
			questPicker.Draw();
			trigger.questName = questPicker.currentQuest;
			trigger.useQuestNamePicker = questPicker.usePicker;
			trigger.questState = (QuestState) EditorGUILayout.EnumPopup(new GUIContent("Quest State", "The new quest state"), trigger.questState);

			// Alert message:
			trigger.alertMessage = EditorGUILayout.TextField(new GUIContent("Alert Message", "Optional alert message to display when triggered"), trigger.alertMessage);
			trigger.localizedTextTable = EditorGUILayout.ObjectField(new GUIContent("Localized Text Table", "The localized text table to use for the alert message text"), trigger.localizedTextTable, typeof(LocalizedTextTable), true) as LocalizedTextTable;

			// Send Messages list:
			EditorTools.DrawSerializedProperty(serializedObject, "sendMessages");

			EditorWindowTools.EndIndentedSection();
		}

	}

}
