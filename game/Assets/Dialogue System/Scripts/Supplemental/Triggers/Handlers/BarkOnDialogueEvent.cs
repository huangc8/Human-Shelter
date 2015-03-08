using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Barks at the start and/or end of a dialogue event. You can use this to chain barks, 
	/// essentially creating an interactive, gameplay conversation of barks.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/On Dialogue Event/Bark")]
	public class BarkOnDialogueEvent : ActOnDialogueEvent {
		
		/// <summary>
		/// The parameters for a bark action.
		/// </summary>
		[System.Serializable]
		public class BarkAction : Action {

			public Transform speaker;

			public Transform listener;

			[ConversationPopup]
			public string conversation;
		}

		/// <summary>
		/// Actions to take on the "start" event (e.g., OnConversationStart).
		/// </summary>
		public BarkAction[] onStart;
		
		/// <summary>
		/// Actions to take on the "end" event (e.g., OnConversationEnd).
		/// </summary>
		public BarkAction[] onEnd;
		
		/// <summary>
		/// The bark order.
		/// </summary>
		public BarkOrder barkOrder = BarkOrder.Random;
		
		/// <summary>
		/// Gets the sequencer used by the current bark, if a bark is playing.
		/// If a bark is not playing, this is undefined. To check if a bark is
		/// playing, check the bark UI's IsPlaying property.
		/// </summary>
		/// <value>The sequencer.</value>
		public Sequencer sequencer { get; private set; }
		
		private BarkHistory barkHistory;
		
		void Awake() {
			barkHistory = new BarkHistory(barkOrder);
			sequencer = null;
		}
		
		public override void TryStartActions(Transform actor) {
			TryActions(onStart, actor);
		}
		
		public override void TryEndActions(Transform actor) {
			TryActions(onEnd, actor);
		}
		
		private void TryActions(BarkAction[] actions, Transform actor) {
			foreach (BarkAction action in actions) {
				if (action.condition.IsTrue(actor)) DoAction(action, actor);
			}
		}
		
		public void DoAction(BarkAction action, Transform actor) {
			if (action != null) {
				Transform speaker = Tools.Select(action.speaker, this.transform);
				Transform listener = Tools.Select(action.listener, actor);
				DialogueManager.Bark(action.conversation, speaker, listener, barkHistory);
				sequencer = BarkController.LastSequencer;
			}
		}
		
	}

}
