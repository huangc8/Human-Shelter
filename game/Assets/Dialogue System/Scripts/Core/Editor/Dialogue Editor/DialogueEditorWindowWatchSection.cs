using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles the Watches tab, which replaces the 
	/// Templates tab at runtime to allow the user to watch Lua values.
	/// </summary>
	public partial class DialogueEditorWindow {

		[Serializable]
		public class Watch {
			public bool isVariable = false;
			public int variableIndex = -1;
			public string expression = string.Empty;
			public string value = string.Empty;

			public Watch() {}

			public Watch(bool isVariable, string expression, string value) {
				this.isVariable = isVariable;
				this.variableIndex = -1;
				this.expression = expression;
				this.value = value;
			}

			public void Evaluate() {
				value = string.IsNullOrEmpty(expression)
					? string.Empty
					: Lua.Run("return " + expression).AsString;
			}
		}

		[SerializeField]
		private List<Watch> watches;

		private bool autoUpdateWatches = false;

		private float watchUpdateFrequency = 1f;

		private double nextWatchUpdateTime = 0f;

		private string[] watchableVariableNames = null;

		private string luaCommand = string.Empty;

		private void DrawWatchSection() {
			if (watches == null) watches = new List<Watch>();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Watches", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			DrawWatchMenu();
			EditorGUILayout.EndHorizontal();

			EditorWindowTools.StartIndentedSection();
			DrawWatches();
			DrawGlobalWatchControls();
			EditorWindowTools.EndIndentedSection();
		}

		private void DrawWatchMenu() {
			if (GUILayout.Button("Menu", "MiniPullDown", GUILayout.Width(56))) {
				GenericMenu menu = new GenericMenu();
				menu.AddItem(new GUIContent("Add Watch"), false, AddWatch);
				menu.AddItem(new GUIContent("Add Variable"), false, AddVariable);
				menu.AddItem(new GUIContent("Reset"), false, ResetWatches);
				menu.ShowAsContext();
			}
		}

		private void ResetWatches() {
			watches.Clear();
		}

		private void AddWatch() {
			watches.Add(new Watch(false, string.Empty, string.Empty));
		}

		private void AddVariable() {
			watches.Add(new Watch(true, string.Empty, string.Empty));
		}

		private void DrawWatches() {
			Watch watchToDelete = null;
			foreach (var watch in watches) {
				EditorGUILayout.BeginHorizontal();
				if (watch.isVariable) {
					DrawWatchVariableNamePopup(watch);
				} else {
					watch.expression = EditorGUILayout.TextField(watch.expression);
				}
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.TextField(watch.value);
				EditorGUI.EndDisabledGroup();
				if (GUILayout.Button(new GUIContent("Update", "Re-evaluate now."), EditorStyles.miniButton, GUILayout.Width(56))) {
					watch.Evaluate();
				}
				if (GUILayout.Button(new GUIContent(" ", "Delete this watch."), "OL Minus", GUILayout.Width(27), GUILayout.Height(16))) { 
					watchToDelete = watch;
				}
				EditorGUILayout.EndHorizontal();
			}
			if (watchToDelete != null) watches.Remove(watchToDelete);
		}

		private void DrawGlobalWatchControls() {
			EditorGUILayout.BeginHorizontal();
			autoUpdateWatches = EditorGUILayout.ToggleLeft("Auto-Update", autoUpdateWatches, GUILayout.Width(100));
			watchUpdateFrequency = EditorGUILayout.FloatField(watchUpdateFrequency, GUILayout.Width(128));
			GUILayout.FlexibleSpace();
			EditorGUI.BeginDisabledGroup(watches.Count == 0);
			if (GUILayout.Button(new GUIContent("Update All", "Re-evaluate all now."), EditorStyles.miniButton, GUILayout.Width(56+27))) {
				UpdateAllWatches();
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();
		}

		private void DrawWatchVariableNamePopup(Watch watch) {
			if (watchableVariableNames == null || watchableVariableNames.Length == 0) {
				List<string> variableNames = new List<string>();
				if (database != null) {
					foreach (var variable in database.variables) {
						variableNames.Add(variable.Name);
					}
				}
				watchableVariableNames = variableNames.ToArray();
			}
			int newIndex = EditorGUILayout.Popup(watch.variableIndex, watchableVariableNames);
			if (newIndex != watch.variableIndex) {
				watch.variableIndex = newIndex;
				if (0 <= watch.variableIndex && watch.variableIndex < watchableVariableNames.Length) {
					watch.expression = string.Format("Variable[\"{0}\"]", DialogueLua.StringToTableIndex(watchableVariableNames[watch.variableIndex]));
				} else {
					watch.expression = string.Empty;
				}
				watch.Evaluate();
			}
		}

		private void UpdateRuntimeWatchesTab() {
			if (autoUpdateWatches &&  EditorApplication.timeSinceStartup > nextWatchUpdateTime) {
				UpdateAllWatches();
			}
		}

		private void UpdateAllWatches() {
			foreach (var watch in watches) {
				watch.Evaluate();
			}
			Repaint();
			ResetWatchTime();
		}

		private void ResetWatchTime() {
			nextWatchUpdateTime = EditorApplication.timeSinceStartup + watchUpdateFrequency;
		}

		private bool IsLuaWatchBarVisible {
			get { return Application.isPlaying && (toolbar.Current == Toolbar.Tab.Templates); }
		}

		private void DrawLuaWatchBar() {
			EditorWindowTools.DrawHorizontalLine();
			EditorGUILayout.BeginHorizontal();
			GUI.SetNextControlName("LuaEmptyLabel");
			EditorGUILayout.LabelField(string.Empty, GUILayout.Width(8));
			luaCommand = EditorGUILayout.TextField(string.Empty, luaCommand);
			if (GUILayout.Button("Clear", "ToolbarSeachCancelButton")) {
				Debug.Log ("Clearing!");
				luaCommand = string.Empty;
				GUI.FocusControl("LuaEmptyLabel"); // Need to deselect field to clear text field's display.
			}
			if (GUILayout.Button("Run", EditorStyles.miniButton, GUILayout.Width(32))) {
				Debug.Log ("Running: " + luaCommand);
				Lua.Run(luaCommand, true);
				luaCommand = string.Empty;
				GUI.FocusControl("LuaEmptyLabel"); // Need to deselect field to clear text field's display.
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.LabelField(string.Empty, GUILayout.Height(1));
		}
		
	}

}