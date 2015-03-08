using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
		
	/// <summary>
	/// The Alert Trigger component shows a gameplay alert when a trigger event occurs.
	/// If a LocalizedTextTable has been asssigned to the Dialogue Manager, it will use
	/// localized version of the alert message.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/Alert")]
	public class AlertTrigger : DialogueEventStarter {
	
		/// <summary>
		/// The trigger that shows the alert.
		/// </summary>
		public DialogueTriggerEvent trigger = DialogueTriggerEvent.OnUse;

		/// <summary>
		/// If localizedTextTable is assigned, message is the field name of a field
		/// in the localized text table.
		/// </summary>
		public LocalizedTextTable localizedTextTable;
			
		/// <summary>
		/// The message to show.
		/// </summary>
		public string message;

		/// <summary>
		/// The duration to show the message.
		/// </summary>
		public float duration = 5f;
		
		/// <summary>
		/// The condition required for the alert.
		/// </summary>
		public Condition condition;
		
		private bool tryingToStart = false;
		
		public void OnBarkEnd(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnBarkEnd)) TryStart(actor);
		}
		
		public void OnConversationEnd(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnConversationEnd)) TryStart(actor);
		}
		
		public void OnSequenceEnd(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnSequenceEnd)) TryStart(actor);
		}
		
		public void OnUse(Transform actor) {
			if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStart(actor);
		}
		
		public void OnUse(string message) {
			if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStart(null);
		}
		
		public void OnUse() {
			if (enabled && (trigger == DialogueTriggerEvent.OnUse)) TryStart(null);
		}
		
		public void OnTriggerEnter(Collider other) {
			if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStart(other.transform);
		}
		
		public void OnTriggerEnter2D(Collider2D other) {
			if (enabled && (trigger == DialogueTriggerEvent.OnTriggerEnter)) TryStart(other.transform);
		}
		
		void Start() {
			// Waits one frame to allow all other components to finish their Start() methods.
			if (trigger == DialogueTriggerEvent.OnStart) StartCoroutine(StartAfterOneFrame());
		}
		
		void OnEnable() {
			// Waits one frame to allow all other components to finish their OnEnable() methods.
			if (trigger == DialogueTriggerEvent.OnEnable) StartCoroutine(StartAfterOneFrame());
		}
		
		private IEnumerator StartAfterOneFrame() {
			yield return null;
			TryStart(null);
		}
		
		/// <summary>
		/// Show the alert if the condition is true.
		/// </summary>
		public void TryStart(Transform actor) {
			if (tryingToStart) return;
			tryingToStart = true;
			try {
				if (((condition == null) || condition.IsTrue(actor)) && !string.IsNullOrEmpty(message)) {
					string actualMessage = message;
					if ((localizedTextTable != null) && localizedTextTable.ContainsField(message)) {
						actualMessage = localizedTextTable[message];
					}
					string text = FormattedText.Parse(actualMessage, DialogueManager.MasterDatabase.emphasisSettings).text;
					DialogueManager.ShowAlert(text, duration);
					DestroyIfOnce();
				}
			} finally {
				tryingToStart = false;
			}
		}	
		
	}

}
