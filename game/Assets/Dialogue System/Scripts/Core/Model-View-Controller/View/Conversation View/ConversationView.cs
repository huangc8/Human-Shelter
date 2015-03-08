using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {
	
	public delegate void DialogueEntrySpokenDelegate(Subtitle subtitle);
	
	/// <summary>
	/// Handles the user interaction part of a conversation. The ConversationController provides 
	/// the content and receives UI events.
	/// </summary>
	public class ConversationView : MonoBehaviour {
		
		/// <summary>
		/// Called when a subtitle is finished displaying (including text delay and cutscene
		/// sequence).
		/// </summary>
		public event EventHandler FinishedSubtitleHandler = null;
		
		/// <summary>
		/// Called when the player selects a response.
		/// </summary>
		public event EventHandler<SelectedResponseEventArgs> SelectedResponseHandler = null;
		
		private IDialogueUI ui = null;
		private Sequencer sequencer = null;
		private DisplaySettings settings = null;
		private Subtitle lastNPCSubtitle = null;
		private Subtitle lastPCSubtitle = null;
		private Subtitle lastSubtitle = null;
		private Action CancelledHandler = null;
		private DialogueEntrySpokenDelegate dialogueEntrySpokenHandler = null;
		private bool waitForContinue = false;
		private bool isPlayingResponseMenuSequence = false;

		/// <summary>
		/// Initialize a UI and sequencer with displaySettings.
		/// </summary>
		/// <param name='ui'>
		/// Dialogue UI.
		/// </param>
		/// <param name='sequencer'>
		/// Sequencer.
		/// </param>
		/// <param name='displaySettings'>
		/// Display settings to initiate the UI and sequencer with.
		/// </param>
		public void Initialize(IDialogueUI ui, Sequencer sequencer, DisplaySettings displaySettings, DialogueEntrySpokenDelegate dialogueEntrySpokenHandler) {
			this.ui = ui;
			this.sequencer = sequencer;
			this.settings = displaySettings;
			this.dialogueEntrySpokenHandler = dialogueEntrySpokenHandler;
			ui.Open();
			sequencer.Open();
			ui.SelectedResponseHandler += OnSelectedResponse;
			sequencer.FinishedSequenceHandler += OnFinishedSubtitle;
		}
		
		/// <summary>
		/// Close the conversation view.
		/// </summary>
		public void Close() {
			ui.SelectedResponseHandler -= OnSelectedResponse;
			sequencer.FinishedSequenceHandler -= OnFinishedSubtitle;
			ui.Close();
			sequencer.Close();
			Destroy(this);
		}
		
		/// <summary>
		/// Checks if the player has cancelled the conversation.
		/// </summary>
		public void Update() {
			if (Cancelled() && (CancelledHandler != null)) CancelledHandler();
		}
		
		private bool Cancelled() {
			return settings.inputSettings.cancel.IsDown;
		}
		
		/// <summary>
		/// Starts displaying a subtitle.
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle to display.
		/// </param>
		/// <param name='isPCResponseNext'> 
		/// Indicates whether the next stage is the player or NPC.
		/// </param>
		/// <param name='isPCAutoResponseNext'> 
		/// Indicates whether the next stage is a player auto-response.
		/// </param>
		public void StartSubtitle(Subtitle subtitle, bool isPCResponseMenuNext, bool isPCAutoResponseNext) {
			if (subtitle != null) {
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: {1} says '{2}'", new System.Object[] { DialogueDebug.Prefix, Tools.GetGameObjectName(subtitle.speakerInfo.transform), subtitle.formattedText.text }));
				NotifyParticipantsOnConversationLine(subtitle);
				if (ShouldShowSubtitle(subtitle)) {
					ui.ShowSubtitle(subtitle);
					waitForContinue = ShouldWaitForContinueButton(subtitle, isPCResponseMenuNext, isPCAutoResponseNext);
					var showContinueButton = ShouldShowContinueButton(isPCResponseMenuNext, isPCAutoResponseNext);
					if (waitForContinue) {
						if (string.IsNullOrEmpty(subtitle.formattedText.text) && (subtitle.dialogueEntry.id == 0)) {
							waitForContinue = false;
						}
					}
					if (!showContinueButton) {
						if (ui is AbstractDialogueUI) (ui as AbstractDialogueUI).HideContinueButton(subtitle);
					}
				} else {
					waitForContinue = false;
				}
				sequencer.SetParticipants(subtitle.speakerInfo.transform, subtitle.listenerInfo.transform);
				sequencer.entrytag = subtitle.entrytag;
				sequencer.PlaySequence(string.IsNullOrEmpty(subtitle.sequence) ? GetDefaultSequence(subtitle) : PreprocessSequence(subtitle), settings.subtitleSettings.informSequenceStartAndEnd, false);
				if (subtitle.speakerInfo.IsNPC) {
					lastNPCSubtitle = subtitle;
				} else {
					lastPCSubtitle = subtitle;
				}
				lastSubtitle = subtitle;
				if (dialogueEntrySpokenHandler != null) dialogueEntrySpokenHandler(subtitle);
			} else {
				FinishSubtitle();
			}
			CancelledHandler = OnCancelSubtitle;
		}

		private bool ShouldWaitForContinueButton(Subtitle subtitle, bool isPCResponseMenuNext, bool isPCAutoResponseNext) {
			switch (settings.subtitleSettings.continueButton) {
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.Always:
				return true;
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.Never:
				return false;
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalBeforeResponseMenu:
				return !isPCResponseMenuNext;
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotBeforeResponseMenu:
				return !isPCResponseMenuNext;
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalBeforePC:
				return !(isPCResponseMenuNext || isPCAutoResponseNext);
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotBeforePC:
				return !(isPCResponseMenuNext || isPCAutoResponseNext);
			default:
				return false;
			}
		}

		private bool ShouldShowContinueButton(bool isPCResponseMenuNext, bool isPCAutoResponseNext) {
			// Should we show the continue button? (Even if optional and not waiting for it)
			switch (settings.subtitleSettings.continueButton) {
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.Always:
				return true;
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.Never:
				return false;
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalBeforeResponseMenu:
				return true;
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotBeforeResponseMenu:
				return !isPCResponseMenuNext;
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.OptionalBeforePC:
				return true;
			case DisplaySettings.SubtitleSettings.ContinueButtonMode.NotBeforePC:
				return !(isPCResponseMenuNext || isPCAutoResponseNext);
			default: 
				return false;
			}
		}
		
		/// <summary>
		/// Shows the most recently displayed subtitle.
		/// </summary>
		public void ShowLastNPCSubtitle() {
			if (ShouldShowLastNPCSubtitle()) ui.ShowSubtitle(lastNPCSubtitle);
			FinishSubtitle();
		}
		
		private bool ShouldShowLastNPCSubtitle() {
			return (settings != null) && (settings.subtitleSettings != null) &&
				settings.subtitleSettings.showNPCSubtitlesWithResponses && 
				(lastNPCSubtitle != null) && 
				(lastNPCSubtitle.speakerInfo.characterType == CharacterType.NPC);
		}
		
		private bool ShouldShowLastPCSubtitle() {
			return (settings != null) && (settings.subtitleSettings != null) &&
				settings.subtitleSettings.showNPCSubtitlesWithResponses &&
				settings.subtitleSettings.allowPCSubtitleReminders &&
				(lastPCSubtitle != null) && (lastSubtitle == lastPCSubtitle) &&
				(lastPCSubtitle.speakerInfo.characterType == CharacterType.PC);
		}
		
		private bool ShouldShowSubtitle(Subtitle subtitle) {
			if ((subtitle != null) && (settings != null) && (settings.subtitleSettings != null)) {
				return
					((subtitle.speakerInfo.characterType == CharacterType.NPC) && settings.subtitleSettings.showNPCSubtitlesDuringLine) ||
					((subtitle.speakerInfo.characterType == CharacterType.PC) &&  settings.subtitleSettings.showPCSubtitlesDuringLine);
			} else {
				return false;
			}
		}
		
		public void OnConversationContinue() {
			waitForContinue = false;
			FinishSubtitle();
		}
		
		private void OnCancelSubtitle() {
			BroadcastMessage("OnConversationLineCancelled", lastNPCSubtitle, SendMessageOptions.DontRequireReceiver);
			waitForContinue = false;
			FinishSubtitle();
		}
		
		private void FinishSubtitle() {
			if (!waitForContinue) {
				sequencer.Stop();
				if (lastNPCSubtitle != null) ui.HideSubtitle(lastNPCSubtitle);
				if (lastPCSubtitle != null) ui.HideSubtitle(lastPCSubtitle);
				if (FinishedSubtitleHandler != null) FinishedSubtitleHandler(this, EventArgs.Empty);
			}
		}

		private void OnFinishedSubtitle() {
			FinishSubtitle();
		}
		
		/// <summary>
		/// Displays the player response menu.
		/// </summary>
		/// <param name='subtitle'>
		/// Last subtitle, to display as a reminder of what the player is responding to.
		/// </param>
		/// <param name='responses'>
		/// Responses.
		/// </param>
		public void StartResponses(Subtitle subtitle, Response[] responses) {
			PlayResponseMenuSequence(subtitle.responseMenuSequence);
			Subtitle lastSubtitle = ShouldShowLastPCSubtitle()
				? lastPCSubtitle
				: ShouldShowLastNPCSubtitle()
					? lastNPCSubtitle 
					: null;
			ui.ShowResponses(lastSubtitle, responses, settings.inputSettings.responseTimeout);
			CancelledHandler = OnCancelResponseMenu;
		}

		private void PlayResponseMenuSequence(string responseMenuSequence) {
			if (!string.IsNullOrEmpty(responseMenuSequence)) {
				sequencer.FinishedSequenceHandler -= OnFinishedSubtitle;
				sequencer.Stop();
				sequencer.PlaySequence(responseMenuSequence);
				isPlayingResponseMenuSequence = true;
			}
		}

		private void StopResponseMenuSequence() {
			if (isPlayingResponseMenuSequence) {
				sequencer.Stop();
				sequencer.StopAllCoroutines();
				sequencer.FinishedSequenceHandler += OnFinishedSubtitle;
			}
		}
		
		private void OnCancelResponseMenu() {
			BroadcastMessage("OnConversationCancelled", sequencer.Speaker, SendMessageOptions.DontRequireReceiver);
			SelectResponse(new SelectedResponseEventArgs(null));
		}
		
		private void OnSelectedResponse(object sender, SelectedResponseEventArgs e) {
			SelectResponse(e);
		}
		
		public void SelectResponse(SelectedResponseEventArgs e) {
			StopResponseMenuSequence();
			ui.HideResponses();
			if (SelectedResponseHandler != null) SelectedResponseHandler(this, e);
		}
		
		/// <summary>
		/// Gets the default sequence for a subtitle.
		/// </summary>
		/// <returns>
		/// The default sequence.
		/// </returns>
		/// <param name='subtitle'>
		/// Subtitle.
		/// </param>
		public string GetDefaultSequence(Subtitle subtitle) {
			if ((subtitle.speakerInfo.characterType == CharacterType.PC) && !settings.subtitleSettings.showPCSubtitlesDuringLine) {
				return null;
			} else {
				float duration = GetDefaultSubtitleDuration(subtitle.formattedText.text);
				if (string.IsNullOrEmpty(settings.cameraSettings.defaultSequence)) {
					return string.Format("Delay({0})", new System.Object[] { duration });
				} else {
					return settings.cameraSettings.defaultSequence.Replace("{{end}}", duration.ToString());
				}
			}
		}
		
		private float GetDefaultSubtitleDuration(string text) {
			int numCharacters = string.IsNullOrEmpty(text) ? 0 : text.Length;
			return Mathf.Max(settings.subtitleSettings.minSubtitleSeconds, numCharacters / Mathf.Max(1, settings.subtitleSettings.subtitleCharsPerSecond));
		}

		private string PreprocessSequence(Subtitle subtitle) {
			if ((subtitle == null) || (string.IsNullOrEmpty(subtitle.sequence))) return string.Empty;
			if (!subtitle.sequence.Contains("{{end}}")) return subtitle.sequence;
			float duration = GetDefaultSubtitleDuration(subtitle.formattedText.text);
			return subtitle.sequence.Replace("{{end}}", duration.ToString());
		}
		
		private void NotifyParticipantsOnConversationLine(Subtitle subtitle) {
			if (subtitle != null) {
				bool validSpeakerTransform = CharacterInfoHasValidTransform(subtitle.speakerInfo);
				bool validListenerTransform = CharacterInfoHasValidTransform(subtitle.listenerInfo);
				bool speakerIsListener = validSpeakerTransform && validListenerTransform && (subtitle.speakerInfo.transform == subtitle.listenerInfo.transform);
				if (validSpeakerTransform) subtitle.speakerInfo.transform.BroadcastMessage("OnConversationLine", subtitle, SendMessageOptions.DontRequireReceiver);
				if (validListenerTransform && !speakerIsListener) subtitle.listenerInfo.transform.BroadcastMessage("OnConversationLine", subtitle, SendMessageOptions.DontRequireReceiver);
				DialogueManager.Instance.BroadcastMessage("OnConversationLine", subtitle, SendMessageOptions.DontRequireReceiver);
			}
		}
		
		private bool CharacterInfoHasValidTransform(CharacterInfo characterInfo) {
			return (characterInfo != null) && (characterInfo.transform != null);
		}

		/// <summary>
		/// Sets the PC portrait to use for the response menu.
		/// </summary>
		/// <param name="pcTexture">PC texture.</param>
		/// <param name="pcName">PC name.</param>
		public void SetPCPortrait(Texture2D pcTexture, string pcName) {
			AbstractDialogueUI ui = DialogueManager.DialogueUI as AbstractDialogueUI;
			if (ui == null) return;
			ui.SetPCPortrait(pcTexture, pcName);
		}

		/// <summary>
		/// Sets the portrait texture to use in the UI for an actor.
		/// This is used when the SetPortrait() sequencer command changes an actor's image.
		/// </summary>
		/// <param name="actorName">Actor name.</param>
		/// <param name="portraitTexture">Portrait texture.</param>
		public void SetActorPortraitTexture(string actorName, Texture2D portraitTexture) {
			AbstractDialogueUI ui = DialogueManager.DialogueUI as AbstractDialogueUI;
			if (ui == null) return;
			ui.SetActorPortraitTexture(actorName, portraitTexture);
		}

	}

}