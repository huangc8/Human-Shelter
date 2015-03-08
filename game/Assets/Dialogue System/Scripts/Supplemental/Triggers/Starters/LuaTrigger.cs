using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
		
	/// <summary>
	/// The Lua trigger component runs Lua code when the game object receives a specified 
	/// trigger event. For example, you can add a Lua trigger and a static trigger collider 
	/// to an area. When the player enters the trigger area, this component could set a Lua variable.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/Lua")]
	public class LuaTrigger : DialogueEventStarter {
	
		/// <summary>
		/// The trigger that runs the Lua code..
		/// </summary>
		public DialogueTriggerEvent trigger = DialogueTriggerEvent.OnUse;
		
		/// <summary>
		/// The conditions under which the trigger will fire.
		/// </summary>
		public Condition condition;
		
		/// <summary>
		/// The Lua code to run.
		/// </summary>
		public string luaCode;
		
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
		/// Runs the Lua code if the condition is true.
		/// </summary>
		public void TryStart(Transform actor) {
			if (tryingToStart) return;
			tryingToStart = true;
			try {
				if (((condition == null) || condition.IsTrue(actor)) && !string.IsNullOrEmpty(luaCode)) {
					Lua.Run(luaCode, DialogueDebug.LogInfo);
					DialogueManager.CheckAlerts();
					DestroyIfOnce();
				}
			} finally {
				tryingToStart = false;
			}
		}	
		
	}

}
