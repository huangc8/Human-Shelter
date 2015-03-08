using UnityEngine;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// The persistent position data component works with the PersistentDataManager to keep track 
	/// of a game object's position when saving and loading games or changing levels.
	/// </summary>
	[AddComponentMenu("Dialogue System/Save System/Persistent Position Data")]
	public class PersistentPositionData : MonoBehaviour {
		
		/// <summary>
		/// (Optional) Normally, this component uses the game object's name as the name of the 
		/// actor in the Lua Actor[] table. If your actor is named differently in the Lua Actor[] 
		/// table (e.g., the actor has a different name in Chat Mapper or the DialogueDatabase), 
		/// then set this property to the Lua name.
		/// </summary>
		public string overrideActorName;

		/// <summary>
		/// If <c>true<c/c>, the object's current level is also recorded; and, on load, the 
		/// position is only applied if the current level matches the recorded level.
		/// </summary>
		public bool recordCurrentLevel = true;

		public bool restoreCurrentLevelPosition = true;
		
		private string actorName { 
			get { return string.IsNullOrEmpty(overrideActorName) ? gameObject.name : overrideActorName; } 
		}

		public void Start() {
			if (string.IsNullOrEmpty(overrideActorName)) {
				OverrideActorName globalOverrideActorName = GetComponentInChildren<OverrideActorName>();
				if (globalOverrideActorName != null) overrideActorName = globalOverrideActorName.GetOverrideName();
			}
		}

		/// <summary>
		/// Listens for the OnRecordPersistentData message and records the game object's position 
		/// and rotation into the Lua Actor[] table.
		/// </summary>
		public void OnRecordPersistentData() {
			string positionString = GetPositionString();
			DialogueLua.SetActorField(actorName, "Position", positionString);
			if (recordCurrentLevel) {
				DialogueLua.SetActorField(actorName, "Position_" + Application.loadedLevelName, positionString);
			}
		}
		
		/// <summary>
		/// Listens for the OnApplyPersistentData message and retrieves the game object's position 
		/// and rotation from the Lua Actor[] table.
		/// </summary>
		public void OnApplyPersistentData() {
			string s = string.Empty;
			if (restoreCurrentLevelPosition) {
				s = DialogueLua.GetActorField(actorName, "Position_" + Application.loadedLevelName).AsString;
			}
			if (string.IsNullOrEmpty(s) || string.Equals(s, "nil")) {
				s = DialogueLua.GetActorField(actorName, "Position").AsString;
			}
			if (!string.IsNullOrEmpty(s)) {
				ApplyPositionString(s);
			}
		}
		
		private string GetPositionString() {
			string optionalLevelName = recordCurrentLevel ? DialogueLua.DoubleQuotesToSingle("," + Application.loadedLevelName) : string.Empty;
			return string.Format("{0},{1},{2},{3},{4},{5},{6}{7}", 
				new System.Object[] { transform.position.x, transform.position.y, transform.position.z,
				transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w,
				optionalLevelName });
		}
		
		private void ApplyPositionString(string s) {
			if (string.IsNullOrEmpty(s) || s.Equals("nil")) return;
			string[] tokens = s.Split(',');
			if ((7 <= tokens.Length) && (tokens.Length <= 8)) {
				if (recordCurrentLevel) {
					if ((tokens.Length == 8) && !string.Equals(tokens[7], Application.loadedLevelName)) {
						return; // This is not the recorded level. Don't apply position.
					}
				}
				float[] values = new float[7];
				for (int i = 0; i < 7; i++) {
					values[i] = 0;
					float.TryParse(tokens[i], out values[i]);
				}
				transform.position = new Vector3(values[0], values[1], values[2]);
				transform.rotation = new Quaternion(values[3], values[4], values[5], values[6]);
			}
		}
		
	}

}
