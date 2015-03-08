using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// An in-game Lua console presented using Unity GUI. This console is activated by ~(tilde) + L
	/// and allows you to enter Lua commands and view the results. The up and down keys scroll
	/// through previous commands, and Escape closes the console.
	/// </summary>
	/// <remarks>
	/// This class depends on LuaInterface's internal representation of Lua tables.
	/// </remarks>
	[AddComponentMenu("Dialogue System/Miscellaneous/Lua Console")]
	public class LuaConsole : MonoBehaviour {

		/// <summary>
		/// Is the console visible or hidden?
		/// </summary>
		public bool visible = false;

		/// <summary>
		/// The minimum size of the console window.
		/// </summary>
		public Vector2 minSize = new Vector2(256f, 128f);
		
		/// <summary>
		/// The max number of previous commands to remember.
		/// </summary>
		public int maxHistory = 20;
		
		private List<string> history = new List<string>();
		
		private int historyPosition = 0;
		
		private string input = string.Empty;
		
		private string output = string.Empty;
		
		private Rect windowRect = new Rect(0, 0, 0, 0);
		
		private Rect closeButtonRect = new Rect(0, 0, 0, 0);
		
		private Vector2 scrollPosition = new Vector2(0, 0);
		
		/// <summary>
		/// OnGUI draws the console if it's visible, and toggles visibility based on the key 
		/// trigger.
		/// </summary>
		void OnGUI() {
			if (IsConsoleKeyEvent()) visible = !visible;
			if (visible) DrawConsole();
		}
		
		private void DrawConsole() {
			if (windowRect.width <= 0) {
				windowRect = DefineWindowRect();
				closeButtonRect = new Rect(windowRect.width - 30, 2, 26, 16);
			}
			windowRect = GUI.Window(0, windowRect, DrawConsoleWindow, "Lua Console");
		}

		private Rect DefineWindowRect() {
			float width = Mathf.Max(minSize.x, Screen.width / 4f);
			float height = Mathf.Max(minSize.y, Screen.height / 4f);
			return new Rect(Screen.width - width, 0, width, height);				
		}

		private void DrawConsoleWindow(int id) {
			if (IsKeyEvent(KeyCode.Return)) {
				RunLuaCommand();
			} else if (IsKeyEvent(KeyCode.UpArrow)) {
				UseHistory(-1);
			} else if (IsKeyEvent(KeyCode.DownArrow)) {
				UseHistory(1);
			} else if (IsKeyEvent(KeyCode.Escape) || GUI.Button(closeButtonRect, "X")) {
				visible = false;
				return;
			}
			GUI.SetNextControlName("Input");
			GUI.FocusControl("Input");
			if (string.Equals(input, "\n")) input = string.Empty;
			input = GUILayout.TextArea(input);
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
			GUILayout.Label(output);
			GUILayout.EndScrollView();
		}

		private bool IsKeyEvent(KeyCode keyCode) {
			if ((UnityEngine.Event.current.type == EventType.KeyDown) && (UnityEngine.Event.current.keyCode == keyCode)) {
				UnityEngine.Event.current.Use();
				return true;
			} else {
				return false; 
			}
		}
		
		private bool IsConsoleKeyEvent() {
			return 
				(Input.GetKey(KeyCode.BackQuote) && IsKeyEvent(KeyCode.L)) ||
				(Input.GetKey(KeyCode.L) && IsKeyEvent(KeyCode.BackQuote));
		}
		
		private void RunLuaCommand() {
			if (string.IsNullOrEmpty(input)) return;
			try {
				Lua.Result result = Lua.Run(input, DialogueDebug.LogInfo);
				output = "Output: " + GetLuaResultString(result);
			} catch (Exception e) {
				output = "Output: [Exception] " + e.Message;
			}
			history.Add(input);
			if (history.Count >= maxHistory) history.RemoveAt(0);
			historyPosition = history.Count;
			input = string.Empty;
		}
		
		private string GetLuaResultString(Lua.Result result) {
			if (!result.HasReturnValue) return "(no return value)";
			return result.IsTable ? FormatTableResult(result) : result.AsString;
		}
		
		private string FormatTableResult(Lua.Result result) {
			if (!result.IsTable) return result.AsString;
			LuaTableWrapper table = result.AsTable;
			StringBuilder sb = new StringBuilder();
			sb.Append("Table:\n");
			foreach (object key in table.Keys) {
				sb.Append(string.Format("[{0}]: {1}\n", new System.Object[] { key.ToString(), table[key.ToString()].ToString() }));
			}
			return sb.ToString();
		}
		
		private void UseHistory(int direction) {
			historyPosition = Mathf.Clamp(historyPosition + direction, 0, history.Count);
			input = ((history.Count > 0) && (historyPosition < history.Count)) ? history[historyPosition] : string.Empty;
		}
		
	}

}
