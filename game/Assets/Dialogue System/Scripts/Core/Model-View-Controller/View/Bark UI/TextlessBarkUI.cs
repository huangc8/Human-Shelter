using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// A textless implementation of IBarkUI. This implementation doesn't show bark text.
	/// It's useful if everything happens in a sequence instead (such as audio and animation),
	/// since the Dialogue System requires some kind of a bark UI on the barker.
	/// </summary>
	[AddComponentMenu("Dialogue System/UI/Miscellaneous/Textless Bark UI")]
	public class TextlessBarkUI : MonoBehaviour, IBarkUI {

		public bool IsPlaying { get { return false; } }

		/// <summary>
		/// Barks a subtitle. In this implementation, this method doesn't actually do anything.
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle to bark.
		/// </param>
		public void Bark(Subtitle subtitle) {}

	}

}
