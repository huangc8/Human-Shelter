using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem {

	[CustomEditor(typeof(LuaTrigger))]
	public class LuaTriggerEditor : Editor {

		private LuaScriptWizard luaScriptWizard = null;

		public void OnEnable() {
			var trigger = target as LuaTrigger;
			if (trigger == null) return;
			if (EditorTools.selectedDatabase == null) EditorTools.selectedDatabase = EditorTools.FindInitialDatabase();
			luaScriptWizard = new LuaScriptWizard(EditorTools.selectedDatabase);
		}

		public override void OnInspectorGUI() {
			var trigger = target as LuaTrigger;
			if (trigger == null || luaScriptWizard == null) return;

			trigger.trigger = (DialogueTriggerEvent) EditorGUILayout.EnumPopup(new GUIContent("Trigger", "The event that triggers the Lua code"), trigger.trigger);

			var newDatabase = EditorGUILayout.ObjectField("Reference Database", luaScriptWizard.database, typeof(DialogueDatabase), false) as DialogueDatabase;
			if (newDatabase != luaScriptWizard.database) {
				EditorTools.selectedDatabase = newDatabase;
				luaScriptWizard.database = newDatabase;
				luaScriptWizard.RefreshWizardResources();
			}

			// Lua code / wizard:
			trigger.luaCode = luaScriptWizard.Draw(new GUIContent("Lua Code", "The Lua code to run"), trigger.luaCode);

			trigger.once = EditorGUILayout.Toggle(new GUIContent("Only Once", "Only trigger once, then destroy this component"), trigger.once);
			EditorTools.DrawSerializedProperty(serializedObject, "condition");
		}

	}

}
