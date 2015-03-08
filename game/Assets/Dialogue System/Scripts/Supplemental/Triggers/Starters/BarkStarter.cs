using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {
		
	/// <summary>
	/// This is the base class for bark trigger components such as BarkOnIdle and BarkTrigger.
	/// </summary>
	public class BarkStarter : ConversationStarter {
	
		/// <summary>
		/// Specifies the order to run through the list of barks.
		/// 
		/// - Random: Choose a random bark from the conversation.
		/// - Sequential: Choose the barks in order from first to last, looping at the end.
		/// </summary>
		public BarkOrder barkOrder = BarkOrder.Random;

		/// <summary>
		/// Are barks allowed during conversations?
		/// </summary>
		public bool allowDuringConversations = false;

		/// <summary>
		/// If ticked, bark info is cached during the first bark. This can reduce stutter
		/// when barking on slower mobile devices, but barks are not reevaluated each time
		/// as the state changes, barks use no em formatting codes, and sequences are not
		/// played with barks.
		/// </summary>
		public bool cacheBarkLines = false;

		/// <summary>
		/// Gets the sequencer used by the current bark, if a bark is playing.
		/// If a bark is not playing, this is undefined. To check if a bark is
		/// playing, check the bark UI's IsPlaying property.
		/// </summary>
		/// <value>The sequencer.</value>
		public Sequencer sequencer { get; private set; }

		private BarkHistory barkHistory;
		
		private bool tryingToBark = false;

		private ConversationState cachedState = null;

		private IBarkUI barkUI = null;
		
		/// <summary>
		/// Gets or sets the bark index for sequential barks.
		/// </summary>
		/// <value>The index of the bark, starting from <c>0</c>.</value>
		public int BarkIndex {
			get { return barkHistory.index; }
			set { barkHistory.index = value; }
		}
		
		void Awake() {
			barkHistory = new BarkHistory(barkOrder);
			sequencer = null;
		}
		
		/// <summary>
		/// Barks if the condition is true.
		/// </summary>
		public void TryBark(Transform target) {
			if (!tryingToBark) {
				tryingToBark = true;
				try {
					if ((condition == null) || condition.IsTrue(target)) {
						if (string.IsNullOrEmpty(conversation)) {
							if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark triggered on {1}, but conversation name is blank.", new System.Object[] { DialogueDebug.Prefix, name }), GetBarker());
						} else if (DialogueManager.IsConversationActive && !allowDuringConversations) {
							if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Bark triggered on {1}, but a conversation is already active.", new System.Object[] { DialogueDebug.Prefix, name }), GetBarker());
						} else if (cacheBarkLines) {
							BarkCachedLine(GetBarker(), target);
						} else {
							DialogueManager.Bark(conversation, GetBarker(), target, barkHistory);
							sequencer = BarkController.LastSequencer;
						}
						DestroyIfOnce();
					}
				} finally {
					tryingToBark = false;
				}				
			}
		}

		private Transform GetBarker() {
			return Tools.Select(conversant, this.transform);
		}

		private string GetBarkerName() {
			return OverrideActorName.GetActorName(GetBarker());
		}

		private void BarkCachedLine(Transform speaker, Transform listener) {
			if (barkUI == null) barkUI = speaker.GetComponentInChildren(typeof(IBarkUI)) as IBarkUI;
			if (cachedState == null) PopulateCache(speaker, listener);
			BarkNextCachedLine(speaker, listener);
		}

		private void PopulateCache(Transform speaker, Transform listener) {
			if (string.IsNullOrEmpty(conversation) && DialogueDebug.LogWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): conversation title is blank", new System.Object[] { DialogueDebug.Prefix, speaker, listener }), speaker);
			ConversationModel conversationModel = new ConversationModel(DialogueManager.MasterDatabase, conversation, speaker, listener, DialogueManager.AllowLuaExceptions, DialogueManager.IsDialogueEntryValid);
			cachedState = conversationModel.FirstState;
			if ((cachedState == null) && DialogueDebug.LogWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' has no START entry", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversation }), speaker);
			if (!cachedState.HasAnyResponses && DialogueDebug.LogWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' has no valid bark lines", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversation }), speaker);
		}

		private void BarkNextCachedLine(Transform speaker, Transform listener) {
			if ((barkUI != null) && (cachedState != null) && cachedState.HasAnyResponses) {
				Response[] responses = cachedState.HasNPCResponse ? cachedState.npcResponses : cachedState.pcResponses;
				int index = (barkHistory ?? new BarkHistory(BarkOrder.Random)).GetNextIndex(responses.Length);
				DialogueEntry barkEntry = responses[index].destinationEntry;
				if ((barkEntry == null) && DialogueDebug.LogWarnings) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}' bark entry is null", new System.Object[] { DialogueDebug.Prefix, speaker, listener, conversation }), speaker);
				if (barkEntry != null) {
					Subtitle subtitle = new Subtitle(cachedState.subtitle.listenerInfo, cachedState.subtitle.speakerInfo, new FormattedText(barkEntry.DialogueText), string.Empty, string.Empty, barkEntry);
					if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Bark (speaker={1}, listener={2}): '{3}'", new System.Object[] { DialogueDebug.Prefix, speaker, listener, subtitle.formattedText.text }), speaker);
					StartCoroutine(BarkController.Bark(subtitle, speaker, listener, barkUI));
				}
			}
		}

		/// <summary>
		/// Listens for the OnRecordPersistentData message and records the current bark index.
		/// </summary>
		public void OnRecordPersistentData() {
			if (enabled) {
				DialogueLua.SetActorField(GetBarkerName(), "Bark_Index", barkHistory.index);
			}
		}
		
		/// <summary>
		/// Listens for the OnApplyPersistentData message and retrieves the current bark index.
		/// </summary>
		public void OnApplyPersistentData() {
			if (enabled) {
				barkHistory.index = DialogueLua.GetActorField(GetBarkerName(), "Bark_Index").AsInt;
			}
		}

	}

}
