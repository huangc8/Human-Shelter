using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles the Locations tab. Locations are
	/// just treated as basic assets, so it uses the generic asset methods.
	/// </summary>
	public partial class DialogueEditorWindow {

		private AssetFoldouts locationFoldouts = new AssetFoldouts();

		private void ResetLocationSection() {
			locationFoldouts = new AssetFoldouts();
		}

		private void DrawLocationSection() {
			if (database.syncInfo.syncLocations) {
				DrawAssetSection<Location>("Location", database.locations, locationFoldouts, DrawLocationMenu, DrawLocationSyncDatabase);
			} else {
				DrawAssetSection<Location>("Location", database.locations, locationFoldouts, DrawLocationMenu);
			}
		}

		private void DrawLocationMenu() {
			if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56))) {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("New Location"), false, AddNewLocation);
				menu.AddItem(new GUIContent("Sort/By Name"), false, SortLocationsByName);
				menu.AddItem(new GUIContent("Sort/By ID"), false, SortLocationsByID);
				menu.AddItem(new GUIContent("Sync From DB"), database.syncInfo.syncLocations, ToggleSyncLocationsFromDB);
				menu.ShowAsContext();
			}
		}
		
		private void AddNewLocation() {
			AddNewAsset<Location>(database.locations);
		}
		
		private void SortLocationsByName() {
			database.locations.Sort((x, y) => x.Name.CompareTo(y.Name));
		}
		
		private void SortLocationsByID() {
			database.locations.Sort((x, y) => x.id.CompareTo(y.id));
		}

		private void ToggleSyncLocationsFromDB() {
			database.syncInfo.syncLocations = !database.syncInfo.syncLocations;
		}
		
		private void DrawLocationSyncDatabase() {
			EditorGUILayout.BeginHorizontal();
			DialogueDatabase newDatabase = EditorGUILayout.ObjectField(new GUIContent("Sync From", "Database to sync locations from."),
			                                                           database.syncInfo.syncLocationsDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
			if (newDatabase != database.syncInfo.syncLocationsDatabase) {
				database.syncInfo.syncLocationsDatabase = newDatabase;
				database.SyncLocations();
			}
			if (GUILayout.Button(new GUIContent("Sync Now", "Syncs from the database."), EditorStyles.miniButton, GUILayout.Width(72))) {
				database.SyncLocations();
			}
			EditorGUILayout.EndHorizontal();
		}
		
	}

}