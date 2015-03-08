using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// This class defines the menu items in the Dialogue System menu (except for custom Editor Windows, 
	/// which define their own menu items).
	/// </summary>
	static public class DialogueSystemMenuItems {

		#if ACADEMIC
		//================================== ACADEMIC LICENSE CODE: ======================================
		[MenuItem("Window/Dialogue System/*Academic License*", false, 0)]
		static public void HelpAcademicLicense() {

		}
		
		[MenuItem("Window/Dialogue System/*Academic License*", true, 0)]
		static bool ValidateHelpAcademicLicense() {
			return false;
		}		
		//=========================================================================================
		#endif

		[MenuItem("Window/Dialogue System/Help/Manual", false, 0)]
		static public void HelpUserManual() {
			Application.OpenURL("http://www.pixelcrushers.com/dialogue_system/manual/html/");
		}
		
		[MenuItem("Window/Dialogue System/Help/Video Tutorials", false, 1)]
		static public void HelpVideoTutorials() {
			Application.OpenURL("http://www.pixelcrushers.com/dialogue-system-tutorials/");
		}
		
		//[MenuItem("Window/Dialogue System/Help/Quick Start", false, 2)]
		//static public void HelpQuickStart() {
		//	Application.OpenURL("http://www.pixelcrushers.com/dialogue_system/manual/html/quick_start.html");
		//}
		
		[MenuItem("Window/Dialogue System/Help/FAQ", false, 3)]
		static public void HelpFAQ() {
			Application.OpenURL("http://www.pixelcrushers.com/dialogue_system/manual/html/faq.html");
		}
		
		[MenuItem("Window/Dialogue System/Help/Scripting Reference", false, 4)]
		static public void HelpScriptingReference() {
			Application.OpenURL("http://www.pixelcrushers.com/dialogue_system/manual/html/annotated.html");
		}
		
		[MenuItem("Window/Dialogue System/Help/Release Notes", false, 15)]
		static public void HelpLateReleaseNotes() {
			Application.OpenURL("http://www.pixelcrushers.com/dialogue_system/manual/html/release_notes.html");
		}
		
		[MenuItem("Window/Dialogue System/Help/Late-Breaking News", false, 16)]
		static public void HelpLateBreakingNews() {
			Application.OpenURL("http://www.pixelcrushers.com/dialogue-system-late-breaking-news/");
		}
		
		[MenuItem("Window/Dialogue System/Help/Forum", false, 17)]
		static public void HelpForum() {
			Application.OpenURL("http://forum.unity3d.com/threads/204752-Dialogue-System-for-Unity");
		}
		
		[MenuItem("Window/Dialogue System/Help/Report a Bug", false, 17)]
		static public void HelpReportABug() {
			Application.OpenURL("http://www.pixelcrushers.com/support-form/");
		}
		
		// Tools > Converters, Camera Angle Editor priorities = 1, 2
		
		#region Assets
	
		[MenuItem("Assets/Create/Dialogue System/Dialogue Database", false, 0)]
		public static void CreateDialogueDatabase() {
			CreateAsset(CreateDialogueDatabaseInstance(), "Dialogue Database");
		}
		
		public static DialogueDatabase CreateDialogueDatabaseInstance() {
			Template template = Template.FromEditorPrefs();
			DialogueDatabase database = ScriptableObject.CreateInstance<DialogueDatabase>();
			database.actors.Add(template.CreateActor(1, "Player", true));
			database.variables.Add(template.CreateVariable(1, "Alert", string.Empty));
			return database;
		}
		
		[MenuItem("Assets/Create/Dialogue System/Localized Text Table", false, 0)]
		public static void CreateLocalizedTextTable() {
			CreateAsset(CreateLocalizedTextTableInstance(), "Localized Text");
		}
		
		private static LocalizedTextTable CreateLocalizedTextTableInstance() {
			LocalizedTextTable table = ScriptableObject.CreateInstance<LocalizedTextTable>();
			table.languages = new List<string> { "Default" };
			return table;
		}
		
		public static void CreateAsset(Object asset, string defaultAssetName) {
			string path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if (string.IsNullOrEmpty(path)) {
				path = "Assets";
			} else if (!string.IsNullOrEmpty(Path.GetExtension(path))) {
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/New {1}.asset", path, defaultAssetName));
			AssetDatabase.CreateAsset(asset, assetPathAndName);
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;	
		}
		
		#endregion
		
		#region Game Objects
		
		[MenuItem("GameObject/Create Other/Dialogue System/Dialogue Manager", false, 0)]
		public static void CreateDialogueSystemGameObject() {
			GameObject gameObject = new GameObject("Dialogue Manager", typeof(DialogueSystemController));
			Selection.activeGameObject = gameObject;
		}
		
		[MenuItem("GameObject/Create Other/Dialogue System/Lua Console", false, 1)]
		public static void CreateLuaConsole() {
			GameObject gameObject = new GameObject("Lua Console", typeof(LuaConsole));
			Selection.activeGameObject = gameObject;
		}
		
		#endregion
		
		#region Generic AddComponent
		
		public static void AddComponentToSelection<T>() where T : MonoBehaviour {
			if (Selection.activeGameObject == null) return;
			Selection.activeGameObject.AddComponent<T>();
		}
		
		#endregion
		
	}
}
