using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Sends messages to game objects at the start and/or end of a dialogue event.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/On Dialogue Event/Send Message")]
	public class SendMessageOnDialogueEvent : ActOnDialogueEvent {
		
		[System.Serializable]
		public class SendMessageAction : Action {
			public Transform target;
			public string methodName;
			public string parameter;
		}
		
		/// <summary>
		/// Actions to take on the "start" event (e.g., OnConversationStart).
		/// </summary>
		public SendMessageAction[] onStart;
		
		/// <summary>
		/// Actions to take on the "end" event (e.g., OnConversationEnd).
		/// </summary>
		public SendMessageAction[] onEnd;
		
		public override void TryStartActions(Transform actor) {
			TryActions(onStart, actor);
		}
		
		public override void TryEndActions(Transform actor) {
			TryActions(onEnd, actor);
		}
		
		private void TryActions(SendMessageAction[] actions, Transform actor) {
			foreach (SendMessageAction action in actions) {
				if (action.condition.IsTrue(actor)) DoAction(action, actor);
			}
		}
		
		/// <summary>
		/// Executes an action.
		/// </summary>
		/// <param name='action'>
		/// The details of the action to perform. If the action.parameter is empty, this method sends
		/// a reference to the sender's game object as the parameter.
		/// </param>
		/// <param name='actor'>
		/// The actor performing the action (versus the target, which is specified in the action details.
		/// </param>
		private void DoAction(SendMessageAction action, Transform actor) {
			if (action != null) {
				Transform target = Tools.Select(action.target, this.transform);
				string parameter = string.IsNullOrEmpty(action.parameter) ? null : action.parameter;
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sending message '{1}' to {2} (parameter={3}).", new System.Object[] { DialogueDebug.Prefix, action.methodName, target, parameter }), this);
				target.BroadcastMessage(action.methodName, parameter, SendMessageOptions.DontRequireReceiver);
			}
		}
		
	}

}
