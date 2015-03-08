using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles drawing a single field.
	/// Drawing fields is complicated because a field can be one of several types.
	/// Actor fields need to provide a popup menu of the actors in the database,
	/// quest state fields need to provide a popup menu of the quest states, etc.
	/// </summary>
	public partial class DialogueEditorWindow {

		private List<string> textAreaFields = new List<string>() { "Description", "Success Description", "Failure Description" };		
		private static readonly string[] questStateStrings = { "(None)", "unassigned", "active", "success", "failure", "done", "abandoned" };

		private bool showStateFieldAsQuest = true;
		
		private void DrawFieldsSection(List<Field> fields) {
			EditorWindowTools.StartIndentedSection();
			DrawFieldsHeading();
			DrawFieldsContent(fields);
			EditorWindowTools.EndIndentedSection();
		}

		private void DrawFieldsHeading() {
			EditorGUILayout.BeginHorizontal();
			GUI.enabled = false;
			EditorGUILayout.TextField("Title");
			EditorGUILayout.TextField("Value");
			EditorGUILayout.TextField("Type");
			EditorGUI.BeginDisabledGroup(true);
			GUILayout.Button(new GUIContent("↑", "Move up"), EditorStyles.miniButton, GUILayout.Width(22));
			GUILayout.Button(new GUIContent("↓", "Move down"), EditorStyles.miniButton, GUILayout.Width(22));
			EditorGUI.EndDisabledGroup();
			GUILayout.Button(" ", "OL Minus", GUILayout.Width(16));
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();
		}
		
		private void DrawFieldsContent(List<Field> fields) {
			int fieldToRemove = -1;
			int fieldToMoveUp = -1;
			int fieldToMoveDown = -1;
			for (int i = 0; i < fields.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				if (IsTextAreaField(fields[i])) {
					DrawTextAreaFirstPart(fields[i]);
					DrawFieldManipulationButtons(i, fields.Count, fields[i].title, ref fieldToRemove, ref fieldToMoveUp, ref fieldToMoveDown);
					DrawTextAreaSecondPart(fields[i]);
				} else {
					DrawField(fields[i]);
					DrawFieldManipulationButtons(i, fields.Count, fields[i].title, ref fieldToRemove, ref fieldToMoveUp, ref fieldToMoveDown);
				}
				EditorGUILayout.EndHorizontal();
			}
			if (fieldToRemove >= 0) fields.RemoveAt(fieldToRemove);
			if (fieldToMoveUp >= 0) {
				var field = fields[fieldToMoveUp];
				fields.RemoveAt(fieldToMoveUp);
				fields.Insert(fieldToMoveUp - 1, field);
			}
			if (fieldToMoveDown >= 0) {
				var field = fields[fieldToMoveDown];
				fields.RemoveAt(fieldToMoveDown);
				fields.Insert(fieldToMoveDown + 1, field);
			}
		}

		private void DrawFieldManipulationButtons(int i, int fieldCount, string fieldTitle, ref int fieldToRemove, ref int fieldToMoveUp, ref int fieldToMoveDown) {
			// Up/down buttons:
			EditorGUI.BeginDisabledGroup(i == 0);
			if (GUILayout.Button(new GUIContent("↑", "Move up"), EditorStyles.miniButton, GUILayout.Width(22))) fieldToMoveUp = i;
			EditorGUI.EndDisabledGroup();
			EditorGUI.BeginDisabledGroup(i == fieldCount - 1);
			if (GUILayout.Button(new GUIContent("↓", "Move down"), EditorStyles.miniButton, GUILayout.Width(22))) fieldToMoveDown = i;
			EditorGUI.EndDisabledGroup();
			
			// Delete button:
			if (GUILayout.Button(new GUIContent(" ", string.Format("Delete field {0}.", fieldTitle)), "OL Minus", GUILayout.Width(16))) fieldToRemove = i;
			
		}
		
		private void DrawTextAreaFirstPart(Field field, bool isTitleEditable = true) {
			EditorGUI.BeginDisabledGroup(!isTitleEditable);
			field.title = EditorGUILayout.TextField(field.title);
			EditorGUI.EndDisabledGroup();
			GUI.enabled = false;
			EditorGUILayout.TextField(" ");
			GUI.enabled = true;
			DrawFieldType(field);
		}
		
		private void DrawTextAreaSecondPart(Field field) {
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			field.value = EditorGUILayout.TextArea(field.value);
		}
		
		private void DrawField(Field field, bool isTitleEditable = true) {
			EditorGUI.BeginDisabledGroup(!isTitleEditable);
			field.title = EditorGUILayout.TextField(field.title);
			EditorGUI.EndDisabledGroup();
			switch (field.type) {
			default:
			case FieldType.Text:
				field.value = IsQuestStateField(field) ? DrawQuestStateField(field.value) : EditorGUILayout.TextField(field.value);
				break;
			case FieldType.Files:
			case FieldType.Localization:
				field.value = EditorGUILayout.TextField(field.value);
				break;
			case FieldType.Number:
				field.value = EditorGUILayout.FloatField(StringToFloat(field.value, 0)).ToString();
				break;
			case FieldType.Boolean:
				field.value = EditorGUILayout.EnumPopup(StringToBooleanType(field.value)).ToString();
				break;
			case FieldType.Actor:
				field.value = DrawAssetPopup<Actor>(field.value, (database != null) ? database.actors : null, null);
				break;
			case FieldType.Item:
				field.value = DrawAssetPopup<Item>(field.value, (database != null) ? database.items : null, null);
				break;
			case FieldType.Location:
				field.value = DrawAssetPopup<Location>(field.value, (database != null) ? database.locations : null, null);
				break;
			}
			DrawFieldType(field);
		}
		
		private void DrawFieldType(Field field) {
			FieldType newFieldType = (FieldType) EditorGUILayout.EnumPopup(field.type);
			if (newFieldType != field.type) {
				field.type = newFieldType;
				field.value = string.Empty;
			}
		}
		
		private bool IsTextAreaField(Field field) {
			if (field == null || field.title == null) return false;
			return textAreaFields.Contains(field.title) || IsQuestEntryDescription(field);
		}

		private bool IsQuestEntryDescription(Field field) {
			if (field == null || field.title == null) return false;
			return field.title.StartsWith("Entry ") && !field.title.EndsWith(" State") && 
				!field.title.EndsWith(" Count") && !field.title.EndsWith(" End") && 
					!field.title.EndsWith(" ID");
		}
		
		private bool IsQuestStateField(Field field) {
			return showStateFieldAsQuest && (field != null) &&
				(string.Equals(field.title, "State") || 
				 (!string.IsNullOrEmpty(field.title) && field.title.EndsWith(" State")));
		}
		
		private string DrawQuestStateField(string value) {
			int index = 0;
			for (int i = 0; i < questStateStrings.Length; i++) {
				if (string.Equals(value, questStateStrings[i])) index = i;
			}
			int newIndex = EditorGUILayout.Popup(index, questStateStrings);
			return (newIndex == index)
				? value
				: ((newIndex == 0) ? string.Empty : questStateStrings[newIndex]);
		}
		
		private string DrawAssetPopup<T>(string value, List<T> assets, GUIContent assetLabel) where T : Asset {
			if (assets != null) {
				AssetList assetList = GetAssetList<T>(assets);
				int id = -1;
				int.TryParse(value, out id);
				int index = assetList.GetIndex(id);
				int newIndex;
				if ((assetLabel == null) || string.IsNullOrEmpty(assetLabel.text)) {
					newIndex = EditorGUILayout.Popup(index, assetList.names);
				} else {
					newIndex = EditorGUILayout.Popup(assetLabel, index, assetList.names);
				}
				return (newIndex != index) ? assetList.GetID(newIndex) : value;
			} else {
				EditorGUILayout.LabelField("(no database)");
				return value;
			}
		}
		
		private string DrawLabeledAssetPopup<T>(string label, string value, List<T> assets) where T : Asset {
			AssetList assetList = GetAssetList<T>(assets);
			int index = -1;
			int.TryParse(value, out index);
			int newIndex = EditorGUILayout.Popup(new GUIContent(label, string.Empty), index, assetList.names);
			return (newIndex != index) ? assetList.GetID(newIndex) : value;
		}
		
		private enum BooleanType { True, False }
		
		private static BooleanType StringToBooleanType(string s) {
			return (string.Compare(s, "true", System.StringComparison.OrdinalIgnoreCase) == 0) ? BooleanType.True : BooleanType.False;
		}
		
		private int StringToInt(string s, int defaultValue) {
			try {
				return System.Convert.ToInt32(s);
			} catch (FormatException) {
				return defaultValue;
			}
		}
		
		private float StringToFloat(string s, int defaultValue) {
			try {
				return (float) System.Convert.ToDouble(s);
			} catch (FormatException) {
				return defaultValue;
			}
		}
		
		private void EditTextField(List<Field> fields, string fieldTitle, string tooltip, bool isTextArea) {
			EditTextField(fields, fieldTitle, fieldTitle, tooltip, isTextArea, null);
		}
		
		private void EditTextField(List<Field> fields, string fieldTitle, string tooltip, bool isTextArea, List<Field> alreadyDrawn) {
			EditTextField(fields, fieldTitle, fieldTitle, tooltip, isTextArea, alreadyDrawn);
		}

		private void EditTextField(List<Field> fields, string fieldTitle, string label, string tooltip, bool isTextArea) {
			EditTextField(fields, fieldTitle, label, tooltip, isTextArea, null);
		}

		private void EditTextField(List<Field> fields, string fieldTitle, string label, string tooltip, bool isTextArea, List<Field> alreadyDrawn) {
			Field field = Field.Lookup(fields, fieldTitle);
			if (field == null) {
				field = new Field(fieldTitle, string.Empty, FieldType.Text);
				fields.Add(field);
			}
			if (isTextArea) {
				EditorGUILayout.LabelField(new GUIContent(label, tooltip));
				field.value = EditorGUILayout.TextArea(field.value);
			} else {
				field.value = EditorGUILayout.TextField(new GUIContent(label, tooltip), field.value);
			}
			if (alreadyDrawn != null) alreadyDrawn.Add(field);
		}

	}

}