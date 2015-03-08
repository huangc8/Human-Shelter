using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Contains all dialogue (conversation) controls for a Unity UI Dialogue UI.
	/// </summary>
	[System.Serializable]
	public class UnityUIDialogueControls : AbstractDialogueUIControls {
		
		/// <summary>
		/// The panel containing the dialogue controls. A panel is optional, but you may want one
		/// so you can include a background image, panel-wide effects, etc.
		/// </summary>
		public UnityEngine.UI.Graphic panel;
		
		/// <summary>
		/// The NPC subtitle controls.
		/// </summary>
		public UnityUISubtitleControls npcSubtitle;
		
		/// <summary>
		/// The PC subtitle controls.
		/// </summary>
		public UnityUISubtitleControls pcSubtitle;
		
		/// <summary>
		/// The response menu controls.
		/// </summary>
		public UnityUIResponseMenuControls responseMenu;
		
		[Serializable]
		public class AnimationTransitions {
			public string showTrigger = "Show";
			public string hideTrigger = "Hide";
		}
		
		public AnimationTransitions animationTransitions = new AnimationTransitions();
		
		private bool isVisible = false;
		
		private Animator animator = null;
		
		private bool lookedForAnimator = false;
		
		public override AbstractUISubtitleControls NPCSubtitle { 
			get { return npcSubtitle; }
		}
		
		public override AbstractUISubtitleControls PCSubtitle {
			get { return pcSubtitle; }
		}
		
		public override AbstractUIResponseMenuControls ResponseMenu {
			get { return responseMenu; }
		}
		
		public override void ShowPanel() {
			if (panel != null) Tools.SetGameObjectActive(panel, true);
			if (!isVisible && CanTriggerAnimations() && !string.IsNullOrEmpty(animationTransitions.showTrigger)) {
				animator.SetTrigger(animationTransitions.showTrigger);
			}
			isVisible = true;
		}
		
		public override void SetActive(bool value) {
			if (value == true) {
				base.SetActive(true);
				if (panel != null) Tools.SetGameObjectActive(panel, true);
				if (!isVisible && CanTriggerAnimations() && !string.IsNullOrEmpty(animationTransitions.showTrigger)) {
					animator.SetTrigger(animationTransitions.showTrigger);
				}
			} else {
				if (isVisible && CanTriggerAnimations() && !string.IsNullOrEmpty(animationTransitions.hideTrigger)) {
					animator.SetTrigger(animationTransitions.hideTrigger);
					DialogueManager.Instance.StartCoroutine(DisableAfterAnimation());
				} else {
					base.SetActive(false);
					if (panel != null) Tools.SetGameObjectActive(panel, false);
				}
			}
			isVisible = value;
		}

		private bool CanTriggerAnimations() {
			if ((animator == null) && !lookedForAnimator) {
				lookedForAnimator = true;
				if (panel != null) animator = panel.GetComponent<Animator>();
			}
			return (animator != null) && (animationTransitions != null);
		}

		private IEnumerator DisableAfterAnimation() {
			if (animator != null) {
				const float maxWaitDuration = 10;
				float timeout = Time.realtimeSinceStartup + maxWaitDuration;
				var oldHashId = UITools.GetAnimatorNameHash(animator.GetCurrentAnimatorStateInfo(0));
				while ((UITools.GetAnimatorNameHash(animator.GetCurrentAnimatorStateInfo(0)) == oldHashId) && (Time.realtimeSinceStartup < timeout)) {
					yield return null;
				}
				yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
			}
			base.SetActive(false);
			if (panel != null) Tools.SetGameObjectActive(panel, false);
		}
		
	}
		
}
