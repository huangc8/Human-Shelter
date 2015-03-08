/*
 * This functionality was moved into LuaScriptWizard.
 * 
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles the Script wizard.
	/// </summary>
	public partial class DialogueEditorWindow {

		private class ScriptItem {
			public WizardResourceType resourceType = WizardResourceType.Quest;
			public int questNamesIndex = 0;
			public int questEntryIndex = 0;
			public int variableNamesIndex = 0;
			public QuestState questState = QuestState.Unassigned;
			public string stringValue = string.Empty;
			public BooleanType booleanValue = BooleanType.True;
			public float floatValue = 0;
		}
		

		private DialogueEntry currentScriptWizardEntry = null;
		private List<ScriptItem> scriptItems = new List<ScriptItem>();
		private string[] scriptQuestEntryNames = new string[0];
		private string savedScriptString = string.Empty;

		private void ResetScriptWizard() {
			currentScriptWizardEntry = null;
			savedScriptString = string.Empty;
		}

		private void ToggleScriptWizard(DialogueEntry entry) {
			currentScriptWizardEntry = (currentScriptWizardEntry != entry) ? entry : null;
			savedScriptString = (currentScriptWizardEntry != null) ? currentScriptWizardEntry.userScript : string.Empty;
			if (currentScriptWizardEntry != null) currentScriptWizardEntry.userScript = string.Empty;
			scriptItems.Clear();
			RefreshWizardResources();
		}

		private bool IsScriptWizardOpen(DialogueEntry entry) {
			return (currentScriptWizardEntry == entry);
		}
		
		private void DrawScriptWizard() {
			EditorGUILayout.BeginVertical("button");
			
			EditorGUI.BeginChangeCheck();
			
			// Script items:
			ScriptItem itemToDelete = null;
			foreach (ScriptItem item in scriptItems) {
				DrawScriptItem(item, ref itemToDelete);
			}
			if (itemToDelete != null) scriptItems.Remove(itemToDelete);
			
			// "+", Revert, and Apply buttons:
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("+", "Add a new script action."), EditorStyles.miniButton, GUILayout.Width(22))) {
				scriptItems.Add(new ScriptItem());
			}

			if (EditorGUI.EndChangeCheck()) ApplyScriptWizard();
			
			GUILayout.FlexibleSpace();
			if (GUILayout.Button(new GUIContent("Revert", "Cancel these settings."), EditorStyles.miniButton, GUILayout.Width(48))) {
				CancelScriptWizard();
			}
			if (GUILayout.Button(new GUIContent("Apply", "Apply these settings"), EditorStyles.miniButton, GUILayout.Width(48))) {
				AcceptScriptWizard();
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.EndVertical();
		}
		
		private void DrawScriptItem(ScriptItem item, ref ScriptItem itemToDelete) {
			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField("Set", GUILayout.Width(32));
			WizardResourceType newResourceType = (WizardResourceType) EditorGUILayout.EnumPopup(item.resourceType, GUILayout.Width(96));
			if (newResourceType != item.resourceType) {
				item.resourceType = newResourceType;
				scriptQuestEntryNames = new string[0];
			}

			if (item.resourceType == WizardResourceType.Quest) {
				item.questNamesIndex = EditorGUILayout.Popup(item.questNamesIndex, questNames);
				EditorGUILayout.LabelField("to", GUILayout.Width(22));
				item.questState = (QuestState) EditorGUILayout.EnumPopup(item.questState, GUILayout.Width(96));
			} else if (item.resourceType == WizardResourceType.QuestEntry) {
				int newQuestNamesIndex  = EditorGUILayout.Popup(item.questNamesIndex, complexQuestNames);
				if (newQuestNamesIndex != item.questNamesIndex) {
					item.questNamesIndex = newQuestNamesIndex;
					scriptQuestEntryNames = new string[0];
				}
				if ((scriptQuestEntryNames.Length == 0) && (item.questNamesIndex < complexQuestNames.Length)) {
					scriptQuestEntryNames = GetQuestEntryNames(complexQuestNames[item.questNamesIndex]);
				}
				item.questEntryIndex = EditorGUILayout.Popup(item.questEntryIndex, scriptQuestEntryNames);
				EditorGUILayout.LabelField("to", GUILayout.Width(22));
				item.questState = (QuestState) EditorGUILayout.EnumPopup(item.questState, GUILayout.Width(96));
			} else {
				item.variableNamesIndex = EditorGUILayout.Popup(item.variableNamesIndex, variableNames);
				EditorGUILayout.LabelField("to", GUILayout.Width(22));
				switch (GetWizardVariableType(item.variableNamesIndex)) {
				case FieldType.Boolean: 
					item.booleanValue = (BooleanType) EditorGUILayout.EnumPopup(item.booleanValue);
					break;
				case FieldType.Number:
					item.floatValue = EditorGUILayout.FloatField(item.floatValue);
					break;
				default:
					item.stringValue = EditorGUILayout.TextField(item.stringValue);
					break;
				}
			}
			
			if (GUILayout.Button(new GUIContent("-", "Delete this script action."), EditorStyles.miniButton, GUILayout.Width(22))) {
				itemToDelete = item;
			}
			EditorGUILayout.EndHorizontal();
		}

		private void CancelScriptWizard() {
			if (currentScriptWizardEntry != null) currentScriptWizardEntry.userScript = savedScriptString;
			currentScriptWizardEntry = null;
		}

		private void AcceptScriptWizard() {
			currentScriptWizardEntry = null;
		}
		
		private void ApplyScriptWizard() {
			try {
				StringBuilder sb = new StringBuilder();
				string endText = (scriptItems.Count > 1) ? ";\n" : string.Empty;
				foreach (ScriptItem item in scriptItems) {
					if (item.resourceType == WizardResourceType.Quest) {
						string questName = GetWizardQuestName(questNames, item.questNamesIndex);
						sb.AppendFormat("Quest[\"{0}\"].State = \"{1}\"",
						                DialogueLua.StringToTableIndex(questName),
						                QuestLog.StateToString(item.questState));
					} else if (item.resourceType == WizardResourceType.QuestEntry) {
						string questName = GetWizardQuestName(complexQuestNames, item.questNamesIndex);
						sb.AppendFormat("Quest[\"{0}\"].Entry_{1}_State = \"{2}\"",
						                DialogueLua.StringToTableIndex(questName),
						                item.questEntryIndex + 1,
						                QuestLog.StateToString(item.questState));
					} else {
						string variableName = variableNames[item.variableNamesIndex];
						switch (GetWizardVariableType(item.variableNamesIndex)) {
						case FieldType.Boolean: 
							sb.AppendFormat("Variable[\"{0}\"] = {1}",
							                DialogueLua.StringToTableIndex(variableName),
							                (item.booleanValue == BooleanType.True) ? "true" : "false");
							break;
						case FieldType.Number:
							sb.AppendFormat("Variable[\"{0}\"] = {1}",
							                DialogueLua.StringToTableIndex(variableName),
							                item.floatValue);
							break;
						default:
							sb.AppendFormat("Variable[\"{0}\"] = \"{1}\"",
							                DialogueLua.StringToTableIndex(variableName),
							                item.stringValue);
							break;
						}
					}
					sb.AppendFormat(endText);
				}
				currentScriptWizardEntry.userScript = sb.ToString();
			} catch (Exception e) {
				Debug.LogError(string.Format("{0}: Internal error building script: {1}", DialogueDebug.Prefix, e.Message));
			}
			//--- Moved to AcceptScriptWizard: currentScriptWizardEntry = null;
		}
		
	}
	
}*/