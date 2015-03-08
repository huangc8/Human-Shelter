using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem.UnityGUI {

	/// <summary>
	/// This script replaces the normal continue button functionality with
	/// a two-stage process. If the typewriter effect is still playing, it
	/// simply stops the effect. Otherwise it sends OnContinue to the UI.
	/// </summary>
	[AddComponentMenu("Dialogue System/UI/Unity GUI/Controls/Effects/Continue Button Fast Forward")]
	public class ContinueButtonFastForward : MonoBehaviour {

		public UnityDialogueUI dialogueUI;
		
		public TypewriterEffect typewriterEffect;

		public void OnFastForward() {
			if ((typewriterEffect != null) && typewriterEffect.IsPlaying) {
				typewriterEffect.Stop();
			} else {
				if (dialogueUI != null) dialogueUI.OnContinue();
			}
		}
		
	}

}
