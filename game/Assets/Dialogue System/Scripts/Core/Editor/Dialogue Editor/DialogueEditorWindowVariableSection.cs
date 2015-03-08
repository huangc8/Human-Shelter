using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles the Variables tab. Variables are
	/// just treated as basic assets, so it uses the generic asset methods.
	/// </summary>
	public partial class DialogueEditorWindow {

		private AssetFoldouts variableFoldouts = new AssetFoldouts();

		private void ResetVariableSection() {
			variableFoldouts = new AssetFoldouts();
		}

		private void DrawVariableSection() {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Variables", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			DrawVariableMenu();
			EditorGUILayout.EndHorizontal();
			if (database.syncInfo.syncVariables) DrawVariableSyncDatabase();
			DrawVariables();
		}
		
		private void DrawVariableMenu() {
			if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56))) {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("New Variable"), false, AddNewVariable);
				menu.AddItem(new GUIContent("Sort/By Name"), false, SortVariablesByName);
				menu.AddItem(new GUIContent("Sort/By ID"), false, SortVariablesByID);
				menu.AddItem(new GUIContent("Sync From DB"), database.syncInfo.syncVariables, ToggleSyncVariablesFromDB);
				menu.ShowAsContext();
			}
		}

		private void AddNewVariable() {
			Variable newVariable = AddNewAsset<Variable>(database.variables);
			if (!Field.FieldExists(newVariable.fields, "Name")) newVariable.fields.Add(new Field("Name", string.Empty, FieldType.Text));
			if (!Field.FieldExists(newVariable.fields, "Initial Value")) newVariable.fields.Add(new Field("Initial Value", string.Empty, FieldType.Text));
			if (!Field.FieldExists(newVariable.fields, "Description")) newVariable.fields.Add(new Field("Description", string.Empty, FieldType.Text));
			int index = database.variables.Count - 1;
			if (!variableFoldouts.properties.ContainsKey(index)) variableFoldouts.properties.Add(index, false);
			variableFoldouts.properties[index] = true;
		}
		
		private void SortVariablesByName() {
			database.variables.Sort((x, y) => x.Name.CompareTo(y.Name));
		}
		
		private void SortVariablesByID() {
			database.variables.Sort((x, y) => x.id.CompareTo(y.id));
		}

		private void ToggleSyncVariablesFromDB() {
			database.syncInfo.syncVariables = !database.syncInfo.syncVariables;
		}
		
		private void DrawVariableSyncDatabase() {
			EditorGUILayout.BeginHorizontal();
			DialogueDatabase newDatabase = EditorGUILayout.ObjectField(new GUIContent("Sync From", "Database to sync variables from."),
			                                                           database.syncInfo.syncVariablesDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
			if (newDatabase != database.syncInfo.syncVariablesDatabase) {
				database.syncInfo.syncVariablesDatabase = newDatabase;
				database.SyncVariables();
			}
			if (GUILayout.Button(new GUIContent("Sync Now", "Syncs from the database."), EditorStyles.miniButton, GUILayout.Width(72))) {
				database.SyncVariables();
			}
			EditorGUILayout.EndHorizontal();
		}
		
		private void DrawVariables() {
			List<Variable> assets = database.variables;
			AssetFoldouts foldouts = variableFoldouts;
			EditorWindowTools.StartIndentedSection();
			Variable assetToRemove = null;
			for (int index = 0; index < assets.Count; index++) {
				Variable asset = assets[index];
				EditorGUILayout.BeginHorizontal();
				if (!foldouts.properties.ContainsKey(index)) foldouts.properties.Add(index, false);
				foldouts.properties[index] = EditorGUILayout.Foldout(foldouts.properties[index], GetAssetName(asset));
				if (GUILayout.Button(new GUIContent(" ", string.Format("Delete {0}.", GetAssetName(asset))), "OL Minus", GUILayout.Width(16))) assetToRemove = asset;
				EditorGUILayout.EndHorizontal();
				if (foldouts.properties[index]) DrawVariable(asset, index, foldouts);
			}
			if (assetToRemove != null) {
				if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", GetAssetName(assetToRemove)), "Are you sure you want to delete this?", "Delete", "Cancel")) {
					assets.Remove(assetToRemove);
				}
			}
			EditorWindowTools.EndIndentedSection();
		}

		private void DrawVariable(Variable asset, int index, AssetFoldouts foldouts) {
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.BeginVertical("button");
			List<Field> fields = asset.fields;
			for (int i = 0; i < fields.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				if (IsTextAreaField(fields[i])) {
					DrawTextAreaFirstPart(fields[i], false);
					DrawTextAreaSecondPart(fields[i]);
				} else {
					DrawField(fields[i], false);
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();
			EditorWindowTools.EndIndentedSection();
		}
		
	}

}