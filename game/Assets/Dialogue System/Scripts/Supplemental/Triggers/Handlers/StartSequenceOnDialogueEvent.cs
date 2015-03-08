using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Starts a sequence at the start and/or end of a dialogue event.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/On Dialogue Event/Start Sequence")]
	public class StartSequenceOnDialogueEvent : ActOnDialogueEvent {
		
		/// <summary>
		/// The parameters for a sequence action.
		/// </summary>
		[System.Serializable]
		public class SequenceAction : Action {
		
			/// <summary>
			/// The actor performing the sequence.
			/// </summary>
			public Transform actor;
			
			/// <summary>
			/// The other participant (e.g., in a conversation, this is the "listener").
			/// </summary>
			public Transform otherParticipant;
			
			/// <summary>
			/// The sequence to play.
			/// </summary>
			[Multiline]
			public string sequence;
			
		}
		
		/// <summary>
		/// Actions to take on the "start" event (e.g., OnConversationStart).
		/// </summary>
		public SequenceAction[] onStart;
		
		/// <summary>
		/// Actions to take on the "end" event (e.g., OnConversationEnd).
		/// </summary>
		public SequenceAction[] onEnd;
		
		public override void TryStartActions(Transform actor) {
			TryActions(onStart, actor);
		}
		
		public override void TryEndActions(Transform actor) {
			TryActions(onEnd, actor);
		}
		
		private void TryActions(SequenceAction[] actions, Transform actor) {
			foreach (SequenceAction action in actions) {
				if (action.condition.IsTrue(actor)) DoAction(action, actor);
			}
		}
		
		public void DoAction(SequenceAction action, Transform actor) {
			if (action != null) {
				Transform sequenceActor = Tools.Select(action.actor, this.transform);
				Transform otherParticipant = Tools.Select(action.otherParticipant, actor);
				DialogueManager.PlaySequence(action.sequence, sequenceActor, otherParticipant);
			}
		}
		
	}

}
