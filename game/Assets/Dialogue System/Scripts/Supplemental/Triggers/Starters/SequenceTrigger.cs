using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
		
	/// <summary>
	/// The sequence trigger component starts a sequence when the game object receives a specified 
	/// dialogue event. For example, you can add a sequence trigger and a static trigger collider 
	/// to a cutscene area. When the player enters the trigger area, this component will start the
	/// sequence.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/Sequence")]
	public class SequenceTrigger : SequenceStarter {
	
		/// <summary>
		/// The trigger that starts the conversation.
		/// </summary>
		public DialogueTriggerEvent trigger = DialogueTriggerEvent.OnUse;

		public bool waitOneFrameOnStartOrEnable = true;
		
		public void OnBarkEnd(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnBarkEnd)) TryStartSequence(actor);
		}
		
		public void OnConversationEnd(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnConversationEnd)) TryStartSequence(actor);
		}
		
		public void OnSequenceEnd(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnSequenceEnd)) TryStartSequence(actor);
		}
		
		public void OnUse(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStartSequence(actor);
		}
		
		public void OnUse(string message) {
			if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStartSequence(null);
		}
		
		public void OnUse() {
			if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStartSequence(null);
		}
		
		public void OnTriggerEnter(Collider other) {
			if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStartSequence(other.transform);
		}
		
		public void OnTriggerEnter2D(Collider2D other) {
			if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStartSequence(other.transform);
		}
		
		void Start() {
			// Waits one frame to allow all other components to finish their Start() methods.
			if (trigger == DialogueTriggerEvent.OnStart) StartCoroutine(StartSequenceAfterOneFrame());
		}
		
		void OnEnable() {
			// Waits one frame to allow all other components to finish their OnEnable() methods.
			if (trigger == DialogueTriggerEvent.OnEnable) StartCoroutine(StartSequenceAfterOneFrame());
		}
		
		private IEnumerator StartSequenceAfterOneFrame() {
			if (waitOneFrameOnStartOrEnable) yield return null;
			TryStartSequence(null);
		}
		
	}

}
