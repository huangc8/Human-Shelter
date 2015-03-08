using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Starts a conversation at the start and/or end of a dialogue event.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/On Dialogue Event/Start Conversation")]
	public class StartConversationOnDialogueEvent : ActOnDialogueEvent {
		
		[System.Serializable]
		public class ConversationAction : Action {

			public Transform speaker;

			public Transform listener;

			[ConversationPopup]
			public string conversation;

			public bool skipIfNoValidEntries;
		}
		
		/// <summary>
		/// Actions to take on the "start" event (e.g., OnConversationStart).
		/// </summary>
		public ConversationAction[] onStart;
		
		/// <summary>
		/// Actions to take on the "end" event (e.g., OnConversationEnd).
		/// </summary>
		public ConversationAction[] onEnd;
		
		public override void TryStartActions(Transform actor) {
			TryActions(onStart, actor);
		}
		
		public override void TryEndActions(Transform actor) {
			TryActions(onEnd, actor);
		}
		
		private void TryActions(ConversationAction[] actions, Transform actor) {
			if (actions != null) {
				foreach (ConversationAction action in actions) {
					if (action.condition.IsTrue(actor)) DoAction(action, actor);
				}
			}
		}
		
		/// <summary>
		/// Executes a conversation action.
		/// </summary>
		public void DoAction(ConversationAction action, Transform actor) {
			if (action != null) {
				Transform speaker = Tools.Select(action.speaker, this.transform);
				Transform listener = Tools.Select(action.listener, actor);
				bool skip = action.skipIfNoValidEntries && !DialogueManager.ConversationHasValidEntry(action.conversation, speaker, listener);
				if (!skip) DialogueManager.StartConversation(action.conversation, speaker, listener);
			}
		}
		
	}

}
