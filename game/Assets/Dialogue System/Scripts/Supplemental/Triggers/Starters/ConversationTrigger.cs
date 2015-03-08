using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// The conversation trigger component starts a conversation between an actor and this game 
	/// object when the game object receives a specified dialogue trigger. For example, you can use
	/// this component to start a conversation as soon as a game object or level is loaded by 
	/// choosing the OnStart event.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/Conversation")]
	public class ConversationTrigger : ConversationStarter {
		
		/// <summary>
		/// The actor to converse with. If not set, the game object that triggered the event.
		/// </summary>
		public Transform actor;
		
		/// <summary>
		/// The trigger that starts the conversation.
		/// </summary>
		public DialogueTriggerEvent trigger = DialogueTriggerEvent.OnUse;
		
		/// <summary>
		/// Set <c>true</c> to stop the conversation if the actor leaves the trigger area.
		/// </summary>
		public bool stopConversationOnTriggerExit = false;
		
		private float earliestTimeToAllowTriggerExit = 0;
			
		private const float MarginToAllowTriggerExit = 0.2f;
		
		public void OnBarkEnd(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnBarkEnd)) TryStartConversation(Tools.Select(this.actor, actor));
		}
		
		public void OnConversationEnd(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnConversationEnd)) TryStartConversation(Tools.Select(this.actor, actor));
		}
		
		public void OnSequenceEnd(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnSequenceEnd)) TryStartConversation(Tools.Select(this.actor, actor));
		}
		
		public void OnUse(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStartConversation(Tools.Select(this.actor, actor));
		}
		
		public void OnUse(string message) {
			if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStartConversation(this.actor);
		}
		
		public void OnUse() {
			if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStartConversation(this.actor);
		}
		
		public void OnTriggerEnter(Collider other) {
			if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStartConversationOnTriggerEnter(other.transform);
		}
		
		public void OnTriggerExit(Collider other) {
			CheckOnTriggerExit(other.transform);
		}

		public void OnTriggerEnter2D(Collider2D other) {
			if (!DialogueManager.IsConversationActive) { // Workaround for Unity bug 579005: OnTriggerEnter2D called repeatedly.
				if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStartConversationOnTriggerEnter(other.transform);
			}
		}
		
		public void OnTriggerExit2D(Collider2D other) {
			CheckOnTriggerExit(other.transform);
		}
		
		private void TryStartConversationOnTriggerEnter(Transform otherTransform) {
			if ((otherTransform != this.actor) && !condition.IsTrue(otherTransform)) return;
			TryStartConversation(Tools.Select(this.actor, otherTransform));
			earliestTimeToAllowTriggerExit = Time.time + MarginToAllowTriggerExit;
		}

		private void CheckOnTriggerExit(Transform otherTransform) {
			if (enabled &&
			    stopConversationOnTriggerExit &&
			    DialogueManager.IsConversationActive &&
			    (Time.time > earliestTimeToAllowTriggerExit) &&
			    ((DialogueManager.CurrentActor == otherTransform) || (DialogueManager.CurrentConversant == otherTransform))) {
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Stopping conversation because {1} exited trigger area.", new System.Object[] { DialogueDebug.Prefix, otherTransform.name }));
				DialogueManager.StopConversation();
			}
		}
		
		void Start() {
			// Waits one frame to allow all other components to finish their Start() methods.
			if (trigger == DialogueTriggerEvent.OnStart) StartCoroutine(StartConversationAfterOneFrame());
		}
		
		void OnEnable() {
			// Waits one frame to allow all other components to finish their OnEnable() methods.
			if (trigger == DialogueTriggerEvent.OnEnable) StartCoroutine(StartConversationAfterOneFrame());
		}
		
		private IEnumerator StartConversationAfterOneFrame() {
			yield return null;
			TryStartConversation(actor);
		}
		
	}

}
