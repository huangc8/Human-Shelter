using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Sets components enabled or disabled at the start and/or end of a dialogue event.
	/// The older SetEnabledOnDialogueEvent trigger was written for MonoBehaviours. On customer
	/// request, this trigger was added to handle renderers and colliders, which aren't
	/// MonoBehaviours.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/On Dialogue Event/Set Enabled")]
	public class SetComponentEnabledOnDialogueEvent : ActOnDialogueEvent {
		
		[System.Serializable]
		public class SetComponentEnabledAction : Action {
			public Component target;
			public Toggle state;
		}
		
		/// <summary>
		/// Actions to take on the "start" event (e.g., OnConversationStart).
		/// </summary>
		public SetComponentEnabledAction[] onStart;
		
		/// <summary>
		/// Actions to take on the "end" event (e.g., OnConversationEnd).
		/// </summary>
		public SetComponentEnabledAction[] onEnd;
		
		public override void TryStartActions(Transform actor) {
			TryActions(onStart, actor);
		}
		
		public override void TryEndActions(Transform actor) {
			TryActions(onEnd, actor);
		}
		
		private void TryActions(SetComponentEnabledAction[] actions, Transform actor) {
			foreach (SetComponentEnabledAction action in actions) {
				if (action.condition.IsTrue(actor)) DoAction(action, actor);
			}
		}
		
		public void DoAction(SetComponentEnabledAction action, Transform actor) {
			if ((action != null) && (action.target != null)) {
				Tools.SetComponentEnabled(action.target, action.state);
			}
		}
		
	}

}
