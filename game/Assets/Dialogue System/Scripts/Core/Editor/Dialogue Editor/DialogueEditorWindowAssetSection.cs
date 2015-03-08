using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window provides generic functionality for assets.
	/// Most of the tabs (Actor, Item, etc.) use the generic methods in this part as a base.
	/// </summary>
	public partial class DialogueEditorWindow {

		private List<Field> clipboardFields = null;

		private void DrawAssetSection<T>(string label, List<T> assets, AssetFoldouts foldouts) where T : Asset, new() {
			DrawAssetSection<T>(label, assets, foldouts, null, null);
		}

		private void DrawAssetSection<T>(string label, List<T> assets, AssetFoldouts foldouts, Action menuDelegate) where T : Asset, new() {
			DrawAssetSection<T>(label, assets, foldouts, menuDelegate, null);
		}
		
		private void DrawAssetSection<T>(string label, List<T> assets, AssetFoldouts foldouts, Action menuDelegate, Action syncDatabaseDelegate) where T : Asset, new() {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(label + "s", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			if (menuDelegate != null) menuDelegate();
			EditorGUILayout.EndHorizontal();
			if (syncDatabaseDelegate != null) syncDatabaseDelegate();
			DrawAssets<T>(label, assets, foldouts);
		}
		
		private void DrawAssets<T>(string label, List<T> assets, AssetFoldouts foldouts) where T : Asset {
			EditorWindowTools.StartIndentedSection();
			showStateFieldAsQuest = false;
			DrawAssetSpecificFoldoutProperties<T>(assets);
			T assetToRemove = null;
			for (int index = 0; index < assets.Count; index++) {
				T asset = assets[index];
				EditorGUILayout.BeginHorizontal();
				if (!foldouts.properties.ContainsKey(index)) foldouts.properties.Add(index, false);
				foldouts.properties[index] = EditorGUILayout.Foldout(foldouts.properties[index], GetAssetName(asset));
				if (GUILayout.Button(new GUIContent(" ", string.Format("Delete {0}.", GetAssetName(asset))), "OL Minus", GUILayout.Width(16))) assetToRemove = asset;
				EditorGUILayout.EndHorizontal();
				if (foldouts.properties[index]) DrawAsset<T>(asset, index, foldouts);
			}
			if (assetToRemove != null) {
				if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", GetAssetName(assetToRemove)), "Are you sure you want to delete this?", "Delete", "Cancel")) {
					assets.Remove(assetToRemove);
				}
			}
			EditorWindowTools.EndIndentedSection();
		}

		private string GetAssetName(Asset asset) {
			return (asset is Conversation) ? (asset as Conversation).Title : asset.Name;
		}
		
		private void DrawAssetSpecificFoldoutProperties<T>(List<T> assets) {
			//--- No longer used: if (typeof(T) == typeof(Item)) DrawItemSpecificFoldoutProperties();
		}
		
		private void DrawAsset<T>(T asset, int index, AssetFoldouts foldouts) where T : Asset {
			EditorWindowTools.StartIndentedSection();
			EditorGUILayout.BeginVertical("button");
			DrawAssetSpecificPropertiesFirstPart(asset);
			DrawFieldsFoldout<T>(asset, index, foldouts);
			DrawAssetSpecificPropertiesSecondPart(asset, index, foldouts);
			EditorGUILayout.EndVertical();
			EditorWindowTools.EndIndentedSection();
		}
		
		private void DrawAssetSpecificPropertiesFirstPart(Asset asset) {
			if (!(asset is Variable)) asset.id = EditorGUILayout.IntField(new GUIContent("ID", "Internal ID. Change at your own risk."), asset.id);
			asset.Name = EditorGUILayout.TextField(new GUIContent("Name", "Name of this asset."), asset.Name);
			if (asset is Actor) DrawActorPortrait(asset as Actor);
			if (asset is Item) DrawItemPropertiesFirstPart(asset as Item);
		}
		
		private void DrawAssetSpecificPropertiesSecondPart(Asset asset, int index, AssetFoldouts foldouts) {
			if (asset is Item) DrawItemSpecificPropertiesSecondPart(asset as Item, index, foldouts);
		}
		
		private void DrawFieldsFoldout<T>(T asset, int index, AssetFoldouts foldouts) where T : Asset {
			if (!foldouts.fields.ContainsKey(index)) foldouts.fields.Add(index, false);
			EditorGUILayout.BeginHorizontal();
			foldouts.fields[index] = EditorGUILayout.Foldout(foldouts.fields[index], "All Fields");
			if (foldouts.fields[index]) {
				GUILayout.FlexibleSpace();
				if (GUILayout.Button(new GUIContent("Template", "Add any missing fields from the template."), EditorStyles.miniButton, GUILayout.Width(60))) {
					ApplyTemplate(asset.fields, GetTemplateFields(asset));
				}
				if (GUILayout.Button(new GUIContent("Copy", "Copy these fields to the clipboard."), EditorStyles.miniButton, GUILayout.Width(60))) {
					CopyFields(asset.fields);
				}
				EditorGUI.BeginDisabledGroup(clipboardFields == null);
				if (GUILayout.Button(new GUIContent("Paste", "Paste the clipboard into these fields."), EditorStyles.miniButton, GUILayout.Width(60))) {
					PasteFields(asset.fields);
				}
				EditorGUI.EndDisabledGroup();
			}
			if (GUILayout.Button(new GUIContent(" ", "Add new field."), "OL Plus", GUILayout.Width(16))) asset.fields.Add(new Field());
			EditorGUILayout.EndHorizontal();
			if (foldouts.fields[index]) DrawFieldsSection(asset.fields);
		}

		private void CopyFields(List<Field> fields) {
			clipboardFields = fields;
		}

		private void PasteFields(List<Field> fields) {
			foreach (Field clipboardField in clipboardFields) {
				Field field = Field.Lookup(fields, clipboardField.title);
				if (field != null) {
					field.value = clipboardField.value;
					field.type = clipboardField.type;
				} else {
					fields.Add(new Field(clipboardField));
				}
			}
		}

		private void ApplyTemplate(List<Field> fields, List<Field> templateFields) {
			if (fields == null || templateFields == null) return;
			foreach (Field templateField in templateFields) {
				if (!string.IsNullOrEmpty(templateField.title)) {
					if (!Field.FieldExists(fields, templateField.title)) {
						fields.Add(new Field(templateField));
					}
				}
			}
		}
		
		private T AddNewAsset<T>(List<T> assets) where T: Asset, new() {
			T asset = new T();
			int highestID = -1;
			assets.ForEach(a => highestID = Mathf.Max(highestID, a.id));
			asset.id = Mathf.Max(1, highestID + 1);
			asset.fields = template.CreateFields(GetTemplateFields(asset));
			if (asset is Conversation) {
				InitializeConversation(asset as Conversation);
			} else if (asset is Item) {
				string itemTypeLabel = template.treatItemsAsQuests ? "Quest" : "Item";
				asset.Name = string.Format("New {0} {1}", itemTypeLabel, asset.id);
				Field.SetValue(asset.fields, "Is Item", !template.treatItemsAsQuests);
			} else {
				asset.Name = string.Format("New {0} {1}", typeof(T).Name, asset.id);
			}
			assets.Add(asset);
			return asset;
		}
		
		private T AddNewAssetFromTemplate<T>(List<T> assets, List<Field> templateFields, string typeLabel) where T: Asset, new() {
			T asset = new T();
			int highestID = -1;
			assets.ForEach(a => highestID = Mathf.Max(highestID, a.id));
			asset.id = Mathf.Max(1, highestID + 1);
			if (templateFields == null) templateFields = GetTemplateFields(asset);
			asset.fields = template.CreateFields(templateFields);
			if (asset is Conversation) {
				InitializeConversation(asset as Conversation);
			} else {
				asset.Name = string.Format("New {0} {1}", typeLabel, asset.id);
			}
			assets.Add(asset);
			return asset;
		}
		
		private void InitializeConversation(Conversation conversation) {
			conversation.Title = string.Format("New Conversation {0}", conversation.id);
			conversation.ActorID = database.playerID;
			DialogueEntry startEntry = new DialogueEntry();
			startEntry.fields = new List<Field>();
			InitializeFieldsFromTemplate(startEntry.fields, template.dialogueEntryFields);
			startEntry.Title = "START";
			startEntry.Sequence = "None()";
			startEntry.ActorID = database.playerID;
			conversation.dialogueEntries.Add(startEntry);
		}
		
		private void InitializeFieldsFromTemplate(List<Field> fields, List<Field> templateFields) {
			templateFields.ForEach(templateField => fields.Add(new Field(templateField.title, templateField.value, templateField.type)));
		}
		
		private List<Field> GetTemplateFields(Asset asset) {
			if (asset is Actor) return template.actorFields;
			if (asset is Item) return (asset as Item).IsItem ? template.itemFields : template.questFields;
			if (asset is Location) return template.locationFields;
			if (asset is Variable) return template.variableFields;
			if (asset is Conversation) return template.conversationFields;
			return template.locationFields;
		}

	}

}