using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles the Actors tab.
	/// </summary>
	public partial class DialogueEditorWindow {

		private AssetFoldouts actorFoldouts = new AssetFoldouts();

		private void ResetActorSection() {
			actorFoldouts = new AssetFoldouts();
		}

		private void DrawActorSection() {
			if (database.syncInfo.syncActors) {
				DrawAssetSection<Actor>("Actor", database.actors, actorFoldouts, DrawActorMenu, DrawActorSyncDatabase);
			} else {
				DrawAssetSection<Actor>("Actor", database.actors, actorFoldouts, DrawActorMenu);
			}
		}

		private void DrawActorMenu() {
			if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56))) {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("New Actor"), false, AddNewActor);
				menu.AddItem(new GUIContent("Sort/By Name"), false, SortActorsByName);
				menu.AddItem(new GUIContent("Sort/By ID"), false, SortActorsByID);
				menu.AddItem(new GUIContent("Sync From DB"), database.syncInfo.syncActors, ToggleSyncActorsFromDB);
				menu.ShowAsContext();
			}
		}

		private void AddNewActor() {
			AddNewAsset<Actor>(database.actors);
		}
		
		private void SortActorsByName() {
			database.actors.Sort((x, y) => x.Name.CompareTo(y.Name));
		}
		
		private void SortActorsByID() {
			database.actors.Sort((x, y) => x.id.CompareTo(y.id));
		}

		private void ToggleSyncActorsFromDB() {
			database.syncInfo.syncActors = !database.syncInfo.syncActors;
		}

		private void DrawActorSyncDatabase() {
			EditorGUILayout.BeginHorizontal();
			DialogueDatabase newDatabase = EditorGUILayout.ObjectField(new GUIContent("Sync From", "Database to sync actors from."),
			                                                           database.syncInfo.syncActorsDatabase, typeof(DialogueDatabase), false) as DialogueDatabase;
			if (newDatabase != database.syncInfo.syncActorsDatabase) {
				database.syncInfo.syncActorsDatabase = newDatabase;
				database.SyncActors();
			}
			if (GUILayout.Button(new GUIContent("Sync Now", "Syncs from the database."), EditorStyles.miniButton, GUILayout.Width(72))) {
				database.SyncActors();
			}
			EditorGUILayout.EndHorizontal();
		}
		
		private void DrawActorPortrait(Actor actor) {
			EditorGUILayout.BeginHorizontal();

			actor.portrait = EditorGUILayout.ObjectField(new GUIContent("Portraits", "This actor's portrait. Only necessary if your UI uses portraits."), 
			                                             actor.portrait, typeof(Texture2D), false, GUILayout.Height(64)) as Texture2D;
			int indexToDelete = -1;
			for (int i = 0; i < actor.alternatePortraits.Count; i++) {
				EditorGUILayout.BeginVertical();

				actor.alternatePortraits[i] = EditorGUILayout.ObjectField(actor.alternatePortraits[i], typeof(Texture2D), false, GUILayout.Width (54), GUILayout.Height(54)) as Texture2D;
				EditorGUILayout.BeginHorizontal(GUILayout.Width(54));
				EditorGUILayout.LabelField(string.Format("[{0}]", i+2), CenteredLabelStyle, GUILayout.Width(27));
				if (GUILayout.Button(new GUIContent(" ", "Delete this portrait."), "OL Minus", GUILayout.Width(27), GUILayout.Height(16))) { 
					indexToDelete = i;
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();
			}
			if (indexToDelete > -1) actor.alternatePortraits.RemoveAt(indexToDelete);

			if (GUILayout.Button(new GUIContent(" ", "Add new alternate portrait image."), "OL Plus", GUILayout.Height(48))) {
				actor.alternatePortraits.Add(null);
			}	
			GUILayout.FlexibleSpace();

			EditorGUILayout.EndHorizontal();
		}
		
	}

}