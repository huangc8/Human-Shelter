using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This Lua condition wizard is meant to be called from a custom editor's
	/// OnInspectorGUI() method. It includes an EditorGUILayout version (Draw(...))
	/// and an EditorGUI version (Draw(rect,...)).
	/// </summary>
	public class LuaConditionWizard : LuaWizardBase {

		private class ConditionItem {
			public WizardResourceType conditionType = WizardResourceType.Quest;
			public int questNamesIndex = 0;
			public int questEntryIndex = 0;
			public int variableNamesIndex = 0;
			public int actorNamesIndex = 0;
			public int actorFieldIndex = 0;
			public int itemNamesIndex = 0;
			public int itemFieldIndex = 0;
			public int locationNamesIndex = 0;
			public int locationFieldIndex = 0;
			public EqualityType equalityType = EqualityType.Is;
			public ComparisonType comparisonType = ComparisonType.Is;
			public QuestState questState = QuestState.Unassigned;
			public string stringValue = string.Empty;
			public BooleanType booleanValue = BooleanType.True;
			public float floatValue = 0;
		}

		public bool IsOpen { get { return isOpen; } }

		private bool isOpen = false;
		private List<ConditionItem> conditionItems = new List<ConditionItem>();
		private LogicalOperatorType conditionsLogicalOperator = LogicalOperatorType.All;
		private string[] conditionsQuestEntryNames = new string[0];
		private string savedLuaCode = string.Empty;

		public LuaConditionWizard(DialogueDatabase database) : base(database) {
		}

		public float GetHeight() {
			if (database == null) return 0;
			if (!isOpen) return EditorGUIUtility.singleLineHeight;
			return 4 + ((3 + conditionItems.Count) * (EditorGUIUtility.singleLineHeight + 2f));
		}

		public string Draw(GUIContent guiContent, string luaCode, bool showOpenCloseButton = true) {
			if (database == null) isOpen = false;
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(guiContent);
			if (showOpenCloseButton) {
				EditorGUI.BeginDisabledGroup(database == null);
				if (GUILayout.Button(new GUIContent("...", "Open Lua wizard."), EditorStyles.miniButton, GUILayout.Width(22))) {
					OpenWizard(luaCode);
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndHorizontal();
			
			if (isOpen) {
				luaCode = DrawConditionsWizard(luaCode);
			}
			
			luaCode = EditorGUILayout.TextArea(luaCode);
			
			return luaCode;
		}

		public void OpenWizard(string luaCode) {
			if (isOpen) return;
			ToggleConditionsWizard();
			if (isOpen) savedLuaCode = luaCode;
		}
		
		public void ResetWizard() {
			isOpen = false;
			savedLuaCode = string.Empty;
		}
		
		private void ToggleConditionsWizard() {
			isOpen = !isOpen && (database != null);
			conditionItems.Clear();
			RefreshWizardResources();
		}

		private string DrawConditionsWizard(string luaCode) {
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
				luaCode = CancelConditionsWizard();
			}
			if (GUILayout.Button(new GUIContent("Apply", "Apply these settings"), EditorStyles.miniButton, GUILayout.Width(48))) {
				luaCode = AcceptConditionsWizard();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			return luaCode;
		}

		private void DrawConditionItem(ConditionItem item, ref ConditionItem itemToDelete) {
			EditorGUILayout.BeginHorizontal();

			WizardResourceType newConditionType = (WizardResourceType) EditorGUILayout.EnumPopup(item.conditionType, GUILayout.Width(96));
			if (newConditionType != item.conditionType) {
				item.conditionType = newConditionType;
				conditionsQuestEntryNames = new string[0];
			}

			if (item.conditionType == WizardResourceType.Quest) {

				// Quest:
				item.questNamesIndex = EditorGUILayout.Popup(item.questNamesIndex, questNames);
				item.equalityType = (EqualityType) EditorGUILayout.EnumPopup(item.equalityType, GUILayout.Width(60));
				item.questState = (QuestState) EditorGUILayout.EnumPopup(item.questState, GUILayout.Width(96));

			} else if (item.conditionType == WizardResourceType.QuestEntry) {

				// Quest Entry:
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

			} else if (item.conditionType == WizardResourceType.Variable) {

				// Variable:
				item.variableNamesIndex = EditorGUILayout.Popup(item.variableNamesIndex, variableNames);
				DrawRightHand(item, GetWizardVariableType(item.variableNamesIndex));

			} else if (item.conditionType == WizardResourceType.Actor) {
				
				// Actor:
				item.actorNamesIndex = EditorGUILayout.Popup(item.actorNamesIndex, actorNames);
				item.actorFieldIndex = EditorGUILayout.Popup(item.actorFieldIndex, actorFieldNames);
				DrawRightHand(item, GetWizardActorFieldType(item.actorFieldIndex));

			} else if (item.conditionType == WizardResourceType.Item) {
				
				// Item:
				item.itemNamesIndex = EditorGUILayout.Popup(item.itemNamesIndex, itemNames);
				item.itemFieldIndex = EditorGUILayout.Popup(item.itemFieldIndex, itemFieldNames);
				DrawRightHand(item, GetWizardItemFieldType(item.itemFieldIndex));
				
			} else if (item.conditionType == WizardResourceType.Location) {
				
				// Location:
				item.locationNamesIndex = EditorGUILayout.Popup(item.locationNamesIndex, locationNames);
				item.locationFieldIndex = EditorGUILayout.Popup(item.locationFieldIndex, locationFieldNames);
				DrawRightHand(item, GetWizardLocationFieldType(item.locationFieldIndex));
				
			}

			if (GUILayout.Button(new GUIContent("-", "Delete this condition."), EditorStyles.miniButton, GUILayout.Width(22))) {
				itemToDelete = item;
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawRightHand(ConditionItem item, FieldType fieldType) {
			switch (fieldType) {
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

		private string CancelConditionsWizard() {
			isOpen = false;
			return savedLuaCode;
		}

		private string AcceptConditionsWizard() {
			isOpen = false;
			return ApplyConditionsWizard();
		}

		private string openParen = string.Empty;
		private string closeParen = string.Empty;
		
		private string ApplyConditionsWizard() {
			try {
				StringBuilder sb = new StringBuilder();
				string logicalOperator = GetLogicalOperatorText(conditionsLogicalOperator);
				openParen = (conditionItems.Count > 1) ? "(" : string.Empty;
				closeParen = (conditionItems.Count > 1) ? ")" : string.Empty;
				bool first = true;
				foreach (ConditionItem item in conditionItems) {
					if (!first) sb.AppendFormat(" {0} ", logicalOperator);
					first = false;
					if (item.conditionType == WizardResourceType.Quest) {

						// Quest:
						string questName = GetWizardQuestName(questNames, item.questNamesIndex);
						sb.AppendFormat("{0}Quest[\"{1}\"].State {2} \"{3}\"{4}",
						                openParen,
						                DialogueLua.StringToTableIndex(questName),
						                GetWizardEqualityText(item.equalityType),
						                QuestLog.StateToString(item.questState),
						                closeParen);

					} else if (item.conditionType == WizardResourceType.QuestEntry) {

						// Quest Entry:
						string questName = GetWizardQuestName(complexQuestNames, item.questNamesIndex);
						sb.AppendFormat("{0}Quest[\"{1}\"].Entry_{2}_State {3} \"{4}\"{5}",
						                openParen,
						                DialogueLua.StringToTableIndex(questName),
						                item.questEntryIndex + 1,
						                GetWizardEqualityText(item.equalityType),
						                QuestLog.StateToString(item.questState),
						                closeParen);

					} else if (item.conditionType == WizardResourceType.Variable) {

						// Variable:
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

					} else if (item.conditionType == WizardResourceType.Actor) {

						// Actor:
						if (item.actorNamesIndex < actorNames.Length) {
							var actorName = actorNames[item.actorNamesIndex];
							var actorField = actorFieldNames[item.actorFieldIndex];
							var actorFieldType = GetWizardActorFieldType(item.actorFieldIndex);
							AppendFormat(sb, "Actor", actorName, actorField, actorFieldType, item);
						} else {
							sb.Append("(true)");
						}

					} else if (item.conditionType == WizardResourceType.Item) {
						
						// Item:
						if (item.itemNamesIndex < itemNames.Length) {
							var itemName = itemNames[item.itemNamesIndex];
							var itemField = itemFieldNames[item.itemFieldIndex];
							var itemFieldType = GetWizardItemFieldType(item.itemFieldIndex);
							AppendFormat(sb, "Item", itemName, itemField, itemFieldType, item);
						} else {
							sb.Append("(true)");
						}
						
					} else if (item.conditionType == WizardResourceType.Location) {
						
						// Location:
						if (item.locationNamesIndex < locationNames.Length) {
							var locationName = locationNames[item.locationNamesIndex];
							var locationField = locationFieldNames[item.locationFieldIndex];
							var locationFieldType = GetWizardLocationFieldType(item.locationFieldIndex);
							AppendFormat(sb, "Location", locationName, locationField, locationFieldType, item);
						} else {
							sb.Append("(true)");
						}
						
					}
				}
				return sb.ToString();
			} catch (Exception e) {
				Debug.LogError(string.Format("{0}: Internal error building condition: {1}", DialogueDebug.Prefix, e.Message));
				return savedLuaCode;
			}
			//-- Moved to AcceptConditionsWizard: currentConditionsWizardEntry = null;
		}

		private void AppendFormat(StringBuilder sb, string tableName, string elementName, string fieldName, FieldType fieldType, ConditionItem item) {
			switch (fieldType) {
			case FieldType.Boolean: 
				sb.AppendFormat("{0}{1}[\"{2}\"].{3} {4} {5}{6}",
				                openParen,
				                tableName,
				                DialogueLua.StringToTableIndex(elementName),
				                DialogueLua.StringToTableIndex(fieldName),
				                GetWizardEqualityText(item.equalityType),
				                (item.booleanValue == BooleanType.True) ? "true" : "false",
				                closeParen);
				break;
			case FieldType.Number:
				sb.AppendFormat("{0}{1}[\"{2}\"].{3} {4} {5}{6}",
				                openParen,
				                tableName,
				                DialogueLua.StringToTableIndex(elementName),
				                DialogueLua.StringToTableIndex(fieldName),
				                GetWizardComparisonText(item.comparisonType),
				                item.floatValue,
				                closeParen);
				break;
			default:
				sb.AppendFormat("{0}{1}[\"{2}\"].{3} {4} \"{5}\"{6}",
				                openParen,
				                tableName,
				                DialogueLua.StringToTableIndex(elementName),
				                DialogueLua.StringToTableIndex(fieldName),
				                GetWizardEqualityText(item.equalityType),
				                item.stringValue,
				                closeParen);
				break;
			}
		}

		public string Draw(Rect position, GUIContent guiContent, string luaCode) {
			if (database == null) isOpen = false;

			// Title label:
			var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(rect, guiContent);
			
			if (isOpen) {

				// Lua wizard content:
				rect = new Rect(position.x + 16, position.y + EditorGUIUtility.singleLineHeight + 2f, position.width - 16, position.height - (2 * (EditorGUIUtility.singleLineHeight + 2f)));
				EditorGUI.BeginDisabledGroup(true);
				GUI.Button(rect, GUIContent.none);
				EditorGUI.EndDisabledGroup();

				var innerWidth = rect.width - 4;
				var innerHeight = rect.height - 4;
				var innerRect = new Rect(rect.x + 2, rect.y + 2, innerWidth, innerHeight);
				GUI.BeginGroup(innerRect);
				luaCode = DrawConditionsWizard(new Rect(0, 0, innerWidth, innerHeight), luaCode);
				GUI.EndGroup();
			}
			
			rect = new Rect(position.x, position.y + position.height - EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
			luaCode = EditorGUI.TextField(rect, luaCode);

			return luaCode;
		}
		
		private string DrawConditionsWizard(Rect position, string luaCode) {
			int originalIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			
			var rect = new Rect(position.x, position.y, position.width, position.height);
			var x = position.x;
			var y = position.y;

			EditorGUI.BeginChangeCheck();

			// Condition items:
			ConditionItem itemToDelete = null;
			foreach (ConditionItem item in conditionItems) {
				var innerHeight = EditorGUIUtility.singleLineHeight + 2;
				var innerRect = new Rect(x, y, position.width, innerHeight);
				GUI.BeginGroup(innerRect);
				DrawConditionItem(new Rect(0, 0, position.width, innerHeight), item, ref itemToDelete);
				GUI.EndGroup();
				y += EditorGUIUtility.singleLineHeight + 2;
			}
			if (itemToDelete != null) conditionItems.Remove(itemToDelete);

			// Bottom row (add condition item, logical operator, Revert & Apply buttons):
			x = 0;
			y = position.height - (EditorGUIUtility.singleLineHeight);
			rect = new Rect(x, y, 22, EditorGUIUtility.singleLineHeight);
			if (GUI.Button(rect, new GUIContent("+", "Add a new condition."), EditorStyles.miniButton)) {
				conditionItems.Add(new ConditionItem());
			}
			x += rect.width + 2;

			rect = new Rect(x, y, 74, EditorGUIUtility.singleLineHeight);
			conditionsLogicalOperator = (LogicalOperatorType) EditorGUI.EnumPopup(rect, conditionsLogicalOperator);
			x += rect.width + 2;

			rect = new Rect(x, y, 128, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(rect, "must be true.");

			if (EditorGUI.EndChangeCheck()) ApplyConditionsWizard();

			EditorGUI.BeginDisabledGroup(conditionItems.Count <= 0);
			rect = new Rect(position.width - 48 - 4 - 48, y, 48, EditorGUIUtility.singleLineHeight);
			if (GUI.Button(rect, new GUIContent("Revert", "Cancel these settings."), EditorStyles.miniButton)) {
				luaCode = CancelConditionsWizard();
			}
			rect = new Rect(position.width - 48, y, 48, EditorGUIUtility.singleLineHeight);
			if (GUI.Button(rect, new GUIContent("Apply", "Apply these settings"), EditorStyles.miniButton)) {
				luaCode = AcceptConditionsWizard();
			}
			EditorGUI.EndDisabledGroup();

			EditorGUI.indentLevel = originalIndentLevel;

			return luaCode;
		}

		private void DrawConditionItem(Rect position, ConditionItem item, ref ConditionItem itemToDelete) {
			const float typeWidth = 96;
			const float equalityWidth = 64;
			const float questStateWidth = 96;
			const float deleteButtonWidth = 22;

			int originalIndentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			
			var x = position.x;
			var rect = new Rect(x, 0, 96, EditorGUIUtility.singleLineHeight);
			x += rect.width + 2;
			WizardResourceType newConditionType = (WizardResourceType) EditorGUI.EnumPopup(rect, GUIContent.none, item.conditionType);
			if (newConditionType != item.conditionType) {
				item.conditionType = newConditionType;
				conditionsQuestEntryNames = new string[0];
			}

			if (item.conditionType == WizardResourceType.Quest) {
				
				// Quest:
				var questNameWidth = position.width - (typeWidth + equalityWidth + questStateWidth + deleteButtonWidth + 8);
				rect = new Rect(x, 0, questNameWidth, EditorGUIUtility.singleLineHeight);
				x += rect.width + 2;
				item.questNamesIndex = EditorGUI.Popup(rect, item.questNamesIndex, questNames);
				rect = new Rect(x, 0, equalityWidth, EditorGUIUtility.singleLineHeight);
				item.equalityType = (EqualityType) EditorGUI.EnumPopup(rect, item.equalityType);
				x += rect.width + 2;
				rect = new Rect(x, 0, questStateWidth, EditorGUIUtility.singleLineHeight);
				item.questState = (QuestState) EditorGUI.EnumPopup(rect, item.questState);
				x += rect.width + 2;

			} else if (item.conditionType == WizardResourceType.QuestEntry) {
				
				// Quest Entry:
				var freeWidth = position.width - (typeWidth + equalityWidth + questStateWidth + deleteButtonWidth + 10);
				rect = new Rect(x, 0, freeWidth / 2, EditorGUIUtility.singleLineHeight);
				int newQuestNamesIndex  = EditorGUI.Popup(rect, item.questNamesIndex, complexQuestNames);
				if (newQuestNamesIndex != item.questNamesIndex) {
					item.questNamesIndex = newQuestNamesIndex;
					conditionsQuestEntryNames = new string[0];
				}
				if ((conditionsQuestEntryNames.Length == 0) && (item.questNamesIndex < complexQuestNames.Length)) {
					conditionsQuestEntryNames = GetQuestEntryNames(complexQuestNames[item.questNamesIndex]);
				}
				x += rect.width + 2;
				rect = new Rect(x, 0, freeWidth / 2, EditorGUIUtility.singleLineHeight);
				item.questEntryIndex = EditorGUI.Popup(rect, item.questEntryIndex, conditionsQuestEntryNames);
				x += rect.width + 2;
				rect = new Rect(x, 0, equalityWidth, EditorGUIUtility.singleLineHeight);
				item.equalityType = (EqualityType) EditorGUI.EnumPopup(rect, item.equalityType);
				x += rect.width + 2;
				rect = new Rect(x, 0, questStateWidth, EditorGUIUtility.singleLineHeight);
				item.questState = (QuestState) EditorGUI.EnumPopup(rect, item.questState);
				x += rect.width + 2;

			} else if (item.conditionType == WizardResourceType.Variable) {
				
				// Variable:
				var freeWidth = position.width - (typeWidth + equalityWidth + deleteButtonWidth + 8);
				rect = new Rect(x, 0, freeWidth / 2, EditorGUIUtility.singleLineHeight);
				item.variableNamesIndex = EditorGUI.Popup(rect, item.variableNamesIndex, variableNames);
				x += rect.width + 2;
				rect = new Rect(x, 0, equalityWidth + 2 + (freeWidth / 2), EditorGUIUtility.singleLineHeight);
				DrawRightHand(rect, item, GetWizardVariableType(item.variableNamesIndex));
				x += rect.width + 2;

			} else if (item.conditionType == WizardResourceType.Actor) {
				
				// Actor:
				var freeWidth = position.width - (typeWidth + equalityWidth + deleteButtonWidth + 10);
				rect = new Rect(x, 0, freeWidth / 3, EditorGUIUtility.singleLineHeight);
				item.actorNamesIndex = EditorGUI.Popup(rect, item.actorNamesIndex, actorNames);
				x += rect.width + 2;
				rect = new Rect(x, 0, freeWidth / 3, EditorGUIUtility.singleLineHeight);
				item.actorFieldIndex = EditorGUI.Popup(rect, item.actorFieldIndex, actorFieldNames);
				x += rect.width + 2;
				rect = new Rect(x, 0, equalityWidth + 2 + (freeWidth / 3), EditorGUIUtility.singleLineHeight);
				DrawRightHand(rect, item, GetWizardActorFieldType(item.actorFieldIndex));
				x += rect.width + 2;

			} else if (item.conditionType == WizardResourceType.Item) {
				
				// Item:
				var freeWidth = position.width - (typeWidth + equalityWidth + deleteButtonWidth + 10);
				rect = new Rect(x, 0, freeWidth / 3, EditorGUIUtility.singleLineHeight);
				item.itemNamesIndex = EditorGUI.Popup(rect, item.itemNamesIndex, itemNames);
				x += rect.width + 2;
				rect = new Rect(x, 0, freeWidth / 3, EditorGUIUtility.singleLineHeight);
				item.itemFieldIndex = EditorGUI.Popup(rect, item.itemFieldIndex, itemFieldNames);
				x += rect.width + 2;
				rect = new Rect(x, 0, equalityWidth + 2 + (freeWidth / 3), EditorGUIUtility.singleLineHeight);
				DrawRightHand(rect, item, GetWizardItemFieldType(item.itemFieldIndex));
				x += rect.width + 2;

			} else if (item.conditionType == WizardResourceType.Location) {
				
				// Location:
				var freeWidth = position.width - (typeWidth + equalityWidth + deleteButtonWidth + 10);
				rect = new Rect(x, 0, freeWidth / 3, EditorGUIUtility.singleLineHeight);
				item.locationNamesIndex = EditorGUI.Popup(rect, item.locationNamesIndex, locationNames);
				x += rect.width + 2;
				rect = new Rect(x, 0, freeWidth / 3, EditorGUIUtility.singleLineHeight);
				item.locationFieldIndex = EditorGUI.Popup(rect, item.locationFieldIndex, locationFieldNames);
				x += rect.width + 2;
				rect = new Rect(x, 0, equalityWidth + 2 + (freeWidth / 3), EditorGUIUtility.singleLineHeight);
				DrawRightHand(rect, item, GetWizardLocationFieldType(item.locationFieldIndex));
				x += rect.width + 2;

			}

			// Delete button:
			rect = new Rect(position.width - deleteButtonWidth, 0, deleteButtonWidth, EditorGUIUtility.singleLineHeight);
			if (GUI.Button(rect, new GUIContent("-", "Delete this condition."), EditorStyles.miniButton)) {
				itemToDelete = item;
			}

			EditorGUI.indentLevel = originalIndentLevel;
		}
		
		private void DrawRightHand(Rect position, ConditionItem item, FieldType fieldType) {
			const float equalityWidth = 64;

			var rect1 = new Rect(position.x, 0, equalityWidth, EditorGUIUtility.singleLineHeight);
			var rect2 = new Rect(position.x + 2 + equalityWidth, 0, position.width - (equalityWidth + 2), EditorGUIUtility.singleLineHeight);
			switch (fieldType) {
			case FieldType.Boolean: 
				item.equalityType = (EqualityType) EditorGUI.EnumPopup(rect1, item.equalityType);
				item.booleanValue = (BooleanType) EditorGUI.EnumPopup(rect2, item.booleanValue);
				break;
			case FieldType.Number:
				item.comparisonType = (ComparisonType) EditorGUI.EnumPopup(rect1, item.comparisonType);
				item.floatValue = EditorGUI.FloatField(rect2, item.floatValue);
				break;
			default:
				item.equalityType = (EqualityType) EditorGUI.EnumPopup(rect1, item.equalityType);
				item.stringValue = EditorGUI.TextField(rect2, item.stringValue);
				break;
			}
		}

	}
	
}