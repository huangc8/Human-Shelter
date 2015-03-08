using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem {

	[CustomEditor(typeof(PersistentActiveData))]
	public class PersistentActiveDataEditor : Editor {

		public void OnEnable() {
			EditorTools.SetInitialDatabaseIfNull();
		}

		public override void OnInspectorGUI() {
			var persistentActiveData = target as PersistentActiveData;
			if (persistentActiveData == null) return;

			persistentActiveData.target = EditorGUILayout.ObjectField(new GUIContent("Target", "The GameObject to set active or inactive based on the Condition below"), persistentActiveData.target, typeof(GameObject), true) as GameObject;

			EditorTools.DrawReferenceDatabase();
			EditorTools.DrawSerializedProperty(serializedObject, "condition");
		}

	}

}
