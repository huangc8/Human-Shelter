using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace PixelCrushers.DialogueSystem {
	
	public static class EditorWindowTools {
	
		public static void DrawHorizontalLine() {
			GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
		}
		
		public static void StartIndentedSection() {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(string.Empty, GUILayout.Width(8));
			EditorGUILayout.BeginVertical();
		}
		
		public static void EndIndentedSection() {
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}

		public static string GetDirectoryName(string filename) {
			if (!string.IsNullOrEmpty(filename)) try {
				return Path.GetDirectoryName(filename);
			} catch (ArgumentException) {
			}
			return string.Empty;
		}

		public static string GetCurrentDirectory() {
			foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)) {
				string path = AssetDatabase.GetAssetPath(obj);
				if (File.Exists(path)) path = Path.GetDirectoryName(path);
				return path;
			}
			return "Assets";
		}
		
	}
	
}
