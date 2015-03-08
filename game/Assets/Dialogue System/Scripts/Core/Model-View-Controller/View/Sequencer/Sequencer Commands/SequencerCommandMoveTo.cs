using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands {
	
	/// <summary>
	/// Implements sequencer command: "MoveTo(target, [, subject[, duration]])", which matches the 
	/// subject to the target's position and rotation.
	/// 
	/// Arguments:
	/// -# The target. 
	/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. 
	/// Default: speaker.
	/// -# (Optional) Duration in seconds.
	/// </summary>
	public class SequencerCommandMoveTo : SequencerCommand {
		
		private const float SmoothMoveCutoff = 0.05f;
		
		private Transform target;
		private Transform subject;
		private float duration;
		float startTime;
		float endTime;
		Vector3 originalPosition;
		Quaternion originalRotation;
		
		public void Start() {
			// Get the values of the parameters:
			target = GetSubject(0);
			subject = GetSubject(1);
			duration = GetParameterAsFloat(2, 0);
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: MoveTo({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, target, subject, duration }));
			if ((target == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: MoveTo() target '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, GetParameter(0) }));
			if ((subject == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: MoveTo() subject '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, GetParameter(1) }));
			
			// Set up the move:
			if ((subject != null) && (target != null) && (subject != target)) {
				
				// If duration is above the cutoff, smoothly move toward target:
				if (duration > SmoothMoveCutoff) {
					startTime = DialogueTime.time;
					endTime = startTime + duration;
					originalPosition = subject.position;
					originalRotation = subject.rotation;
				} else {
					Stop();
				}
			} else {
				Stop();
			}
		}
			
		
		public void Update() {
			// Keep smoothing for the specified duration:
			if (DialogueTime.time < endTime) {
				float elapsed = (DialogueTime.time - startTime) / duration;
				subject.rotation = Quaternion.Slerp(originalRotation, target.rotation, elapsed);
				subject.position = Vector3.Slerp(originalPosition, target.position, elapsed);
			} else {
				Stop();
			}
		}
					
		public void OnDestroy() {
			// Final position:
			if ((subject != null) && (target != null) && (subject != target)) {
				subject.rotation = target.rotation;
				subject.position = target.position;
			}
			
		}
		
	}

}
