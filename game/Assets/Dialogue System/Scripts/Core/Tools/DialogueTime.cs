using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// A static wrapper class for the built-in Time class. This class allows the user to specify
	/// whether the dialogue system functions in realtime or gameplay time. If the game is paused
	/// during conversations by setting <c>Time.timeScale = 0</c>, then the dialogue system should
	/// use realtime or it will also be paused. However, if you want the dialogue system to
	/// observe the timeScale, then you can use gameplay time (for example, if you want the 
	/// Sequencer to observe timeScale).
	/// </summary>
	/// <remarks>
	/// The current public properties should stay the same in later versions, but I may add 
	/// additional public properties or change the underlying private implementation.
	/// </remarks>
	public static class DialogueTime {
		
		/// <summary>
		/// Dialogue System time mode.
		/// </summary>
		public enum TimeMode {

			/// <summary>
			/// Ignore Time.timeScale. Internally, use Time.realtimeSinceStartup.
			/// </summary>
			Realtime,

			/// <summary>
			/// Observe Time.timeScale. Internally, use Time.time.
			/// </summary>
			Gameplay
		}
		
		/// <summary>
		/// Gets or sets the time mode.
		/// </summary>
		/// <value>
		/// The mode.
		/// </value>
		public static TimeMode Mode { get; set; }

		/// <summary>
		/// Gets the time based on the current Mode.
		/// </summary>
		/// <value>
		/// The time.
		/// </value>
		public static float time {
			get { 
				switch (Mode) {
				case TimeMode.Realtime:
					return (m_isPaused ? realtimeWhenPaused : Time.realtimeSinceStartup) - totalRealtimePaused;
				case TimeMode.Gameplay: return Time.time; 
				default:
					return Time.time;
				}
			}
		}
		
		public static bool IsPaused {
			get { 
				switch (Mode) {
				default:
				case TimeMode.Realtime:
					return m_isPaused;
				case TimeMode.Gameplay:
					return Tools.ApproximatelyZero(Time.timeScale);
				}
			}
			set {
				switch (Mode) {
				case TimeMode.Realtime:
					if (!m_isPaused && value) { 
						// Pausing, so record realtime at time of pause:
						realtimeWhenPaused = Time.realtimeSinceStartup;
					} else if (m_isPaused && !value) {
						// Unpausing, so add to totalRealtimePaused:
						totalRealtimePaused += Time.realtimeSinceStartup - realtimeWhenPaused;
					}
					break;
				case TimeMode.Gameplay:
					Time.timeScale = m_isPaused ? 0 : 1;
					break;
				}
				m_isPaused = value;
			}
		}

		private static bool m_isPaused = false;

		private static float realtimeWhenPaused = 0;
		
		private static float totalRealtimePaused = 0;
		
		/// <summary>
		/// Initializes the <see cref="PixelCrushers.DialogueSystem.DialogueTime"/> class.
		/// </summary>
		static DialogueTime() {
			Mode = TimeMode.Realtime;
		}
	
	}

}