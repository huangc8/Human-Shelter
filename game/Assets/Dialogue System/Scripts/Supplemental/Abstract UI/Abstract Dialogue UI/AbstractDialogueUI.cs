using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// This abstract class forms the base for IDialogueUI implementations in Unity GUI, NGUI,
	/// Daikon Forge GUI, and 2D Toolkit. It implements common code so that the specific
	/// implementation for each GUI system need only deal with the data structures specific to
	/// each GUI system.
	/// </summary>
	public abstract class AbstractDialogueUI : MonoBehaviour, IDialogueUI {

		/// <summary>
		/// Gets the user interface root.
		/// </summary>
		/// <value>
		/// The user interface root.
		/// </value>
		public abstract AbstractUIRoot UIRoot { get; }

		/// <summary>
		/// Gets the dialogue controls.
		/// </summary>
		/// <value>
		/// The dialogue controls.
		/// </value>
		public abstract AbstractDialogueUIControls Dialogue { get; }
		
		/// <summary>
		/// Gets the QTE (Quick Time Event) indicators.
		/// </summary>
		/// <value>
		/// The QTE indicators.
		/// </value>
		public abstract AbstractUIQTEControls QTEs { get; }
		
		/// <summary>
		/// Gets the alert message controls.
		/// </summary>
		/// <value>
		/// The alert message controls
		/// </value>
		public abstract AbstractUIAlertControls Alert { get; }
		
		/// <summary>
		/// Occurs when the player selects a response.
		/// </summary>
		public event EventHandler<SelectedResponseEventArgs> SelectedResponseHandler;
		
		/// <summary>
		/// Gets or sets a value indicating whether the dialogue UI (conversation interface) is open.
		/// </summary>
		/// <value>
		/// <c>true</c> if open; otherwise, <c>false</c>.
		/// </value>
		public bool IsOpen { get; set; }

		/// <summary>
		/// Sets up the component.
		/// </summary>
		public virtual void Awake() {
			IsOpen = false;
			SelectedResponseHandler = null;
		}
		
		/// <summary>
		/// Starts this instance by hiding everything. The only exception is if an alert message is
		/// already showing; it keeps this message visible.
		/// </summary>
		public virtual void Start() {
			if ((UIRoot == null) || (Dialogue == null) || (QTEs == null) || (Alert == null)) {
				enabled = false;
			} else {
				UIRoot.Show();
				Dialogue.Hide();
				QTEs.Hide();
				if (!Alert.IsVisible) Alert.Hide();
				if (IsOpen) Open();
				if (!(Alert.IsVisible || IsOpen)) UIRoot.Hide();
			}
		}
		
		/// <summary>
		/// Opens the conversation GUI.
		/// </summary>
		public virtual void Open() {
			Dialogue.ShowPanel();
			UIRoot.Show();
			IsOpen = true;
		}
		
		/// <summary>
		/// Closes the conversation GUI.
		/// </summary>
		public virtual void Close() {
			Dialogue.Hide();
			if (!AreNonDialogueControlsVisible) UIRoot.Hide();
			IsOpen = false;
		}
		
		/// <summary>
		/// Gets a value indicating whether non-conversation controls (e.g., alert message or QTEs)
		/// are visible.
		/// </summary>
		/// <value>
		/// <c>true</c> if non-conversation controls are visible; otherwise, <c>false</c>.
		/// </value>
		protected virtual bool AreNonDialogueControlsVisible {
			get { return Alert.IsVisible || QTEs.AreVisible; }
		}
		
		/// <summary>
		/// Shows an alert.
		/// </summary>
		/// <param name='message'>
		/// Message to show.
		/// </param>
		/// <param name='duration'>
		/// Duration in seconds.
		/// </param>
		public virtual void ShowAlert(string message, float duration) {
			if (!IsOpen) {
				Dialogue.Hide();
				UIRoot.Show();
			}
			Alert.ShowMessage(message, duration);
		}

		/// <summary>
		/// Hides the alert if it's showing.
		/// </summary>
		public virtual void HideAlert() {
			if (Alert.IsVisible) {
				Alert.Hide();
				if (!(IsOpen || QTEs.AreVisible)) UIRoot.Hide();
			}
		}
		
		/// <summary>
		/// Updates this instance by hiding the alert message when it's done.
		/// </summary>
		public virtual void Update() {
			if (Alert.IsVisible && Alert.IsDone) Alert.Hide();
		}
		
		/// <summary>
		/// Shows the subtitle (NPC or PC) based on the character type.
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle to show.
		/// </param>
		public virtual void ShowSubtitle(Subtitle subtitle) {
			SetSubtitle(subtitle, true);
		}
		
		/// <summary>
		/// Hides the subtitle based on its character type (PC or NPC).
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle to hide.
		/// </param>
		public virtual void HideSubtitle(Subtitle subtitle) {
			SetSubtitle(subtitle, false);
		}

		/// <summary>
		/// Hides the continue button. Call this after showing the subtitle.
		/// The ConversationView uses this to hide the continue button if the 
		/// display setting for continue buttons is set to NotBeforeResponseMenu.
		/// </summary>
		/// <param name="subtitle">Subtitle.</param>
		public virtual void HideContinueButton(Subtitle subtitle) {
			AbstractUISubtitleControls subtitleControls = GetSubtitleControls(subtitle);
			if (subtitleControls != null) {
				subtitleControls.HideContinueButton();
			}
		}
		
		/// <summary>
		/// Sets a subtitle's content and visibility.
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle. The speaker recorded in the subtitle determines whether the NPC or
		/// PC subtitle controls are used.
		/// </param>
		/// <param name='value'>
		/// <c>true</c> to show; <c>false</c> to hide.
		/// </param>
		protected virtual void SetSubtitle(Subtitle subtitle, bool value) {
			AbstractUISubtitleControls subtitleControls = GetSubtitleControls(subtitle);
			if (subtitleControls != null) {
				if (value == true) {
					subtitleControls.ShowSubtitle(subtitle);
				} else {
					subtitleControls.Hide();
				}
			}
		}

		/// <summary>
		/// Gets the appropriate subtitle control (PC or NPC) for a subtitle.
		/// </summary>
		/// <returns>
		/// The subtitle controls to use for a subtitle.
		/// </returns>
		/// <param name='subtitle'>
		/// Subtitle to use; the subtitle's speakerInfo property indicates whether the subtitle is
		/// a PC or NPC line.
		/// </param>
		private AbstractUISubtitleControls GetSubtitleControls(Subtitle subtitle) {
			return (subtitle == null)
				? null
				: (subtitle.speakerInfo.characterType == CharacterType.NPC) 
					? Dialogue.NPCSubtitle 
					: Dialogue.PCSubtitle;
		}

		/// <summary>
		/// Shows the player responses menu.
		/// </summary>
		/// <param name='subtitle'>
		/// The last subtitle, shown as a reminder.
		/// </param>
		/// <param name='responses'>
		/// Responses.
		/// </param>
		/// <param name='timeout'>
		/// If not <c>0</c>, the duration in seconds that the player has to choose a response; 
		/// otherwise the currently-focused response is auto-selected. If no response is
		/// focused (e.g., hovered over), the first response is auto-selected. If <c>0</c>,
		/// there is no timeout; the player can take as long as desired to choose a response.
		/// </param>
		public virtual void ShowResponses(Subtitle subtitle, Response[] responses, float timeout) {
			try {
				if (Dialogue == null) {
					Debug.LogError(DialogueDebug.Prefix + ": In ShowResponses(): The dialogue UI's main dialogue controls field is not set.", this);
				} else if (Dialogue.ResponseMenu == null) {
					Debug.LogError(DialogueDebug.Prefix + ": In ShowResponses(): The dialogue UI's response menu controls field is not set.", this);
				} else if (this.transform == null) {
					Debug.LogError(DialogueDebug.Prefix + ": In ShowResponses(): The dialogue UI's transform is null.", this);
				} else {
					Dialogue.ResponseMenu.ShowResponses(subtitle, responses, this.transform);
					if (timeout > 0) Dialogue.ResponseMenu.StartTimer(timeout);
				}
			} catch (NullReferenceException e) {
				Debug.LogError(DialogueDebug.Prefix + ": In ShowResponses(): " + e.Message);
			}
		}
			
		/// <summary>
		/// Hides the player response menu.
		/// </summary>
		public virtual void HideResponses() {
			Dialogue.ResponseMenu.Hide();
		}

		/// <summary>
		/// Shows a QTE indicator.
		/// </summary>
		/// <param name='index'>
		/// Index of the QTE indicator.
		/// </param>
		public virtual void ShowQTEIndicator(int index) {
			QTEs.ShowIndicator(index);
		}
		
		/// <summary>
		/// Hides a QTE indicator.
		/// </summary>
		/// <param name='index'>
		/// Index of the QTE indicator.
		/// </param>
		public virtual void HideQTEIndicator(int index) {
			QTEs.HideIndicator(index);
		}
		
		/// <summary>
		/// Handles response button clicks.
		/// </summary>
		/// <param name='data'>
		/// The Response assigned to the button. This is passed back to the handler.
		/// </param>
		public virtual void OnClick(object data) {
			if (SelectedResponseHandler != null) SelectedResponseHandler(this, new SelectedResponseEventArgs(data as Response));
		}
		
		/// <summary>
		/// Handles the continue button being clicked. This sends OnConversationContinue to the 
		/// DialogueManager object so the ConversationView knows to progress the conversation.
		/// </summary>
		public virtual void OnContinue() {
			if (Alert.IsVisible) HideAlert();
			if (IsOpen)	DialogueManager.Instance.SendMessage("OnConversationContinue", SendMessageOptions.DontRequireReceiver);
		}

		/// <summary>
		/// Sets the PC portrait name and texture.
		/// </summary>
		/// <param name="portraitTexture">Portrait texture.</param>
		/// <param name="portraitName">Portrait name.</param>
		public virtual void SetPCPortrait(Texture2D portraitTexture, string portraitName) {
			Dialogue.ResponseMenu.SetPCPortrait(portraitTexture, portraitName);
		}

		/// <summary>
		/// Sets the portrait texture for an actor.
		/// This is used to immediately update the GUI control if the SetPortrait() sequencer 
		/// command changes the portrait texture.
		/// </summary>
		/// <param name="actorName">Actor name in database.</param>
		/// <param name="portraitTexture">Portrait texture.</param>
		public virtual void SetActorPortraitTexture(string actorName, Texture2D portraitTexture) {
			Dialogue.NPCSubtitle.SetActorPortraitTexture(actorName, portraitTexture);
			Dialogue.PCSubtitle.SetActorPortraitTexture(actorName, portraitTexture);
			Dialogue.ResponseMenu.SetActorPortraitTexture(actorName, portraitTexture);
		}

		/// <summary>
		/// Gets a valid portrait texture. If the provided portraitTexture is null, grabs the default
		/// textures from the database.
		/// </summary>
		/// <returns>The valid portrait texture.</returns>
		/// <param name="actorName">Actor name.</param>
		/// <param name="portraitTexture">Portrait texture.</param>
		public static Texture2D GetValidPortraitTexture(string actorName, Texture2D portraitTexture) {
			if (portraitTexture != null) {
				return portraitTexture;
			} else {
				Actor actor = DialogueManager.MasterDatabase.GetActor(actorName);
				return (actor != null) ? actor.portrait : null;
			}
		}
		
	}

}
