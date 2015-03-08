using UnityEngine;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
		
	/// <summary>
	/// The quest trigger component sets a quest status when the game object receives a specified 
	/// trigger event. For example, you can add a quest trigger and a static trigger collider 
	/// to an area. When the player enters the trigger area, this component could set a quest status
	/// to success.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/Quest")]
	public class QuestTrigger : DialogueEventStarter {
	
		/// <summary>
		/// The trigger that starts the conversation.
		/// </summary>
		public DialogueTriggerEvent trigger = DialogueTriggerEvent.OnUse;
		
		/// <summary>
		/// The conditions under which the trigger will fire.
		/// </summary>
		public Condition condition;
		
		/// <summary>
		/// The name of the quest.
		/// </summary>
		public string questName;
		
		/// <summary>
		/// The new state of the quest when triggered.
		/// </summary>
		public QuestState questState;
		
		/// <summary>
		/// The lua code to run.
		/// </summary>
		public string luaCode = string.Empty;
		
		/// <summary>
		/// An optional gameplay alert message. Leave blank for no message.
		/// </summary>
		public string alertMessage;

		/// <summary>
		/// An optional localized text table to use for the alert message.
		/// </summary>
		public LocalizedTextTable localizedTextTable;
		
		[Serializable]
		public class SendMessageAction {
			public GameObject gameObject = null;
			public string message = "OnUse";
			public string parameter = string.Empty;
		}

		/// <summary>
		/// Targets and messages to send when the trigger fires.
		/// </summary>
		public SendMessageAction[] sendMessages = new SendMessageAction[0];
		
		[HideInInspector]
		public bool useQuestNamePicker = true;
		
		[HideInInspector]
		public DialogueDatabase selectedDatabase = null;
		
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
		/// Sets the quest status if the condition is true.
		/// </summary>
		public void TryStart(Transform actor) {
			if (tryingToStart) return;
			tryingToStart = true;
			try {
				if (((condition == null) || condition.IsTrue(actor)) && !string.IsNullOrEmpty(questName)) {
					Fire();
				}
			} finally {
				tryingToStart = false;
			}
		}	

		public void Fire() {
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Setting quest '{1}' state to '{2}'", new System.Object[] { DialogueDebug.Prefix, questName, QuestLog.StateToString(questState) }));

			// Quest:
			if (!string.IsNullOrEmpty(questName)) {
				QuestLog.SetQuestState(questName, questState);
			}
			
			// Lua:
			if (!string.IsNullOrEmpty(luaCode)) {
				Lua.Run(luaCode, DialogueDebug.LogInfo);
			}
			
			// Alert:
			if (!string.IsNullOrEmpty(alertMessage)) {
				string localizedAlertMessage = alertMessage;
				if ((localizedTextTable != null) && localizedTextTable.ContainsField(alertMessage)) {
					localizedAlertMessage = localizedTextTable[alertMessage];
				}
				DialogueManager.ShowAlert(localizedAlertMessage);
			}
			
			// Send Messages:
			foreach (var sma in sendMessages) {
				if (sma.gameObject != null && !string.IsNullOrEmpty(sma.message)) {
					sma.gameObject.SendMessage(sma.message, sma.parameter, SendMessageOptions.DontRequireReceiver);
				}
			}
			
			// Once?
			DestroyIfOnce();
		}
		
	}

}
