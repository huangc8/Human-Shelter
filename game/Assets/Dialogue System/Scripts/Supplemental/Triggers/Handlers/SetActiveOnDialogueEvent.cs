using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Sets game objects active or inactive at the start and/or end of a dialogue event. Note that
	/// components on inactive game objects (including SetActiveOnDialogueEvent) do not receive 
	/// messages. If you want to activate an inactive game object, you should put this component
	/// on a different, active game object and set the action's target to the inactive object.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/On Dialogue Event/Set Active")]
	public class SetActiveOnDialogueEvent : ActOnDialogueEvent {
		
		[System.Serializable]
		public class SetActiveAction : Action {
			public Transform target;
			public Toggle state;
		}
		
		/// <summary>
		/// Actions to take on the "start" event (e.g., OnConversationStart).
		/// </summary>
		public SetActiveAction[] onStart;
		
		/// <summary>
		/// Actions to take on the "end" event (e.g., OnConversationEnd).
		/// </summary>
		public SetActiveAction[] onEnd;
		
		public override void TryStartActions(Transform actor) {
			TryActions(onStart, actor);
		}
		
		public override void TryEndActions(Transform actor) {
			TryActions(onEnd, actor);
		}
		
		private void TryActions(SetActiveAction[] actions, Transform actor) {
			foreach (SetActiveAction action in actions) {
				if (action.condition.IsTrue(actor)) DoAction(action, actor);
			}
		}
		
		public void DoAction(SetActiveAction action, Transform actor) {
			if (action != null) {
				Transform target = Tools.Select(action.target, this.transform);
				bool newValue = ToggleTools.GetNewValue(target.gameObject.activeSelf, action.state);
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Trigger: {1}.SetActive({2})", new System.Object[] { DialogueDebug.Prefix, target.name, newValue }));
				target.gameObject.SetActive(newValue);
			}
		}
		
	}

}
