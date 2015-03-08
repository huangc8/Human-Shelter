using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// Specifies the orderings that can be used for a list of barks.
	/// </summary>
	public enum BarkOrder { 
		Random, 
		Sequential 
	}
	
	/// <summary>
	/// Keeps track of a character's current bark. This allows the BarkController to iterate 
	/// through a list of barks.
	/// </summary>
	public class BarkHistory {
		
		public BarkOrder order;
		public int index;
		
		public BarkHistory(BarkOrder order) {
			this.order = order;
			this.index = 0;
		}
		
		public int GetNextIndex(int numEntries) {
			if (order == BarkOrder.Random) {
				return Random.Range(0, numEntries);
			} else {
				int result = (index % numEntries);
				index = ((index + 1) % numEntries);
				return result;
			}
		}
			
	}
	
	/// <summary>
	/// Specifies how to handle bark subtitles:
	/// 
	/// - SameAsDialogueManager: Use the same setting as the dialogue UI currently assigned to the
	/// DialogueManager.
	/// - Show: Always show using the bark UI on the character. (See IBarkUI)
	/// - Hide: Never show.
	/// </summary>
	public enum BarkSubtitleSetting {
		SameAsDialogueManager,
		Show,
		Hide
	}

	/// <summary>
	/// BarkController is a static utility class provides a method to make characters bark.
	/// </summary>
	public static class BarkController {

		/// <summary>
		/// Gets the last sequencer created by BarkController.Bark().
		/// </summary>
		/// <value>The last sequencer.</value>
		public static Sequencer LastSequencer { get; private set; }

		static BarkController() {
			LastSequencer = null;
		}
	
		/// <summary>
		/// Attempts to make a character bark. This is a coroutine; you must start it using
		/// StartCoroutine() or Unity will hang. Shows a line from the named conversation, plays 
		/// the sequence, and sends OnBarkStart/OnBarkEnd messages to the participants.
		/// </summary>
		/// <param name='conversationTitle'>
		/// Title of conversation to pull bark lines from.
		/// </param>
		/// <param name='speaker'>
		/// Speaker performing the bark.
		/// </param>
		/// <param name='listener'>
		/// Listener that the bark is directed to; may be <c>null</c>.
		/// </param>
		/// <param name='barkHistory'>
		/// Bark history used to keep track of the most recent bark so this method can iterate 
		/// through them in a specified order.
		/// </param>
		/// <param name='database'>
		/// The dialogue database to use. If <c>null</c>, uses DialogueManager.MasterDatabase.
		/// </param>
		public static IEnumerator Bark(string conversationTitle, Transform speaker, Transform listener, BarkHistory barkHistory, DialogueDatabase database = null) {
			if (string.IsNullOrEmpty(conversationTitle) && DialogueDebug.LogWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): conversation title is blank", new System.Object[] { DialogueDebug.Prefix, speaker, listener }), speaker);
			if ((speaker == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' speaker is null", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversationTitle }));
			if (string.IsNullOrEmpty(conversationTitle) || (speaker == null)) yield break;
			IBarkUI barkUI = speaker.GetComponentInChildren(typeof(IBarkUI)) as IBarkUI;
			if ((barkUI == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' speaker has no bark UI", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversationTitle }), speaker);
			ConversationModel conversationModel = new ConversationModel(database ?? DialogueManager.MasterDatabase, conversationTitle, speaker, listener, DialogueManager.AllowLuaExceptions, DialogueManager.IsDialogueEntryValid);
			ConversationState firstState = conversationModel.FirstState;
			if ((firstState == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' has no START entry", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversationTitle }), speaker);
			if (!firstState.HasAnyResponses && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' has no valid bark at this time", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversationTitle }), speaker);
			if ((firstState != null) && firstState.HasAnyResponses) {
				try {
					InformParticipants("OnBarkStart", speaker, listener);
					Response[] responses = firstState.HasNPCResponse ? firstState.npcResponses : firstState.pcResponses;
					int index = (barkHistory ?? new BarkHistory(BarkOrder.Random)).GetNextIndex(responses.Length);
					DialogueEntry barkEntry = responses[index].destinationEntry;
					if ((barkEntry == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' bark entry is null", DialogueDebug.Prefix, speaker, listener, conversationTitle), speaker);
					if (barkEntry != null) {
						ConversationState barkState = conversationModel.GetState(barkEntry, false);
						if (firstState.HasNPCResponse) {
							CharacterInfo tempInfo = barkState.subtitle.speakerInfo;
							barkState.subtitle.speakerInfo = barkState.subtitle.listenerInfo;
							barkState.subtitle.listenerInfo = tempInfo;
						}
						if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}'", DialogueDebug.Prefix, speaker, listener, barkState.subtitle.formattedText.text), speaker);

						// Show the bark subtitle:
						if (((barkUI == null) || !(barkUI as MonoBehaviour).enabled) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' bark UI is null or disabled", new System.Object[] { DialogueDebug.Prefix, speaker, listener, barkState.subtitle.formattedText.text }), speaker);
						if ((barkUI != null) && (barkUI as MonoBehaviour).enabled) {
							barkUI.Bark(barkState.subtitle);
						}

						// Run Lua:
						if (!string.IsNullOrEmpty(barkState.subtitle.dialogueEntry.userScript)) {
							Lua.Run(barkState.subtitle.dialogueEntry.userScript, DialogueDebug.LogInfo, false);
						}
						
						// Play the sequence:
						Sequencer sequencer = null;
						if (!string.IsNullOrEmpty(barkState.subtitle.sequence)) {
							sequencer = DialogueManager.PlaySequence(barkState.subtitle.sequence, speaker, listener, false, false);
							sequencer.entrytag = barkState.subtitle.entrytag;
						}
						LastSequencer = sequencer;
						while (((sequencer != null) && sequencer.IsPlaying) || ((barkUI != null) && barkUI.IsPlaying)) {
							yield return null;
						}
						if (sequencer != null) GameObject.Destroy(sequencer);
					}
				} finally {
					InformParticipants("OnBarkEnd", speaker, listener);
				}
			}
		}
		
		/// <summary>
		/// Attempts to make a character bark. This is a coroutine; you must start it using
		/// StartCoroutine() or Unity will hang. Shows a specific subtitle and plays the sequence,
		/// but does not send OnBarkStart/OnBarkEnd messages to the participants. This optimized version
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle to bark.
		/// </param>
		/// <param name='speaker'>
		/// Speaker performing the bark.
		/// </param>
		/// <param name='listener'>
		/// Listener that the bark is directed to; may be <c>null</c>.
		/// </param>
		/// <param name='barkUI'>
		/// The bark UI to bark with.
		/// </param>
		public static IEnumerator Bark(Subtitle subtitle, Transform speaker, Transform listener, IBarkUI barkUI) {
			if ((subtitle == null) || (subtitle.speakerInfo == null)) yield break;
			if ((barkUI == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' speaker has no bark UI", new System.Object[] { DialogueDebug.Prefix, speaker, listener, subtitle.formattedText.text }), speaker);
			if (((barkUI == null) || !(barkUI as MonoBehaviour).enabled) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' bark UI is null or disabled", new System.Object[] { DialogueDebug.Prefix, speaker, listener, subtitle.formattedText.text }), speaker);
			if ((barkUI != null) && (barkUI as MonoBehaviour).enabled) {
				barkUI.Bark(subtitle);
			}
			Sequencer sequencer = null;
			if (!string.IsNullOrEmpty(subtitle.sequence)) {
				sequencer = DialogueManager.PlaySequence(subtitle.sequence, speaker, listener, false, false);
				sequencer.entrytag = subtitle.entrytag;
			}
			LastSequencer = sequencer;
			while (((sequencer != null) && sequencer.IsPlaying) || ((barkUI != null) && barkUI.IsPlaying)) {
				yield return null;
			}
			if (sequencer != null) GameObject.Destroy(sequencer);
		}
		
		/// <summary>
		/// Attempts to make a character bark. This is a coroutine; you must start it using
		/// StartCoroutine() or Unity will hang. Shows a specific subtitle and plays the sequence,
		/// but does not send OnBarkStart/OnBarkEnd messages to the participants.
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle to bark.
		/// </param>
		/// <param name='skipSequence'>
		/// If `true`, don't play the sequence associated with the subtitle.
		/// </param>
		public static IEnumerator Bark(Subtitle subtitle, bool skipSequence = false) {
			if ((subtitle == null) || (subtitle.speakerInfo == null)) yield break;
			Transform speaker = subtitle.speakerInfo.transform;
			Transform listener = (subtitle.listenerInfo != null) ? subtitle.listenerInfo.transform : null;
			IBarkUI barkUI = speaker.GetComponentInChildren(typeof(IBarkUI)) as IBarkUI;
			if ((barkUI == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' speaker has no bark UI", new System.Object[] { DialogueDebug.Prefix, speaker, listener, subtitle.formattedText.text }), speaker);
			if (((barkUI == null) || !(barkUI as MonoBehaviour).enabled) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' bark UI is null or disabled", new System.Object[] { DialogueDebug.Prefix, speaker, listener, subtitle.formattedText.text }), speaker);
			if ((barkUI != null) && (barkUI as MonoBehaviour).enabled) {
				barkUI.Bark(subtitle);
			}
			Sequencer sequencer = null;
			if (!(skipSequence || string.IsNullOrEmpty(subtitle.sequence))) {
				sequencer = DialogueManager.PlaySequence(subtitle.sequence, speaker, listener, false, false);
				sequencer.entrytag = subtitle.entrytag;
			}
			LastSequencer = sequencer;
			while (((sequencer != null) && sequencer.IsPlaying) || ((barkUI != null) && barkUI.IsPlaying)) {
				yield return null;
			}
			if (sequencer != null) GameObject.Destroy(sequencer);
		}
		
		/// <summary>
		/// Broadcasts a message to the participants in a bark. Used to send the OnBarkStart and
		/// OnBarkEnd messages to the speaker and listener.
		/// </summary>
		/// <param name='message'>
		/// Message (i.e., OnBarkStart or OnBarkEnd).
		/// </param>
		/// <param name='speaker'>
		/// Speaker.
		/// </param>
		/// <param name='listener'>
		/// Listener.
		/// </param>
		private static void InformParticipants(string message, Transform speaker, Transform listener) {
			if (speaker != null) {
				speaker.BroadcastMessage(message, speaker, SendMessageOptions.DontRequireReceiver);
				if ((listener != null) && (listener != speaker)) {
					listener.BroadcastMessage(message, speaker, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	
	}
	
}
