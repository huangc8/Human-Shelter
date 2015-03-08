using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This is a base class for most of the "On dialogue event" editors.
	/// It simply adds a Reference Database field to the editor above
	/// the default inspector.
	/// </summary>
	public class ReferenceDatabaseDialogueEventEditor : Editor {

		public void OnEnable() {
			EditorTools.SetInitialDatabaseIfNull();
		}
		
		public override void OnInspectorGUI() {
			EditorTools.DrawReferenceDatabase();
			DrawDefaultInspector();
		}

	}

}
