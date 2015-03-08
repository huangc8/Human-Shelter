using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// A simple static class to keep track of a global debug level setting for dialogue system
	/// log messages. The DialogueManager / DialogueSystemController sets Level. You can also
	/// set it manually. This class doesn't provide any wrappers for Debug.Log() because they
	/// would intercept the reference point that the editor goes to when you double-click the
	/// log message in the console window.
	/// </summary>
	public static class DialogueDebug {
		
		/// <summary>
		/// Dialogue system log messages are prefixed with this string.
		/// </summary>
		public const string Prefix = "Dialogue System";
		
		/// <summary>
		/// The debug levels.
		/// 
		/// - None: Don't log anything.
		/// - Error: Only log critical errors.
		/// - Warning: Log warnings and errors.
		/// - Info: Log trace information, warnings, and errors.
		/// </summary>
		public enum DebugLevel {
			None = 0,
			Error = 1,
			Warning = 2,
			Info = 3
		}
		
		/// <summary>
		/// The current global debug level.
		/// </summary>
		/// <value>
		/// The level.
		/// </value>
		public static DebugLevel Level { get; set; }
		
		static DialogueDebug() {
			Level = DebugLevel.Warning;
		}
		
		/// <summary>
		/// Should the dialogue system log trace information?
		/// </summary>
		/// <value>
		/// <c>true</c> if it should log trace info, warnings, and errors; otherwise, <c>false</c>.
		/// </value>
		public static bool LogInfo { 
			get { return (Level >= DebugLevel.Info); }
		}
		
		/// <summary>
		/// Should the dialogue system log warnings and trace info?
		/// </summary>
		/// <value>
		/// <c>true</c> to log warnings and errors; otherwise, <c>false</c>.
		/// </value>
		public static bool LogWarnings { 
			get { return (Level >= DebugLevel.Warning); }
		}
		
		/// <summary>
		/// Should the dialogue system log critical errors?
		/// </summary>
		/// <value>
		/// <c>true</c> to log errors; otherwise, <c>false</c>.
		/// </value>
		public static bool LogErrors { 
			get { return (Level >= DebugLevel.Error); }
		}
		
	}

}
