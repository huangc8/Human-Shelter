using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands {
	
	/// <summary>
	/// Implements sequencer command: Voice(audioClip, animation[, finalAnimation[, gameobject|speaker|listener]])
	/// </summary>
	public class SequencerCommandVoice : SequencerCommand {
		
		private float stopTime = 0;
		Transform subject = null;
		private string finalClipName = string.Empty;
		private Animation anim = null;
		private AudioSource audioSource = null;

		public void Start() {
			string audioClipName = GetParameter(0);
			string animationClipName = GetParameter(1);
			finalClipName = GetParameter(2);
			subject = GetSubject(3);
			anim = (subject == null) ? null : subject.GetComponent<Animation>();
			AudioClip audioClip = (!string.IsNullOrEmpty(audioClipName)) ? (DialogueManager.LoadAsset(audioClipName) as AudioClip) : null;
			if ((subject == null) || (anim == null)) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Voice({1}, {2}, {3}, {4}) command: No Animation component found on {3}.", new System.Object[] { DialogueDebug.Prefix, audioClipName, animationClipName, finalClipName, (subject != null) ? subject.name : GetParameter(2) }));
			} else if (audioClip == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Voice({1}, {2}, {3}, {4}) command: Clip is null.", new System.Object[] { DialogueDebug.Prefix, audioClipName, animationClipName, finalClipName, subject.name }));
			} else if (string.IsNullOrEmpty(animationClipName)) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Voice({1}, {2}, {3}, {4}) command: Animation name is blank.", new System.Object[] { DialogueDebug.Prefix, audioClipName, animationClipName, finalClipName, subject.name }));
			} else {
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: Voice({1}, {2}, {3}, {4})", new System.Object[] { DialogueDebug.Prefix, audioClipName, animationClipName, finalClipName, Tools.GetObjectName(subject) }));
				audioSource = SequencerTools.GetAudioSource(subject);
				if (audioSource == null) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Voice() command: can't find or add AudioSource to {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
				} else {
					if (IsAudioMuted()) {
						if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: Voice({1}, {2}, {3}, {4}): Audio is muted; not playing it.", new System.Object[] { DialogueDebug.Prefix, audioClipName, animationClipName, finalClipName, Tools.GetObjectName(subject) }));
					} else {
						//--- Was, prior to implementing pause: audioSource.PlayOneShot(audioClip);
						audioSource.clip = audioClip;
						audioSource.Play();
					}
					anim.CrossFade(animationClipName);
					try {
						stopTime = DialogueTime.time + Mathf.Max(0.1f, anim[animationClipName].length - 0.3f);
						if (audioClip.length > anim[animationClipName].length) stopTime = DialogueTime.time + audioClip.length;
					} catch (System.Exception) {
						stopTime = 0;
					}
				}
			}
		}
		
		public void Update() {
			if (DialogueTime.time >= stopTime) Stop();
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
			if ((subject != null) && (anim != null)) {
				if (!string.IsNullOrEmpty(finalClipName)) {
					anim.CrossFade(finalClipName);
				} else if (anim.clip != null) {
					anim.CrossFade(anim.clip.name);
				}
			}
			if ((audioSource != null) && (DialogueTime.time < stopTime)) {
				audioSource.Stop();
			}
		}
		
	}

}
