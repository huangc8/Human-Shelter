using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Sets a quest state at the start and/or end of a dialogue event.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/On Dialogue Event/Set Quest State")]
	public class SetQuestStateOnDialogueEvent : ActOnDialogueEvent {
		
		[System.Serializable]
		public class SetQuestStateAction : Action {

			[QuestPopup]
			public string questName;

			public QuestState questState;

			public string alertMessage;
		}
		
		/// <summary>
		/// Actions to take on the "start" event (e.g., OnConversationStart).
		/// </summary>
		public SetQuestStateAction[] onStart;
		
		/// <summary>
		/// Actions to take on the "end" event (e.g., OnConversationEnd).
		/// </summary>
		public SetQuestStateAction[] onEnd;
		
		public override void TryStartActions(Transform actor) {
			TryActions(onStart, actor);
		}
		
		public override void TryEndActions(Transform actor) {
			TryActions(onEnd, actor);
		}
		
		private void TryActions(SetQuestStateAction[] actions, Transform actor) {
			foreach (SetQuestStateAction action in actions) {
				if (action.condition.IsTrue(actor)) DoAction(action, actor);
			}
		}
		
		public void DoAction(SetQuestStateAction action, Transform actor) {
			if ((action != null) && !string.IsNullOrEmpty(action.questName)) {
				QuestLog.SetQuestState(action.questName, action.questState);
				if (!string.IsNullOrEmpty(action.alertMessage)) DialogueManager.ShowAlert(action.alertMessage);
			}
		}
		
	}

}
