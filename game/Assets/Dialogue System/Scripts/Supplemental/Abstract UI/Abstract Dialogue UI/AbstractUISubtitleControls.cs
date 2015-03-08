using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Abstract subtitle controls. Each GUI system implementation derives its own subclass
	/// from this.
	/// </summary>
	[System.Serializable]
	public abstract class AbstractUISubtitleControls : AbstractUIControls {

		protected Subtitle currentSubtitle = null;
		
		/// <summary>
		/// Gets a value indicating whether text has been assigned to the subtitle controls.
		/// </summary>
		/// <value>
		/// <c>true</c> if it has text; otherwise, <c>false</c>.
		/// </value>
		public abstract bool HasText { get; }
		
		/// <summary>
		/// Sets the subtitle controls' contents.
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle.
		/// </param>
		public abstract void SetSubtitle(Subtitle subtitle);
		
		/// <summary>
		/// Clears the subtitle controls' contents.
		/// </summary>
		public abstract void ClearSubtitle();

		/// <summary>
		/// Hides the continue button.
		/// </summary>
		public virtual void HideContinueButton() {}
		
		/// <summary>
		/// Shows the subtitle controls.
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle.
		/// </param>
		public void ShowSubtitle(Subtitle subtitle) {
			if ((subtitle != null) && !string.IsNullOrEmpty(subtitle.formattedText.text)) {
				currentSubtitle = subtitle;
				SetSubtitle(subtitle);
				Show();
			} else {
				currentSubtitle = null;
				ClearSubtitle();
				Hide();
			}
		}

		/// <summary>
		/// Sets the portrait texture to use in the subtitle if the named actor is the speaker.
		/// </summary>
		/// <param name="actorName">Actor name in database.</param>
		/// <param name="portraitTexture">Portrait texture.</param>
		public virtual void SetActorPortraitTexture(string actorName, Texture2D portraitTexture) {
		}
		
	}

}
