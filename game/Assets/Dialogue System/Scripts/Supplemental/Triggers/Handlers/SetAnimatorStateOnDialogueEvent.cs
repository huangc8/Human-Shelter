using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Sets an animator state on dialogue events. You can use this to set a character
	/// to idle when a conversation starts, and back to gameplay state when it ends.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/On Dialogue Event/Set Animator State")]
	public class SetAnimatorStateOnDialogueEvent : ActOnDialogueEvent {
		
		[System.Serializable]
		public class SetAnimatorStateAction : Action {
			public Transform target;
			public string stateName;
			public float crossFadeDuration = 0.3f;
		}
		
		/// <summary>
		/// Actions to take on the "start" event (e.g., OnConversationStart).
		/// </summary>
		public SetAnimatorStateAction[] onStart;
		
		/// <summary>
		/// Actions to take on the "end" event (e.g., OnConversationEnd).
		/// </summary>
		public SetAnimatorStateAction[] onEnd;
		
		public override void TryStartActions(Transform actor) {
			TryActions(onStart, actor);
		}
		
		public override void TryEndActions(Transform actor) {
			TryActions(onEnd, actor);
		}
		
		private void TryActions(SetAnimatorStateAction[] actions, Transform actor) {
			foreach (SetAnimatorStateAction action in actions) {
				if (action.condition.IsTrue(actor)) DoAction(action, actor);
			}
		}
		
		public void DoAction(SetAnimatorStateAction action, Transform actor) {
			if (action != null) {
				Transform target = Tools.Select(action.target, this.transform);
				Animator animator = target.GetComponentInChildren<Animator>();
				if (animator == null) {
					if (DialogueDebug.LogWarnings) Debug.Log(string.Format("{0}: Trigger: {1}.SetAnimatorState() can't find Animator", new System.Object[] { DialogueDebug.Prefix, target.name }));
				} else {
					if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Trigger: {1}.SetAnimatorState({2})", new System.Object[] { DialogueDebug.Prefix, target.name, action.stateName }));
					animator.CrossFade(action.stateName, action.crossFadeDuration);
				}
			}
		}
		
	}

}
