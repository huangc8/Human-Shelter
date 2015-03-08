using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.SequencerCommands {
	
	/// <summary>
	/// Implements sequencer command: "Fade(in|out, [, duration[, webcolor]])".
	/// 
	/// Arguments:
	/// -# in or out.
	/// -# (Optional) Duration in seconds. Default: 1.
	/// -# (Optional) Web color in "\#rrggbb" format. Default: Black.
	/// </summary>
	public class SequencerCommandFade : SequencerCommand {
		
		private const float SmoothMoveCutoff = 0.05f;
		
		private string direction;
		private float duration;
		private Color color;
		private bool fadeIn;
		private Texture texture = null;
		private GUITexture faderGuiTexture = null;
		private GameObject fader = null;
		float startTime;
		float endTime;
		
		public void Start() {
			// Get the values of the parameters:
			direction = GetParameter(0);
			duration = GetParameterAsFloat(1, 0);
			color = Tools.WebColor(GetParameter(2, "#000000"));
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: Fade({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, direction, duration, color }));
			
			if (duration > SmoothMoveCutoff) {
				
				// Create a 1x1 texture:
				Texture2D texture = new Texture2D (1, 1);
				texture.SetPixel(0, 0, color);
				texture.Apply();
				
				// Add a temporary object with a GUITexture component:
				fader = new GameObject("Fader");
				fader.transform.parent = gameObject.transform;
				fader.transform.position = new Vector3 (0.5f, 0.5f, 1000);
				faderGuiTexture = fader.AddComponent<GUITexture>();
				faderGuiTexture.texture = texture;

				// Set up duration:
				startTime = DialogueTime.time;
				endTime = startTime + duration;
				
				fadeIn = string.Equals(direction, "in", System.StringComparison.OrdinalIgnoreCase);

				if (fadeIn) {
					faderGuiTexture.color = new Color(color.r, color.g, color.b, 1);
				} else {
					faderGuiTexture.color = new Color(color.r, color.g, color.b, 0);
				}
				
			} else {
				Stop();
			}
		}
		
		public void Update() {
			// Keep smoothing for the specified duration:
			if ((DialogueTime.time < endTime) && (faderGuiTexture != null)) {
				float elapsed = (DialogueTime.time - startTime) / duration;
				float alpha = fadeIn ? (1 - elapsed) : elapsed;
				faderGuiTexture.color = new Color(color.r, color.g, color.b, alpha);
			} else {
				Stop();
			}
		}
					
		public void OnDestroy() {
			if (faderGuiTexture != null) faderGuiTexture.texture = null;
			if (fader != null) Destroy(fader);
			if (texture != null) Destroy(texture);
		}
		
		
	}

}
