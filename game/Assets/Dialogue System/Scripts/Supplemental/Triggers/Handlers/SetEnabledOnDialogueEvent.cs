using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Sets MonoBehaviours enabled or disabled at the start and/or end of a dialogue event.
	/// </summary>
	public class SetEnabledOnDialogueEvent : ActOnDialogueEvent {
		
		[System.Serializable]
		public class SetEnabledAction : Action {
			public MonoBehaviour target;
			public Toggle state;
		}
		
		/// <summary>
		/// Actions to take on the "start" event (e.g., OnConversationStart).
		/// </summary>
		public SetEnabledAction[] onStart;
		
		/// <summary>
		/// Actions to take on the "end" event (e.g., OnConversationEnd).
		/// </summary>
		public SetEnabledAction[] onEnd;
		
		public override void TryStartActions(Transform actor) {
			TryActions(onStart, actor);
		}
		
		public override void TryEndActions(Transform actor) {
			TryActions(onEnd, actor);
		}
		
		private void TryActions(SetEnabledAction[] actions, Transform actor) {
			foreach (SetEnabledAction action in actions) {
				if (action.condition.IsTrue(actor)) DoAction(action, actor);
			}
		}
		
		public void DoAction(SetEnabledAction action, Transform actor) {
			if (action != null) {
				MonoBehaviour target = Tools.Select(action.target, this);
				bool newValue = ToggleTools.GetNewValue(target.enabled, action.state);
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Trigger: {1}.{2}.enabled = {3}", new System.Object[] { DialogueDebug.Prefix, target.name, target.GetType().Name, newValue }));
				target.enabled = newValue;
			}
		}
		
	}

}
