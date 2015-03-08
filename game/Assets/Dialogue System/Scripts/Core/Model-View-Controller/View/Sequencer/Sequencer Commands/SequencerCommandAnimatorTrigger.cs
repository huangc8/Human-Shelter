/* 
 * This was moved into Sequencer.cs for efficiency.
 * 
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands {
	
	/// <summary>
	/// Implements sequencer command: "AnimatorTrigger(animatorParameter[, gameobject|speaker|listener])",
	/// which sets a trigger parameter on a subject's Animator.
	/// 
	/// Arguments:
	/// -# Name of a Mecanim animator parameter.
	/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
	/// </summary>
	public class SequencerCommandAnimatorTrigger : SequencerCommand {
		
		public void Start() {
			// Get the values of the parameters:
			string animatorParameter = GetParameter(0);
			Transform subject = GetSubject(1);
			Animator animator = (subject != null) ? subject.GetComponentInChildren<Animator>() : null;
			if (animator == null) {
				if (DialogueDebug.LogWarnings) Debug.Log(string.Format("{0}: Sequencer: AnimatorTrigger({1}, {2}): No Animator found on {2}", DialogueDebug.Prefix, animatorParameter, (subject != null) ? subject.name : GetParameter(1)));
			} else {
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorTrigger({1}, {2})", DialogueDebug.Prefix, animatorParameter, subject));
			}
			if (animator != null) animator.SetTrigger(animatorParameter);
			Stop();
		}
			
	}

}
*/