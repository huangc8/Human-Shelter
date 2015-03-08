using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands {
	
	/// <summary>
	/// Implements sequencer command: Zoom2D([gameobject|speaker|listener[, size[, duration]]])
	/// 
	/// Arguments:
	/// -# subject:(Optional) The subject; can be speaker, listener, or the name of a game object. Default:
	/// speaker.
	/// -# size: (Optional) The orthographic camera size to zoom to.
	/// -# duration: (Optional) Duration over which to move the camera. Default: immediate.
	/// </summary>
	public class SequencerCommandZoom2D : SequencerCommand {
		
		private const float SmoothMoveCutoff = 0.05f;
		
		private Transform subject;
		private Vector3 targetPosition;
		private Vector3 originalPosition;
		private float targetSize;
		private float originalSize;
		private float duration;
		private float startTime;
		private float endTime;

		public void Start() {
			// Get the values of the parameters:
			subject = GetSubject(0);
			targetSize = GetParameterAsFloat(1, 16);
			duration = GetParameterAsFloat(2, 0);

			// Log to the console:
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: Zoom2D({1}, {2}, {3}s)", new System.Object[] { DialogueDebug.Prefix, Tools.GetGameObjectName(subject), targetSize, duration }));
			if ((subject == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Camera subject '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, GetParameter(1) }));
			
			// Start moving the camera:
			Sequencer.TakeCameraControl();
			if (subject != null) {
				targetPosition = new Vector3(subject.position.x, subject.position.y, Sequencer.SequencerCamera.transform.position.z);
				originalPosition = Sequencer.SequencerCamera.transform.position;
				originalSize = Sequencer.SequencerCamera.orthographicSize;

				// If duration is above the cutoff, smoothly move camera toward camera angle:
				if (duration > SmoothMoveCutoff) {
					startTime = DialogueTime.time;
					endTime = startTime + duration;
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
				Sequencer.SequencerCamera.transform.position = Vector3.Slerp(originalPosition, targetPosition, elapsed);
				Sequencer.SequencerCamera.orthographicSize = Mathf.Lerp(originalSize, targetSize, elapsed);
			} else {
				Stop();
			}
		}
		
		public void OnDestroy() {
			// Final position:
			if (subject != null) {
				Sequencer.SequencerCamera.transform.position = targetPosition;
				Sequencer.SequencerCamera.orthographicSize = targetSize;
			}
		}
		
	}
	
}
