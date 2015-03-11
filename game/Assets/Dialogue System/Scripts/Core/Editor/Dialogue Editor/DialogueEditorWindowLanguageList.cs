﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window keeps track of the list of languages used in the
	/// current dialogue database.
	/// </summary>
	public partial class DialogueEditorWindow {

		public HashSet<string> languages = new HashSet<string>();

		private readonly string[] LanguageFieldPrefixes = new string[] { 
			"Dialogue Text ", "Menu Text ", "Sequence ", "Response Menu Sequence ",
			"Description ", "Success Description ", "Failure Description "
		};

		private void ResetLanguageList() {
			languages.Clear();
		}

		private void BuildLanguageListFromFields(List<Field> fields) {
			foreach (var field in fields) {
				if (field.type == FieldType.Localization) {
					// Don't add Dialogue Text:
					if (field.title.Equals("Dialogue Text")) break;

					// Assume it's Chat Mapper-style localized dialogue text, in which case
					// the language is the entire field title:
					string language = field.title;

					// If it's a different type of field, remove the prefix:
					foreach (var prefix in LanguageFieldPrefixes) {
						if (field.title.StartsWith(prefix)) {
							language = field.title.Substring(prefix.Length);
						} else {
							// Handle "Entry X Language":
							Match match = Regex.Match(field.title, @"Entry [0-9]+ .*");
							if (match.Success) {
								string[] entryParts = field.title.Split(new char[] { ' ' });
								language = (entryParts.Length >= 3) ? entryParts[2] : string.Empty;
							}
						}
					}
					if (!string.IsNullOrEmpty(language)) languages.Add(language);
				}
			}
		}

		private void DrawLocalizedVersions(List<Field> fields, string titleFormat, bool alwaysAdd, FieldType fieldType) {
			DrawLocalizedVersions(fields, titleFormat, alwaysAdd, fieldType, null);
		}

		private void DrawLocalizedVersions(List<Field> fields, string titleFormat, bool alwaysAdd, FieldType fieldType, List<Field> alreadyDrawn) {
			bool indented = false;
			foreach (string language in languages) {
				string localizedTitle = string.Format(titleFormat, language);
				Field field = Field.Lookup(fields, localizedTitle);
				if ((field == null) && (alwaysAdd || (Field.FieldExists(template.dialogueEntryFields, localizedTitle)))) {
					field = new Field(localizedTitle, string.Empty, fieldType);
					fields.Add(field);
				}
				if (field != null) {
					if (!indented) {
						indented = true;
						EditorWindowTools.StartIndentedSection();
					}
					EditorGUILayout.LabelField(localizedTitle);
					field.value = EditorGUILayout.TextArea(field.value);
					if (alreadyDrawn != null) alreadyDrawn.Add(field);
				}
			}
			if (indented) EditorWindowTools.EndIndentedSection();
		}
		
	}
	
}