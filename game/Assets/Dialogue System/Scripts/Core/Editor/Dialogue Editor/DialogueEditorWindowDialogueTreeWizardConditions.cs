/*
 * This functionality was moved into LuaConditionWizard.
 * 
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles the Conditions wizard.
	/// </summary>
	public partial class DialogueEditorWindow {

		private class ConditionItem {
			public WizardResourceType conditionType = WizardResourceType.Quest;
			public int questNamesIndex = 0;
			public int questEntryIndex = 0;
			public int variableNamesIndex = 0;
			public EqualityType equalityType = EqualityType.Is;
			public ComparisonType comparisonType = ComparisonType.Is;
			public QuestState questState = QuestState.Unassigned;
			public string stringValue = string.Empty;
			public BooleanType booleanValue = BooleanType.True;
			public float floatValue = 0;
		}

		private DialogueEntry currentConditionsWizardEntry = null;
		private List<ConditionItem> conditionItems = new List<ConditionItem>();
		private LogicalOperatorType conditionsLogicalOperator = LogicalOperatorType.All;
		private string[] conditionsQuestEntryNames = new string[0];
		private string savedConditionsString = string.Empty;

		private void ResetConditionsWizard() {
			currentConditionsWizardEntry = null;
			savedConditionsString = string.Empty;
		}
		
		private void ToggleConditionsWizard(DialogueEntry entry) {
			currentConditionsWizardEntry = (currentConditionsWizardEntry != entry) ? entry : null;
			savedConditionsString = (currentConditionsWizardEntry != null) ? currentConditionsWizardEntry.conditionsString : string.Empty;
			if (currentConditionsWizardEntry != null) currentConditionsWizardEntry.conditionsString = string.Empty;
			conditionItems.Clear();
			RefreshWizardResources();
		}

		private bool IsConditionsWizardOpen(DialogueEntry entry) {
			return (currentConditionsWizardEntry == entry);
		}

		private void DrawConditionsWizard() {
			EditorGUILayout.BeginVertical("button");

			EditorGUI.BeginChangeCheck();

			// Condition items:
			ConditionItem itemToDelete = null;
			foreach (ConditionItem item in conditionItems) {
				DrawConditionItem(item, ref itemToDelete);
			}
			if (itemToDelete != null) conditionItems.Remove(itemToDelete);

			// Logical operator (Any/All) and add new condition button:
			// Revert and Apply buttons:
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("+", "Add a new condition."), EditorStyles.miniButton, GUILayout.Width(22))) {
				conditionItems.Add(new ConditionItem());
			}
			conditionsLogicalOperator = (LogicalOperatorType) EditorGUILayout.EnumPopup(conditionsLogicalOperator, GUILayout.Width(48));
			EditorGUILayout.LabelField("must be true.");
			GUILayout.FlexibleSpace();
			GUILayout.FlexibleSpace();

			if (EditorGUI.EndChangeCheck()) ApplyConditionsWizard();

			if (GUILayout.Button(new GUIContent("Revert", "Cancel these settings."), EditorStyles.miniButton, GUILayout.Width(48))) {
				CancelConditionsWizard();
			}
			if (GUILayout.Button(new GUIContent("Apply", "Apply these settings"), EditorStyles.miniButton, GUILayout.Width(48))) {
				AcceptConditionsWizard();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
		}

		private void DrawConditionItem(ConditionItem item, ref ConditionItem itemToDelete) {
			EditorGUILayout.BeginHorizontal();

			WizardResourceType newConditionType = (WizardResourceType) EditorGUILayout.EnumPopup(item.conditionType, GUILayout.Width(96));
			if (newConditionType != item.conditionType) {
				item.conditionType = newConditionType;
				conditionsQuestEntryNames = new string[0];
			}

			if (item.conditionType == WizardResourceType.Quest) {
				item.questNamesIndex = EditorGUILayout.Popup(item.questNamesIndex, questNames);
				item.equalityType = (EqualityType) EditorGUILayout.EnumPopup(item.equalityType, GUILayout.Width(60));
				item.questState = (QuestState) EditorGUILayout.EnumPopup(item.questState, GUILayout.Width(96));
			} else if (item.conditionType == WizardResourceType.QuestEntry) {
				int newQuestNamesIndex  = EditorGUILayout.Popup(item.questNamesIndex, complexQuestNames);
				if (newQuestNamesIndex != item.questNamesIndex) {
					item.questNamesIndex = newQuestNamesIndex;
					conditionsQuestEntryNames = new string[0];
				}
				if ((conditionsQuestEntryNames.Length == 0) && (item.questNamesIndex < complexQuestNames.Length)) {
					conditionsQuestEntryNames = GetQuestEntryNames(complexQuestNames[item.questNamesIndex]);
				}
				item.questEntryIndex = EditorGUILayout.Popup(item.questEntryIndex, conditionsQuestEntryNames);
				item.equalityType = (EqualityType) EditorGUILayout.EnumPopup(item.equalityType, GUILayout.Width(60));
				item.questState = (QuestState) EditorGUILayout.EnumPopup(item.questState, GUILayout.Width(96));
			} else {
				item.variableNamesIndex = EditorGUILayout.Popup(item.variableNamesIndex, variableNames);
				switch (GetWizardVariableType(item.variableNamesIndex)) {
				case FieldType.Boolean: 
					item.equalityType = (EqualityType) EditorGUILayout.EnumPopup(item.equalityType, GUILayout.Width(60));
					item.booleanValue = (BooleanType) EditorGUILayout.EnumPopup(item.booleanValue);
					break;
				case FieldType.Number:
					item.comparisonType = (ComparisonType) EditorGUILayout.EnumPopup(item.comparisonType, GUILayout.Width(96));
					item.floatValue = EditorGUILayout.FloatField(item.floatValue);
					break;
				default:
					item.equalityType = (EqualityType) EditorGUILayout.EnumPopup(item.equalityType, GUILayout.Width(60));
					item.stringValue = EditorGUILayout.TextField(item.stringValue);
					break;
				}
			}

			if (GUILayout.Button(new GUIContent("-", "Delete this condition."), EditorStyles.miniButton, GUILayout.Width(22))) {
				itemToDelete = item;
			}
			EditorGUILayout.EndHorizontal();
		}

		private void CancelConditionsWizard() {
			if (currentConditionsWizardEntry != null) currentConditionsWizardEntry.conditionsString = savedConditionsString;
			currentConditionsWizardEntry = null;
		}

		private void AcceptConditionsWizard() {
			currentConditionsWizardEntry = null;
		}
		
		private void ApplyConditionsWizard() {
			try {
				StringBuilder sb = new StringBuilder();
				string logicalOperator = GetLogicalOperatorText(conditionsLogicalOperator);
				string openParen = (conditionItems.Count > 1) ? "(" : string.Empty;
				string closeParen = (conditionItems.Count > 1) ? ")" : string.Empty;
				bool first = true;
				foreach (ConditionItem item in conditionItems) {
					if (!first) sb.AppendFormat(" {0} ", logicalOperator);
					first = false;
					if (item.conditionType == WizardResourceType.Quest) {
						string questName = GetWizardQuestName(questNames, item.questNamesIndex);
						sb.AppendFormat("{0}Quest[\"{1}\"].State {2} \"{3}\"{4}",
						                openParen,
						                DialogueLua.StringToTableIndex(questName),
						                GetWizardEqualityText(item.equalityType),
						                QuestLog.StateToString(item.questState),
						                closeParen);
					} else if (item.conditionType == WizardResourceType.QuestEntry) {
						string questName = GetWizardQuestName(complexQuestNames, item.questNamesIndex);
						sb.AppendFormat("{0}Quest[\"{1}\"].Entry_{2}_State {3} \"{4}\"{5}",
						                openParen,
						                DialogueLua.StringToTableIndex(questName),
						                item.questEntryIndex + 1,
						                GetWizardEqualityText(item.equalityType),
						                QuestLog.StateToString(item.questState),
						                closeParen);
					} else {
						string variableName = variableNames[item.variableNamesIndex];
						switch (GetWizardVariableType(item.variableNamesIndex)) {
						case FieldType.Boolean: 
							sb.AppendFormat("{0}Variable[\"{1}\"] {2} {3}{4}",
							                openParen,
							                DialogueLua.StringToTableIndex(variableName),
							                GetWizardEqualityText(item.equalityType),
							                (item.booleanValue == BooleanType.True) ? "true" : "false",
							                closeParen);
							break;
						case FieldType.Number:
							sb.AppendFormat("{0}Variable[\"{1}\"] {2} {3}{4}",
							                openParen,
							                DialogueLua.StringToTableIndex(variableName),
							                GetWizardComparisonText(item.comparisonType),
							                item.floatValue,
							                closeParen);
							break;
						default:
							sb.AppendFormat("{0}Variable[\"{1}\"] {2} \"{3}\"{4}",
							                openParen,
							                DialogueLua.StringToTableIndex(variableName),
							                GetWizardEqualityText(item.equalityType),
							                item.stringValue,
							                closeParen);
							break;
						}
					}
				}
				currentConditionsWizardEntry.conditionsString = sb.ToString();
			} catch (Exception e) {
				Debug.LogError(string.Format("{0}: Internal error building condition: {1}", DialogueDebug.Prefix, e.Message));
			}
			//-- Moved to AcceptConditionsWizard: currentConditionsWizardEntry = null;
		}

	}
	
}*/