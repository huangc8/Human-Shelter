using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This Lua script wizard is meant to be called from a custom editor's
	/// OnInspectorGUI() method.
	/// </summary>
	public class LuaScriptWizard : LuaWizardBase {

		private enum ValueSetMode {
			To,
			Add
		}

		private class ScriptItem {
			public WizardResourceType resourceType = WizardResourceType.Quest;
			public int questNamesIndex = 0;
			public int questEntryIndex = 0;
			public int variableNamesIndex = 0;
			public int actorNamesIndex = 0;
			public int actorFieldIndex = 0;
			public int itemNamesIndex = 0;
			public int itemFieldIndex = 0;
			public int locationNamesIndex = 0;
			public int locationFieldIndex = 0;
			public QuestState questState = QuestState.Unassigned;
			public string stringValue = string.Empty;
			public BooleanType booleanValue = BooleanType.True;
			public float floatValue = 0;
			public ValueSetMode valueSetMode = ValueSetMode.To;
		}
		
		private bool isOpen = false;
		private List<ScriptItem> scriptItems = new List<ScriptItem>();
		private string[] scriptQuestEntryNames = new string[0];
		private string savedLuaCode = string.Empty;

		public LuaScriptWizard(DialogueDatabase database) :
		base(database) {
		}
		
		public string Draw(GUIContent guiContent, string luaCode) {
			if (database == null) isOpen = false;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(guiContent);
			EditorGUI.BeginDisabledGroup(database == null);
			if (GUILayout.Button(new GUIContent("...", "Open Lua wizard."), EditorStyles.miniButton, GUILayout.Width(22))) {
				ToggleScriptWizard();
				if (isOpen) savedLuaCode = luaCode;
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			if (isOpen) {
				luaCode = DrawScriptWizard(luaCode);
			}

			luaCode = EditorGUILayout.TextArea(luaCode);

			return luaCode;
		}

		public void ResetWizard() {
			isOpen = false;
			savedLuaCode = string.Empty;
		}

		private void ToggleScriptWizard() {
			isOpen = !isOpen;
			scriptItems.Clear();
			RefreshWizardResources();
		}

		private string DrawScriptWizard(string luaCode) {
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
				luaCode = CancelScriptWizard();
			}
			if (GUILayout.Button(new GUIContent("Apply", "Apply these settings"), EditorStyles.miniButton, GUILayout.Width(48))) {
				luaCode = AcceptScriptWizard();
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.EndVertical();

			return luaCode;
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

				// Quest:
				item.questNamesIndex = EditorGUILayout.Popup(item.questNamesIndex, questNames);
				EditorGUILayout.LabelField("to", GUILayout.Width(22));
				item.questState = (QuestState) EditorGUILayout.EnumPopup(item.questState, GUILayout.Width(96));

			} else if (item.resourceType == WizardResourceType.QuestEntry) {

				// Quest Entry:
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

			} else if (item.resourceType == WizardResourceType.Variable) {

				// Variable:
				item.variableNamesIndex = EditorGUILayout.Popup(item.variableNamesIndex, variableNames);
				var variableType = GetWizardVariableType(item.variableNamesIndex);
				DrawValueSetMode(item, variableType);
				switch (variableType) {
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
			} else if (item.resourceType == WizardResourceType.Actor) {
				
				// Actor:
				item.actorNamesIndex = EditorGUILayout.Popup(item.actorNamesIndex, actorNames);
				item.actorFieldIndex = EditorGUILayout.Popup(item.actorFieldIndex, actorFieldNames);
				var actorFieldType = GetWizardActorFieldType(item.actorFieldIndex);
				DrawValueSetMode(item, actorFieldType);
				switch (actorFieldType) {
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

			} else if (item.resourceType == WizardResourceType.Item) {
				
				// Item:
				item.itemNamesIndex = EditorGUILayout.Popup(item.itemNamesIndex, itemNames);
				item.itemFieldIndex = EditorGUILayout.Popup(item.itemFieldIndex, itemFieldNames);
				var itemFieldType = GetWizardItemFieldType(item.itemFieldIndex);
				DrawValueSetMode(item, itemFieldType);
				switch (itemFieldType) {
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
				
			} else if (item.resourceType == WizardResourceType.Location) {
				
				// Location:
				item.locationNamesIndex = EditorGUILayout.Popup(item.locationNamesIndex, locationNames);
				item.locationFieldIndex = EditorGUILayout.Popup(item.locationFieldIndex, locationFieldNames);
				var locationFieldType = GetWizardLocationFieldType(item.locationFieldIndex);
				DrawValueSetMode(item, locationFieldType);
				switch (locationFieldType) {
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

		private void DrawValueSetMode(ScriptItem item, FieldType fieldType) {
			if (fieldType == FieldType.Number) {
				item.valueSetMode = (ValueSetMode) EditorGUILayout.EnumPopup(item.valueSetMode, GUILayout.Width(40));
			} else {
				EditorGUILayout.LabelField("to", GUILayout.Width(22));
			}
		}

		private string CancelScriptWizard() {
			isOpen = false;
			return savedLuaCode;
		}

		private string AcceptScriptWizard() {
			isOpen = false;
			return ApplyScriptWizard();
		}
		
		private string ApplyScriptWizard() {
			try {
				StringBuilder sb = new StringBuilder();
				string endText = (scriptItems.Count > 1) ? ";\n" : string.Empty;
				for (int i = 0; i < scriptItems.Count; i++) {
					var item = scriptItems[i];
					if (item.resourceType == WizardResourceType.Quest) {

						// Quest:
						string questName = GetWizardQuestName(questNames, item.questNamesIndex);
						sb.AppendFormat("Quest[\"{0}\"].State = \"{1}\"",
						                DialogueLua.StringToTableIndex(questName),
						                QuestLog.StateToString(item.questState));

					} else if (item.resourceType == WizardResourceType.QuestEntry) {

						// Quest Entry:
						string questName = GetWizardQuestName(complexQuestNames, item.questNamesIndex);
						sb.AppendFormat("Quest[\"{0}\"].Entry_{1}_State = \"{2}\"",
						                DialogueLua.StringToTableIndex(questName),
						                item.questEntryIndex + 1,
						                QuestLog.StateToString(item.questState));

					} else if (item.resourceType == WizardResourceType.Variable) {

						// Variable:
						string variableName = variableNames[item.variableNamesIndex];
						switch (GetWizardVariableType(item.variableNamesIndex)) {
						case FieldType.Boolean: 
							sb.AppendFormat("Variable[\"{0}\"] = {1}",
							                DialogueLua.StringToTableIndex(variableName),
							                (item.booleanValue == BooleanType.True) ? "true" : "false");
							break;
						case FieldType.Number:
							if (item.valueSetMode == ValueSetMode.To) {
								sb.AppendFormat("Variable[\"{0}\"] = {1}",
								                DialogueLua.StringToTableIndex(variableName),
								                item.floatValue);
							} else {
								sb.AppendFormat("Variable[\"{0}\"] = Variable[\"{0}\"] + {1}",
								                DialogueLua.StringToTableIndex(variableName),
								                item.floatValue);
							}
							break;
						default:
							sb.AppendFormat("Variable[\"{0}\"] = \"{1}\"",
							                DialogueLua.StringToTableIndex(variableName),
							                item.stringValue);
							break;
						}

					} else if (item.resourceType == WizardResourceType.Actor) {

						// Actor:
						if (item.actorNamesIndex < actorNames.Length) {
							var actorName = actorNames[item.actorNamesIndex];
							var actorFieldName = actorFieldNames[item.actorFieldIndex];
							var fieldType = GetWizardActorFieldType(item.actorFieldIndex);
							AppendFormat(sb, "Actor", actorName, actorFieldName, fieldType, item);
						}

					} else if (item.resourceType == WizardResourceType.Item) {

						// Item:
						if (item.itemNamesIndex < itemNames.Length) {
							var itemName = itemNames[item.itemNamesIndex];
							var itemFieldName = itemFieldNames[item.itemFieldIndex];
							var fieldType = GetWizardItemFieldType(item.itemFieldIndex);
							AppendFormat(sb, "Item", itemName, itemFieldName, fieldType, item);
						}

					} else if (item.resourceType == WizardResourceType.Location) {

						// Location:
						if (item.locationNamesIndex < locationNames.Length) {
							var locationName = locationNames[item.locationNamesIndex];
							var locationFieldName = locationFieldNames[item.locationFieldIndex];
							var fieldType = GetWizardLocationFieldType(item.locationFieldIndex);
							AppendFormat(sb, "Location", locationName, locationFieldName, fieldType, item);
						}
					}

					if (i < (scriptItems.Count - 1)) sb.AppendFormat(endText);
				}
				return sb.ToString();
			} catch (Exception e) {
				Debug.LogError(string.Format("{0}: Internal error building script: {1}", DialogueDebug.Prefix, e.Message));
				return savedLuaCode;
			}
		}

		private void AppendFormat(StringBuilder sb, string tableName, string elementName, string fieldName, FieldType fieldType, ScriptItem item) {
			switch (fieldType) {
			case FieldType.Boolean: 
				sb.AppendFormat("{0}[\"{1}\"].{2} = {3}",
				                tableName,
				                DialogueLua.StringToTableIndex(elementName),
				                DialogueLua.StringToTableIndex(fieldName),
				                (item.booleanValue == BooleanType.True) ? "true" : "false");
				break;
			case FieldType.Number:
				if (item.valueSetMode == ValueSetMode.To) {
					sb.AppendFormat("{0}[\"{1}\"].{2} = {3}",
					                tableName,
					                DialogueLua.StringToTableIndex(elementName),
					                DialogueLua.StringToTableIndex(fieldName),
					                item.floatValue);
				} else {
					sb.AppendFormat("{0}[\"{1}\"].{2} = {0}[\"{1}\"].{2} + {3}",
					                tableName,
					                DialogueLua.StringToTableIndex(elementName),
					                DialogueLua.StringToTableIndex(fieldName),
					                item.floatValue);
				}
				break;
			default:
				sb.AppendFormat("{0}[\"{1}\"].{2} = \"{3}\"",
				                tableName,
				                DialogueLua.StringToTableIndex(elementName),
				                DialogueLua.StringToTableIndex(fieldName),
				                item.stringValue);
				break;
			}
		}
		
	}
	
}