/*
 * This functionality was moved into LuaWizardBase.
 * 
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window contains common code for the 
	/// Conditions and Script wizards.
	/// </summary>
	public partial class DialogueEditorWindow {

		private enum WizardResourceType { Quest, QuestEntry, Variable }

		private enum EqualityType { Is, IsNot }

		private enum ComparisonType { Is, IsNot, Less, Greater, LessEqual, GreaterEqual }

		private enum LogicalOperatorType { All, Any }

		private string[] questNames = new string[0];

		private string[] complexQuestNames = new string[0];

		private string[] variableNames = new string[0];

		private FieldType[] variableTypes = new FieldType[0];

		private void RefreshWizardResources() {
			RefreshQuestNames();
			RefreshVariableNames();
		}
		
		private void RefreshQuestNames() {
			List<string> questList = new List<string>();
			List<string> complexQuestList = new List<string>();
			foreach (Item item in database.items) {
				if (!item.IsItem) {
					questList.Add(item.Name);
					if (item.LookupInt("Entry Count") > 0) {
						complexQuestList.Add(item.Name);
					}
				}
			}
			questNames = questList.ToArray();
			complexQuestNames = complexQuestList.ToArray();
		}

		private void RefreshVariableNames() {
			List<string> nameList = new List<string>();
			List<FieldType> typeList = new List<FieldType>();
			database.variables.ForEach(variable => { nameList.Add(variable.Name); typeList.Add(variable.Type); });
			variableNames = nameList.ToArray();
			variableTypes = typeList.ToArray();
		}

		private string[] GetQuestEntryNames(string questName) {
			List<string> questEntryList = new List<string>();
			Item item = database.GetItem(questName);
			if (item != null) {
				int entryCount = item.LookupInt("Entry Count");
				if (entryCount > 0) {
					for (int i = 1; i <= entryCount; i++) {
						string entryText = item.LookupValue(string.Format("Entry {0}", i)) ?? string.Empty;
						string s = string.Format("{0}. {1}", 
						                         i, 
						                         ((entryText.Length < 20)
						                         	? entryText
						 							: entryText.Substring(0, 17) + "..."));
						questEntryList.Add(s);
					}
				}
			}
			return questEntryList.ToArray();
		}

		private string GetWizardQuestName(string[] questNames, int index) {
			return (0 <= index && index < questNames.Length) ? questNames[index] : "UNDEFINED";
		}

		private string GetLogicalOperatorText(LogicalOperatorType logicalOperator) {
			return (logicalOperator == LogicalOperatorType.All) ? "and" : "or";
		}

		private FieldType GetWizardVariableType(int variableIndex) {
			return (0 <= variableIndex && variableIndex < variableTypes.Length) ? variableTypes[variableIndex] : FieldType.Text;
		}

		private string GetWizardEqualityText(EqualityType equalityType) {
			return (equalityType == EqualityType.Is) ? "==" : "~=";
		}

		private string GetWizardComparisonText(ComparisonType comparisonType) {
			switch (comparisonType) {
			case ComparisonType.Is:
				return "==";
			case ComparisonType.IsNot:
				return "~=";
			case ComparisonType.Less:
				return "<";
			case ComparisonType.LessEqual:
				return "<=";
			case ComparisonType.Greater:
				return ">";
			case ComparisonType.GreaterEqual:
				return ">=";
			default:
				return "==";
			}
		}

	}
	
}*/