using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// This is the base class for all dialogue event trigger components.
	/// </summary>
	public class DialogueEventStarter : MonoBehaviour {
	
		/// <summary>
		/// Set <c>true</c> if this event should only happen once.
		/// </summary>
		public bool once = false;
		
		protected void DestroyIfOnce() {
			if (once) Destroy(this);
		}
		
	}

}
