using UnityEngine;
using PixelCrushers.DialogueSystem.UnityGUI;

public class ScaleFontSize : MonoBehaviour {
	[System.Serializable]
	public class StyleScale {
		public string styleName = string.Empty;
		public float scaleFactor = 0.04f;
	}
	public StyleScale[] styles = new StyleScale[0];
	private GUIRoot guiRoot = null;
	private float lastScreenHeight = 0f;
	void Awake() {
		guiRoot = GetComponent<GUIRoot>();
	}
	void OnGUI() {
		if (guiRoot == null || guiRoot.guiSkin == null) return;
		if (Screen.height == lastScreenHeight) return;
		lastScreenHeight = Screen.height;
		foreach (var style in styles) {
			GUIStyle guiStyle = guiRoot.guiSkin.GetStyle(style.styleName);
			if (guiStyle != null) {
				//guiStyle.fontSize = (int) (style.scaleFactor * Screen.height);
				guiStyle.fontSize = Screen.width/90;

				if(style.styleName == "Button")
				{
					guiStyle.fontSize = Screen.width/75;
				}
				if(style.styleName == "Label")
				{
					guiStyle.fontSize = Screen.width/65;
				}
				if(style.styleName == "Subtitle")
				{
					guiStyle.fontSize = Screen.width/75;
					guiStyle.padding = new RectOffset(Screen.width/73,Screen.width/30,Screen.height/18,Screen.height/70);
				}
				// guiStyle.fixedHeight = 0;
				// guiStyle.fixedWidth = 0;
			}
		}
		guiRoot.ManualRefresh();
	}
}