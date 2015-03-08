using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// Custom inspector editor for DialogueSystemController (e.g., Dialogue Manager).
	/// </summary>
	[CustomEditor (typeof(DialogueSystemController))]
	public class DialogueSystemControllerEditor : Editor {

		private const string LightSkinIconFilename = "Assets/Dialogue System/DLLs/DialogueManager Inspector Light.png";
		private const string DarkSkinIconFilename  = "Assets/Dialogue System/DLLs/DialogueManager Inspector Dark.png";

		private static Texture2D icon = null;
		private static GUIStyle iconButtonStyle = null;
		private static GUIContent iconButtonContent = null;

		/// <summary>
		/// Draws the inspector GUI. Adds a Dialogue System banner.
		/// </summary>
		public override void OnInspectorGUI() {
			DrawExtraFeatures();
			DrawDefaultInspector();
		}

		private void DrawExtraFeatures() {
			var dialogueSystemController = target as DialogueSystemController;
			if (icon == null) {
				string iconFilename = EditorGUIUtility.isProSkin ? DarkSkinIconFilename : LightSkinIconFilename;
				icon = AssetDatabase.LoadAssetAtPath(iconFilename, typeof(Texture2D)) as Texture2D;
			}
			if (dialogueSystemController == null || icon == null) return;
			//---Was: GUILayout.Label(icon);
			if (iconButtonStyle == null) {
				iconButtonStyle = new GUIStyle(EditorStyles.label);
				iconButtonStyle.normal.background = icon;
				iconButtonStyle.active.background = icon;
			}
			if (iconButtonContent == null) {
				iconButtonContent = new GUIContent(string.Empty, "Click to open Dialogue Editor.");
			}
			if (GUILayout.Button(iconButtonContent, iconButtonStyle, GUILayout.Width(icon.width), GUILayout.Height(icon.height))) {
				Selection.activeObject = dialogueSystemController.initialDatabase;
				PixelCrushers.DialogueSystem.DialogueEditor.DialogueEditorWindow.OpenDialogueEditorWindow();
			}
			EditorWindowTools.DrawHorizontalLine();
		}

	}

}
