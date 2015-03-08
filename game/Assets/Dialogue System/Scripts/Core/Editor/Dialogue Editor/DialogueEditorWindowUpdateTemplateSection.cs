using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window implements UpdateTemplateFromAssets().
	/// </summary>
	public partial class DialogueEditorWindow {

		private void ConfirmUpdateFromAssets() {
			if (EditorUtility.DisplayDialog("Update template?", "This will update the template from the content of all assets, and then apply that template to all assets. You cannot undo this action.", "Update", "Cancel")) {
				UpdateTemplateFromAssets();
				Debug.Log(string.Format("{0}: Dialogue Editor template now contains all fields listed in any dialogue database asset, and those assets now have all fields listed in the template.", DialogueDebug.Prefix));
			}
		}

		private void UpdateTemplateFromAssets() {
			NormalizeActors();
			NormalizeItems();
			NormalizeLocations();
			NormalizeVariables();
			NormalizeConversations();
			NormalizeDialogueEntries();
		}

		private void NormalizeActors() {
			NormalizeAssets<Actor>(database.actors, template.actorFields);
		}
		
		private void NormalizeItems() {
			AddMissingFieldsToTemplate(template.questFields, template.itemFields);
			NormalizeAssets<Item>(database.items, template.itemFields);
		}
		
		private void NormalizeLocations() {
			NormalizeAssets<Location>(database.locations, template.locationFields);
		}

		private void NormalizeVariables() {
			NormalizeAssets<Variable>(database.variables, template.variableFields);
		}
		
		private void NormalizeConversations() {
			NormalizeAssets<Conversation>(database.conversations, template.conversationFields);
		}
		
		private void NormalizeDialogueEntries() {
			foreach (var conversation in database.conversations) {
				foreach (var entry in conversation.dialogueEntries) {
					AddMissingFieldsToTemplate(entry.fields, template.dialogueEntryFields);
				}
			}
			foreach (var conversation in database.conversations) {
				foreach (var entry in conversation.dialogueEntries) {
					EnforceTemplateOnFields(entry.fields, template.dialogueEntryFields);
				}
			}
		}

		private void NormalizeAssets<T>(List<T> assets, List<Field> templateFields) where T : Asset {
			foreach (var asset in assets) {
				AddMissingFieldsToTemplate(asset.fields, templateFields);
			}
			foreach (var asset in assets) {
				EnforceTemplateOnFields(asset.fields, templateFields);
			}
		}
		
		private void AddMissingFieldsToTemplate(List<Field> assetFields, List<Field> templateFields) {
			foreach (var field in assetFields) {
				if (!Field.FieldExists(templateFields, field.title)) {
					templateFields.Add(new Field(field.title, string.Empty, field.type));
				}
			}
		}

		private void EnforceTemplateOnFields(List<Field> fields, List<Field> templateFields) {
			List<Field> newFields = new List<Field>();
			for (int i = 0; i < templateFields.Count; i++) {
				Field templateField = templateFields[i];
				if (!string.IsNullOrEmpty(templateField.title)) {
					newFields.Add(Field.Lookup(fields, templateField.title) ?? new Field(templateField));
				}
			}
			fields.Clear();
			for (int i = 0; i < newFields.Count; i++) {
				fields.Add(newFields[i]);
			}
		}
		
	}

}