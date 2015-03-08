using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// This is the base class for the ConversationOnXXX (e.g., ConversationOnUse) and BarkOnXXX
	/// (e.g., BarkOnUse) components.
	/// </summary>
	public class ConversationStarter : DialogueEventStarter {
	
		/// <summary>
		/// The title of the conversation to start.
		/// </summary>
		public string conversation;
		
		/// <summary>
		/// The condition required for the conversation to start.
		/// </summary>
		public Condition condition;

		/// <summary>
		/// If this is <c>true<c/c> and no valid entries currently link from the start entry,
		/// don't start the conversation.
		/// </summary>
		public bool skipIfNoValidEntries;
		
		/// <summary>
		/// The conversant of the conversation. If not set, this game object. The actor is usually
		/// the entity that caused the trigger (for example, the player that hits the "Use" button
		/// on the conversant, thereby triggering OnUse).
		/// </summary>
		public Transform conversant;
		
		private bool tryingToStart = false;

		[HideInInspector]
		public bool useConversationTitlePicker = true;

		[HideInInspector]
		public DialogueDatabase selectedDatabase = null;

		/// <summary>
		/// If the condition is true, starts the conversation between the specified actor and the
		/// character that this component is attached to.
		/// </summary>
		/// <param name='actor'>
		/// The actor to converse with.
		/// </param>
		public void TryStartConversation(Transform actor) {
			if (!tryingToStart) {
				tryingToStart = true;
				try {
					if ((condition == null) || condition.IsTrue(actor)) {
						if (string.IsNullOrEmpty(conversation)) {
							if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: Conversation triggered on {1}, but conversation name is blank.", new System.Object[] { DialogueDebug.Prefix, name }));
						} else if (DialogueManager.IsConversationActive) {
							if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Conversation triggered on {1}, but another conversation is already active.", new System.Object[] { DialogueDebug.Prefix, name }));
						} else {
							StartConversation(actor);
						}
						DestroyIfOnce();
					}
				} finally {
					tryingToStart = false;
				}
			}
		}
		
		private void StartConversation(Transform actor) {
			Transform actualConversant = Tools.Select(conversant, this.transform);
			bool skip = skipIfNoValidEntries && !DialogueManager.ConversationHasValidEntry(conversation, actor, actualConversant);
			if (skip) {
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Conversation triggered on {1}, but skipping because no entries are currently valid.",new System.Object[] { DialogueDebug.Prefix, name }));
			} else {
				DialogueManager.StartConversation(conversation, actor, actualConversant);
			}
		}
		
	}

}
