using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Abstract alert message controls. Each GUI system implementation derives its own subclass
	/// from this.
	/// </summary>
	[System.Serializable]
	public abstract class AbstractUIAlertControls : AbstractUIControls {
		
		/// <summary>
		/// Gets a value indicating whether an alert is visible.
		/// </summary>
		/// <value>
		/// <c>true</c> if visible; otherwise, <c>false</c>.
		/// </value>
		public abstract bool IsVisible { get; }
		
		/// <summary>
		/// Sets the message text of an alert.
		/// </summary>
		/// <param name='message'>
		/// Message.
		/// </param>
		/// <param name='duration'>
		/// Duration that message will be shown. Used by subclasses to set up fade durations.
		/// </param>
		public abstract void SetMessage(string message, float duration);
		
		protected float alertDoneTime = 0;
		
		/// <summary>
		/// Has the duration passed for the currently-showing alert?
		/// </summary>
		/// <value>
		/// <c>true</c> if done; otherwise, <c>false</c>.
		/// </value>
		public bool IsDone {
			get { return (DialogueTime.time > alertDoneTime); }
		}
		
		/// <summary>
		/// Sets the GUI controls and shows a message.
		/// </summary>
		/// <param name='message'>
		/// Message to show.
		/// </param>
		/// <param name='duration'>
		/// Duration in seconds.
		/// </param>
		public void ShowMessage(string message, float duration) {
			if (!string.IsNullOrEmpty(message)) {
				alertDoneTime = DialogueTime.time + duration;
				SetMessage(message, duration);
				Show();
			} else {
				Hide();
			}
		}
		
	}

}
