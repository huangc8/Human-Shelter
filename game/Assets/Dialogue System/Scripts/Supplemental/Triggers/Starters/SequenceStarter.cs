using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// This is the base class for all sequence trigger components.
	/// </summary>
	public class SequenceStarter : DialogueEventStarter {
	
		/// <summary>
		/// The sequence to play. See @ref sequencer
		/// </summary>
		[Multiline]
		public string sequence;
		
		/// <summary>
		/// The speaker to use for the sequence (or null if no speaker is needed). Sequence
		/// commands can reference 'speaker' and 'listener', so you may need to define them
		/// in this component.
		/// </summary>
		public Transform speaker;
		
		/// <summary>
		/// The listener to use for the sequence (or null if no listener is needed). Sequence
		/// commands can reference 'speaker' and 'listener', so you may need to define them
		/// in this component.
		/// </summary>
		public Transform listener;
		
		/// <summary>
		/// The condition required to allow the sequence to start.
		/// </summary>
		public Condition condition;
		
		private bool tryingToStart = false;
		
		/// <summary>
		/// Starts the sequence if the condition is true.
		/// </summary>
		public void TryStartSequence(Transform actor) {
			if (tryingToStart) return;
			tryingToStart = true;
			try {
				if (((condition == null) || condition.IsTrue(actor)) && !string.IsNullOrEmpty(sequence)) {
					DialogueManager.PlaySequence(sequence, Tools.Select(speaker, this.transform), Tools.Select(listener, actor));
					DestroyIfOnce();
				}
			} finally {
				tryingToStart = false;
			}
		}	
		
	}

}
