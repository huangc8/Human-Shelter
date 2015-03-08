using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands {
	
	/// <summary>
	/// Implements sequencer command: AudioWait(audioClip[, subject[, audioClips...]])
	/// </summary>
	public class SequencerCommandAudioWait : SequencerCommand {
		
		private float stopTime = 0;
		private AudioSource audioSource = null;
		private int nextClipIndex = 2;
		private AudioClip lastClip = null;
		
		public void Start() {
			string audioClipName = GetParameter(0);
			Transform subject = GetSubject(1);
			nextClipIndex = 2;
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AudioWait({1})", new System.Object[] { DialogueDebug.Prefix, GetParameters() }));
			audioSource = SequencerTools.GetAudioSource(subject);
			if (audioSource == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait() command: can't find or add AudioSource to {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
			} else {
				if (Tools.ApproximatelyZero(audioSource.volume)) audioSource.volume = 1f;
				TryAudioClip(audioClipName);
			}
		}

		private void TryAudioClip(string audioClipName) {
			try {
				AudioClip audioClip = (!string.IsNullOrEmpty(audioClipName)) ? (DialogueManager.LoadAsset(audioClipName) as AudioClip) : null;
				if (audioClip == null) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait() command: Clip '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, audioClipName }));
				} else {
					if (IsAudioMuted()) {
						if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AudioWait(): waiting but not playing '{1}'; audio is muted.", new System.Object[] { DialogueDebug.Prefix, audioClipName }));
					} else {
						if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AudioWait(): playing '{1}'.", new System.Object[] { DialogueDebug.Prefix, audioClipName }));
						//--- Was, prior to implementing pause: audioSource.PlayOneShot(audioClip);
						lastClip = audioClip;
						audioSource.clip = audioClip;
						audioSource.Play();
					}
				}
				stopTime = DialogueTime.time + audioClip.length;
			} catch (System.Exception) {
				stopTime = 0;
			}
		}

		public void Update() {
			if (DialogueTime.time >= stopTime) {
				if (nextClipIndex < Parameters.Length) {
					TryAudioClip(GetParameter(nextClipIndex));
					nextClipIndex++;
				} else {
					Stop();
				}
			}
		}

		public void OnDialogueSystemPause() {
			if (audioSource == null) return;
			audioSource.Pause();
		}

		public void OnDialogueSystemUnpause() {
			if (audioSource == null) return;
			audioSource.Play();
		}

		public void OnDestroy() {
			if ((audioSource != null) && (DialogueTime.time < stopTime)) {
				if (audioSource.isPlaying && (audioSource.clip == lastClip)) {
					audioSource.Stop();
				}
			}
		}
		
	}

}
