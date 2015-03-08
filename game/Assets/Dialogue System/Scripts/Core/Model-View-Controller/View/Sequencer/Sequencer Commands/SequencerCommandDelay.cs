using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands {
	
	/// <summary>
	/// Implements sequencer command: Delay(seconds)
	/// </summary>
	public class SequencerCommandDelay : SequencerCommand {
		
		private float stopTime;
		
		public void Start() {
			float seconds = GetParameterAsFloat(0);
			stopTime = DialogueTime.time + seconds;
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: Delay({1})", new System.Object[] { DialogueDebug.Prefix, seconds }));
		}
		
		public void Update() {
			if (DialogueTime.time >= stopTime) Stop();
		}
		
	}

}
