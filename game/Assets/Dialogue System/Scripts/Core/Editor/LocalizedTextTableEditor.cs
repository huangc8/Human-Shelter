using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// Custom inspector editor for localized text tables.
	/// </summary>
	[CustomEditor (typeof(LocalizedTextTable))]
	public class LocalizedTextTableEditor : Editor {

		/// <summary>
		/// Is the languages foldout open?
		/// </summary>
		private static bool languagesFoldout = false;

		/// <summary>
		/// Is the fields foldout open?
		/// </summary>
		private static bool fieldsFoldout = true;

		/// <summary>
		/// Tracks which individual fields are open.
		/// </summary>
		private static Dictionary<int, bool> fieldFoldouts = new Dictionary<int, bool>();

		/// <summary>
		/// The localized text table that we're currently editing.
		/// </summary>
		private LocalizedTextTable table = null;

		/// <summary>
		/// The filename to use when importing and exporting CSV.
		/// </summary>
		private static string csvFilename = string.Empty;

		/// <summary>
		/// Draws our custom inspector.
		/// </summary>
		public override void OnInspectorGUI() {
			table = target as LocalizedTextTable;
			if (table == null) return;
			DrawLanguages();
			EditorWindowTools.DrawHorizontalLine();
			DrawFields();
			if (GUI.changed) EditorUtility.SetDirty(table);
		}

		private void DrawLanguages() {
			// Draw Languages foldout and menu:
			EditorGUILayout.BeginHorizontal();
			languagesFoldout = EditorGUILayout.Foldout(languagesFoldout, "Languages");
			DrawMenu();
			if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(22))) {
				languagesFoldout = true;
				table.languages.Add(string.Empty);
			}
			EditorGUILayout.EndHorizontal();

			// Draw languages:
			if (languagesFoldout) {
				int languageIndexToDelete = -1;
				EditorWindowTools.StartIndentedSection();
				for (int i = 0; i < table.languages.Count; i++) {
					EditorGUILayout.BeginHorizontal();
					table.languages[i] = EditorGUILayout.TextField(table.languages[i]);
					EditorGUI.BeginDisabledGroup(i == 0);
					if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(22))) {
						languageIndexToDelete = i;
					}
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndHorizontal();
				}
				EditorWindowTools.EndIndentedSection();
				if (languageIndexToDelete != -1) DeleteLanguage(languageIndexToDelete);
			}
		}

		private void DrawMenu() {
			if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56))) {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Sort"), false, SortLanguages);
				menu.AddItem(new GUIContent("Import..."), false, Import);
				menu.AddItem(new GUIContent("Export..."), false, Export);
				menu.ShowAsContext();
			}
		}
		


		private void DeleteLanguage(int languageIndex) {
			if (EditorUtility.DisplayDialog("Delete language?", 
			                                string.Format("Are you sure you want to delete '{0}' and related text in all fields?", table.languages[languageIndex]),
			                                "Delete", "Cancel")) {
				table.languages.RemoveAt(languageIndex);
				foreach (var field in table.fields) {
					if (languageIndex < field.values.Count) field.values.RemoveAt(languageIndex);
				}
			}
		}

		private void DrawFields() {
			// Draw Fields foldout and "+" button:
			EditorGUILayout.BeginHorizontal();
			fieldsFoldout = EditorGUILayout.Foldout(fieldsFoldout, "Fields");
			if (GUILayout.Button("Sort", EditorStyles.miniButton, GUILayout.Width(44))) {
				SortFields();
				return;
			}
			if (GUILayout.Button("+", EditorStyles.miniButton, GUILayout.Width(22))) {
				fieldsFoldout = true;
				table.fields.Add(new LocalizedTextTable.LocalizedTextField());
				if (!fieldFoldouts.ContainsKey(table.fields.Count - 1)) {
					fieldFoldouts.Add(table.fields.Count - 1, true);
				}
			}
			EditorGUILayout.EndHorizontal();

			// Draw fields:
			if (fieldsFoldout) {
				int fieldIndexToDelete = -1;
				EditorWindowTools.StartIndentedSection();
				for (int i = 0; i < table.fields.Count; i++) {
					LocalizedTextTable.LocalizedTextField field = table.fields[i];
					if (!fieldFoldouts.ContainsKey(i)) fieldFoldouts.Add(i, false);
					EditorGUILayout.BeginHorizontal();
					fieldFoldouts[i] = EditorGUILayout.Foldout(fieldFoldouts[i], string.IsNullOrEmpty(field.name) ? string.Format("Field {0}", i) : field.name);
					if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(22))) {
						fieldIndexToDelete = i;
					}
					EditorGUILayout.EndHorizontal();
					if (fieldFoldouts[i]) {
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Field", GUILayout.Width(60));
						field.name = EditorGUILayout.TextField(field.name);
						EditorGUILayout.LabelField(string.Empty, GUILayout.Width(22));
						EditorGUILayout.EndHorizontal();
						EditorWindowTools.StartIndentedSection();
						for (int j = 0; j < table.languages.Count; j++) {
							if (j >= field.values.Count) field.values.Add(string.Empty);
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField(table.languages[j], GUILayout.Width(60));
							field.values[j] = EditorGUILayout.TextField(field.values[j]);
							EditorGUILayout.LabelField(string.Empty, GUILayout.Width(22));
							EditorGUILayout.EndHorizontal();
						}
						EditorWindowTools.EndIndentedSection();
					}
				}
				EditorWindowTools.EndIndentedSection();
				if (fieldIndexToDelete != -1) DeleteField(fieldIndexToDelete);
			}
		}

		private void DeleteField(int fieldIndex) {
			if (EditorUtility.DisplayDialog("Delete field?", 
			                                string.Format("Are you sure you want to delete field {0} (\"{1}\")?", fieldIndex, table.fields[fieldIndex].name),
			                                "Delete", "Cancel")) {
				table.fields.RemoveAt(fieldIndex);
			}
		}

		private void SortFields() {
			table.fields.Sort((x, y) => string.Compare(x.name, y.name, System.StringComparison.OrdinalIgnoreCase));
		}
		
		private void SortLanguages() {
			table.languages.RemoveAt(0);
			table.languages.Sort();
			table.languages.Insert(0, "Default");
		}

		private void Import() {
			if (!EditorUtility.DisplayDialog("Import CSV?", 
			                                "Importing from CSV will overwrite the current contents. Are you sure?",
			                                "Import", "Cancel")) {
				return;
			}
			string newFilename = EditorUtility.OpenFilePanel("Import from CSV", EditorWindowTools.GetDirectoryName(csvFilename), "csv");
			if (!string.IsNullOrEmpty(newFilename)) {
				csvFilename = newFilename;
				if (Application.platform == RuntimePlatform.WindowsEditor) {
					csvFilename = csvFilename.Replace("/", "\\");
				}
				try {
					using (StreamReader file = new StreamReader(csvFilename, Encoding.UTF8)) {

						// Work with a temporary, new table:
						LocalizedTextTable newTable = ScriptableObject.CreateInstance<LocalizedTextTable>();

						// Read heading:
						string[] values = CSVExporter.GetValues(file.ReadLine());
						newTable.languages = new List<string>(values);
						newTable.languages.RemoveAt(0);

						// Read fields:
						newTable.fields.Clear();
						while (!file.EndOfStream) {
							values = CSVExporter.GetValues(file.ReadLine());
							LocalizedTextTable.LocalizedTextField field = new LocalizedTextTable.LocalizedTextField();
							field.name = values[0];
							for (int i = 1; i < values.Length; i++) {
								field.values.Add(values[i]);
							}
							newTable.fields.Add(field);
						}

						// If we got to the end, use the new table:
						table.languages.Clear();
						foreach (var newLanguage in newTable.languages) {
							table.languages.Add(newLanguage);
						}
						table.fields.Clear();
						foreach (var newField in newTable.fields) {
							LocalizedTextTable.LocalizedTextField field = new LocalizedTextTable.LocalizedTextField();
							field.name = newField.name;
							field.values = new List<string>(newField.values);
							table.fields.Add(field);
						}
						DestroyImmediate(newTable);
					}
				} catch (System.Exception e) {
					Debug.LogError(e.Message);
					EditorUtility.DisplayDialog("Import Failed", "There was an error importing the CSV file.", "OK");
				}
				EditorUtility.DisplayDialog("Export Complete", "The localized text table was exported to CSV (comma-separated values) format. ", "OK");
			}
		}

		private void Export() {
			string newFilename = EditorUtility.SaveFilePanel("Export to CSV", EditorWindowTools.GetDirectoryName(csvFilename), csvFilename, "csv");
			if (!string.IsNullOrEmpty(newFilename)) {
				csvFilename = newFilename;
				if (Application.platform == RuntimePlatform.WindowsEditor) {
					csvFilename = csvFilename.Replace("/", "\\");
				}
				using (StreamWriter file = new StreamWriter(csvFilename, false, Encoding.UTF8)) {

					// Write heading:
					StringBuilder sb = new StringBuilder();
					sb.Append("Field");
					foreach (var language in table.languages) {
						sb.AppendFormat(",{0}", CSVExporter.CleanField(language));
					}
					file.WriteLine(sb);

					// Write fields:
					foreach (var field in table.fields) {
						sb = new StringBuilder();
						sb.Append(field.name);
						foreach (var value in field.values) {
							sb.AppendFormat(",{0}", CSVExporter.CleanField(value));
						}
						file.WriteLine(sb);
					}
				}
				EditorUtility.DisplayDialog("Export Complete", "The localized text table was exported to CSV (comma-separated values) format. ", "OK");
			}
		}
		

	}

}
