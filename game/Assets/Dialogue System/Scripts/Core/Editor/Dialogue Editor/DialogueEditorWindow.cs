using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// Dialogue database editor window. This part adds the Dialogue Editor menu items to Unity.
	/// The Dialogue Editor is a custom editor window. Its functionality is split into
	/// separate files that constitute partial class definitions. Each file handles one
	/// aspect of the Dialogue Editor, such as the Actors tab or the Items tab.
	/// </summary>
	public partial class DialogueEditorWindow : EditorWindow {

		[MenuItem("Window/Dialogue System/Tools/Dialogue Editor", false, 2)]
		public static void OpenDialogueEditorWindow() {
			EditorWindow.GetWindow(typeof(DialogueEditorWindow), false, "Dialogue");
		}

	}

}