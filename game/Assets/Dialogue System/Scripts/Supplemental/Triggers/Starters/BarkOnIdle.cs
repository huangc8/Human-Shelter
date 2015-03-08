using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// The bark on idle component can be used to make an NPC bark on timed intervals.
	/// Barks don't occur while a conversation is active.
	/// </summary>
	[AddComponentMenu("Dialogue System/Trigger/Bark On Idle")]
	public class BarkOnIdle : BarkStarter {
		
		/// <summary>
		/// The minimum seconds between barks.
		/// </summary>
		public float minSeconds = 5f;
		
		/// <summary>
		/// The maximum seconds between barks.
		/// </summary>
		public float maxSeconds = 10f;
		
		/// <summary>
		/// The target to bark at. Leave unassigned to just bark into the air.
		/// </summary>
		public Transform target;
		
		void Start() {
			StartBarkLoop();
		}
		
		/// <summary>
		/// Starts the bark loop. Normally this is started in the Start() method. If you need to
		/// restart it for some reason, call this method.
		/// </summary>
		public void StartBarkLoop() {
			StartCoroutine(BarkLoop());
		}
		
		private IEnumerator BarkLoop() {
			while (true) {
				yield return new WaitForSeconds(Random.Range(minSeconds, maxSeconds));
				if (enabled && !DialogueManager.IsConversationActive && !DialogueTime.IsPaused) {
					TryBark(target);
				}
			}
		}
		
	}

}
