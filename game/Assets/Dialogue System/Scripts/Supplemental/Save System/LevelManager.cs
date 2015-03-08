using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This component combines Application.LoadLevel[Async] with the saved-game data
	/// features of PersistentDataManager. To use it, add it to your Dialogue Manager
	/// object and pass the saved-game data to LevelManager.LoadGame().
	/// </summary>
	[AddComponentMenu("Dialogue System/Save System/Level Manager")]
	public class LevelManager : MonoBehaviour {

		/// <summary>
		/// The default starting level to use if none is recorded in the saved-game data.
		/// </summary>
		public string defaultStartingLevel;

		/// <summary>
		/// Indicates whether a level is currently loading. Only useful in Unity Pro, which
		/// uses Application.LoadLevelAsync().
		/// </summary>
		/// <value><c>true</c> if loading; otherwise, <c>false</c>.</value>
		public bool IsLoading { get; private set; }

		void Awake() {
			IsLoading = false;
		}

		/// <summary>
		/// Loads the game recorded in the provided saveData.
		/// </summary>
		/// <param name="saveData">Save data.</param>
		public void LoadGame(string saveData) {
			StartCoroutine(LoadLevel(saveData));
		}

		/// <summary>
		/// Restarts the game at the default starting level and resets the
		/// Dialogue System to its initial database state.
		/// </summary>
		public void RestartGame() {
			StartCoroutine(LoadLevel(null));
		}

		private IEnumerator LoadLevel(string saveData) {
			string levelName = defaultStartingLevel;
			if (string.IsNullOrEmpty(saveData)) {
				// If no saveData, reset the database.
				DialogueManager.ResetDatabase(DatabaseResetOptions.RevertToDefault);
			} else {
				// Put saveData in Lua so we can get Variable["SavedLevelName"]:
				Lua.Run(saveData, true);
				levelName = DialogueLua.GetVariable("SavedLevelName").AsString;
				if (string.IsNullOrEmpty(levelName)) levelName = defaultStartingLevel;
			}

			// Load the level:
			PersistentDataManager.LevelWillBeUnloaded();
			if (Application.HasProLicense()) {
				AsyncOperation async = Application.LoadLevelAsync(levelName);
				IsLoading = true;
				while (!async.isDone) {
					yield return null;
				}
				IsLoading = false;
			} else {
				Application.LoadLevel(levelName);
			}

			// Wait two frames for objects in the level to finish their Start() methods:
			yield return null;
			yield return null;

			// Then apply saveData to the objects:
			if (!string.IsNullOrEmpty(saveData)) {
				PersistentDataManager.ApplySaveData(saveData);
			}
		}

		/// <summary>
		/// Records the current level in Lua.
		/// </summary>
		public void OnRecordPersistentData() {
			DialogueLua.SetVariable("SavedLevelName", Application.loadedLevelName);
		}
		
	}

}