using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using PixelCrushers.DialogueSystem.UnityGUI;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// This class defines the menu items in the Dialogue System menu that come from the 
	/// Supplemental script subfolder.
	/// </summary>
	static public class DialogueSystemSupplementalMenuItems {

		#region Game Objects
		
		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Root", false, 0)]
		public static void CreateUnityGUIRoot() {
			AddChildGameObject("GUI Root", typeof(GUIRoot));
		}

		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Label", false, 1)]
		public static void CreateUnityGUILabel() {
			AddChildGameObject("GUI Label", typeof(GUILabel));
		}

		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Button", false, 2)]
		public static void CreateUnityGUIButton() {
			AddChildGameObject("GUI Button", typeof(GUIButton));
		}
		
		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Image", false, 3)]
		public static void CreateUnityGUIImage() {
			AddChildGameObject("GUI Image", typeof(GUIImage));
		}
		
		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Progress Bar", false, 4)]
		public static void CreateUnityGUIProgressBar() {
			AddChildGameObject("GUI Progress Bar", typeof(GUIProgressBar));
		}
		
		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Panel", false, 5)]
		public static void CreateUnityGUIPanel() {
			AddChildGameObject("GUI Panel", typeof(GUIControl));
		}
		
		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Window", false, 6)]
		public static void CreateUnityGUIWindow() {
			AddChildGameObject("GUI Window", typeof(GUIWindow));
		}
		
		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Scroll View", false, 7)]
		public static void CreateUnityGUIScrollView() {
			AddChildGameObject("GUI Scroll View", typeof(GUIScrollView));
		}
		
		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Text Field", false, 8)]
		public static void CreateUnityGUITextField() {
			AddChildGameObject("GUI Text Field", typeof(GUITextField));
		}
		
		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Dialogue UI (top-level)", false, 9)]
		public static void CreateUnityDialogueUI() {
			GameObject uiObject = AddChildGameObject("Unity Dialogue UI", typeof(UnityDialogueUI));
			Selection.activeGameObject = uiObject;
			GameObject root = AddChildGameObject("GUI Root", typeof(GUIRoot));
			Selection.activeGameObject = root;
		}
		
		[MenuItem("GameObject/Create Other/Dialogue System/Unity GUI/Quest Log Window", false, 10)]
		public static void CreateUnityGUIQuestLogWindow() {

			// Create GUI root:
			GameObject questLogWindowObject = AddChildGameObject("Quest Log Window", typeof(UnityGUIQuestLogWindow));
			Selection.activeGameObject = questLogWindowObject;
			GameObject root = AddChildGameObject("GUI Root", typeof(GUIRoot));
			Selection.activeGameObject = root;
			GameObject window = AddChildGameObject("Window", typeof(GUIWindow));
			GameObject abandonQuestPopup = AddChildGameObject("Abandon Quest Popup", typeof(GUIWindow));

			// Create quest window:
			Selection.activeObject = window;
			GameObject scrollView = AddChildGameObject("Scroll View", typeof(GUIScrollView));
			Selection.activeGameObject = window;
			ScaledRect activeButtonRect = new ScaledRect(ScaledRectAlignment.TopCenter, ScaledRectAlignment.TopRight, ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), ScaledValue.FromNormalizedValue(0.5f), ScaledValue.FromNormalizedValue(0.1f), 0, 0);
			GUIButton activeButton = CreateButton(activeButtonRect, "Active Quests", "ClickShowActiveQuests", questLogWindowObject);
			ScaledRect completedButtonRect = new ScaledRect(ScaledRectAlignment.TopCenter, ScaledRectAlignment.TopLeft, ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), ScaledValue.FromNormalizedValue(0.5f), ScaledValue.FromNormalizedValue(0.1f), 0, 0);
			GUIButton completedButton = CreateButton(completedButtonRect, "Completed Quests", "ClickShowCompletedQuests", questLogWindowObject);
			ScaledRect closeRect = new ScaledRect(ScaledRectAlignment.BottomCenter, ScaledRectAlignment.BottomCenter, ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), ScaledValue.FromNormalizedValue(1), ScaledValue.FromNormalizedValue(0.1f), 0, 0);
			CreateButton(closeRect, "Close", "OnClose", questLogWindowObject);
			UnityGUIQuestLogWindow questLogWindow = questLogWindowObject.GetComponent<UnityGUIQuestLogWindow>();
			questLogWindow.guiRoot = root.GetComponent<GUIRoot>();
			questLogWindow.scrollView = scrollView.GetComponent<GUIScrollView>();
			questLogWindow.activeButton = activeButton;
			questLogWindow.completedButton = completedButton;

			// Create abandon quest popup:
			Selection.activeObject = abandonQuestPopup;
			ScaledRect okButtonRect = new ScaledRect(ScaledRectAlignment.BottomCenter, ScaledRectAlignment.BottomRight, ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), ScaledValue.FromNormalizedValue(0.5f), ScaledValue.FromNormalizedValue(0.1f), 0, 0);
			GUIButton okButton = CreateButton(okButtonRect, "Abandon", "ClickConfirmAbandonQuest", questLogWindowObject);
			ScaledRect cancelButtonRect = new ScaledRect(ScaledRectAlignment.BottomCenter, ScaledRectAlignment.BottomLeft, ScaledValue.FromPixelValue(0), ScaledValue.FromPixelValue(0), ScaledValue.FromNormalizedValue(0.5f), ScaledValue.FromNormalizedValue(0.1f), 0, 0);
			GUIButton cancelButton = CreateButton(cancelButtonRect, "Cancel", "ClickCancelAbandonQuest", questLogWindowObject);
			GameObject questTitle = AddChildGameObject("Quest Title Label", typeof(GUILabel));
			questLogWindow.abandonQuestPopup.panel = abandonQuestPopup.GetComponent<GUIWindow>();
			questLogWindow.abandonQuestPopup.ok = okButton;
			questLogWindow.abandonQuestPopup.cancel = cancelButton;
			questLogWindow.abandonQuestPopup.questTitleLabel = questTitle.GetComponent<GUILabel>();

			// Select main window:
			Selection.activeGameObject = questLogWindowObject;
		}
		
		private static GameObject AddChildGameObject(string name, System.Type componentType) {
			GameObject gameObject = new GameObject(name, componentType);
			if (Selection.activeGameObject != null) {
				gameObject.transform.parent = Selection.activeGameObject.transform;
			} else {
				Selection.activeGameObject = gameObject;
			}
			return gameObject;
		}
		
		private static GUIButton CreateButton(ScaledRect scaledRect, string name, string message, GameObject target) {
			GameObject buttonObject = AddChildGameObject(name, typeof(GUIButton));
			GUIButton button = buttonObject.GetComponent<GUIButton>();
			button.scaledRect = scaledRect;
			button.text = name;
			button.message = message;
			button.target = target.transform;
			return button;
		}
		
		#endregion
		

	}
}
