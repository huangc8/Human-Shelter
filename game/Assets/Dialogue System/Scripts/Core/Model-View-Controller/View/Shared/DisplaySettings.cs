using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Display settings to apply to the dialogue UI and sequencer.
	/// </summary>
	[System.Serializable]
	public class DisplaySettings {
	
		public GameObject dialogueUI;
		
		[System.Serializable]
		public class LocalizationSettings {
			/// <summary>
			/// The current language, or blank to use the default language.
			/// </summary>
			public string language = string.Empty;

			/// <summary>
			/// Set <c>true</c> to automatically use the system language at startup.
			/// </summary>
			public bool useSystemLanguage = false;

			/// <summary>
			/// An optional localized text table. Used by DialogueSystemController.GetLocalizedText()
			/// and ShowAlert() if assigned.
			/// </summary>
			public LocalizedTextTable localizedText = null;
		}
		
		public LocalizationSettings localizationSettings = new LocalizationSettings();

		[System.Serializable]
		public class SubtitleSettings {
			/// <summary>
			/// Specifies whether to show NPC subtitles while speaking a line of dialogue.
			/// </summary>
			public bool showNPCSubtitlesDuringLine = true;
			
			/// <summary>
			/// Specifies whether to should show NPC subtitles while presenting the player's follow-up
			/// responses.
			/// </summary>
			public bool showNPCSubtitlesWithResponses = true;
		
			/// <summary>
			/// Specifies whether to show PC subtitles while speaking a line of dialogue.
			/// </summary>
			public bool showPCSubtitlesDuringLine = false;

			/// <summary>
			/// Set <c>true</c> to allow PC subtitles to be used for the reminder line
			/// during the response menu.
			/// </summary>
			public bool allowPCSubtitleReminders = false;
			
			/// <summary>
			/// The default subtitle characters per second. This value is used to compute the default 
			/// duration to display a subtitle if no sequence is specified for a line of dialogue.
			/// This value is also used when displaying alerts.
			/// </summary>
			public float subtitleCharsPerSecond = 30f;
			
			/// <summary>
			/// The minimum duration to display a subtitle if no sequence is specified for a line of 
			/// dialogue. This value is also used when displaying alerts.
			/// </summary>
			public float minSubtitleSeconds = 2f;
			
			public enum ContinueButtonMode {
				/// <summary>
				/// Never wait for the continue button. Use this if your UI doesn't have continue buttons.
				/// </summary>
				Never,

				/// <summary>
				/// Always wait for the continue button.
				/// </summary>
				Always,

				/// <summary>
				/// Wait for the continue button, except when the response menu is next show but don't wait.
				/// </summary>
				OptionalBeforeResponseMenu,

				/// <summary>
				/// Wait for the continue button, except when the response menu is next hide it.
				/// </summary>
				NotBeforeResponseMenu,

				/// <summary>
				/// Wait for the continue button, except when a PC auto-select response or response
				/// menu is next, show but don't wait.
				/// </summary>
				OptionalBeforePC,

				/// <summary>
				/// Wait for the continue button, except with a PC auto-select response or response
				/// menu is next, hide it.
				/// </summary>
				NotBeforePC
			}

			/// <summary>
			/// How to handle continue buttons.
			/// </summary>
			public ContinueButtonMode continueButton = ContinueButtonMode.Never;
			
			/// <summary>
			/// Set <c>true</c> to convert "[em#]" tags to rich text codes in formatted text.
			/// Your implementation of IDialogueUI must support rich text.
			/// </summary>
			public bool richTextEmphases = false;

			/// <summary>
			/// Set <c>true</c> to send OnSequenceStart and OnSequenceEnd messages with 
			/// every dialogue entry's sequence.
			/// </summary>
			public bool informSequenceStartAndEnd = false;
		}
		
		/// <summary>
		/// The subtitle settings.
		/// </summary>
		public SubtitleSettings subtitleSettings = new SubtitleSettings();
	
		[System.Serializable]
		public class CameraSettings {
			/// <summary>
			/// The camera (or prefab) to use for sequences. If unassigned, the sequencer will use the
			/// main camera; when the sequence is done, it will restore the main camera's original
			/// position.
			/// </summary>
			public Camera sequencerCamera = null;

			/// <summary>
			/// An alternate camera object to use instead of sequencerCamera. Use this, for example,
			/// if you have an Oculus VR GameObject that's a parent of two cameras.  Currently this 
			/// <em>must</em> be an object in the scene, not a prefab.
			/// </summary>
			public GameObject alternateCameraObject = null;
			
			/// <summary>
			/// The camera angle object (or prefab) to use for the "Camera()" sequence command. See
			/// @ref sequencerCommandCamera for more information.
			/// </summary>
			public GameObject cameraAngles = null;
			
			/// <summary>
			/// The default sequence to use if the dialogue entry doesn't have a sequence defined 
			/// in its Video File field. See @ref dialogueCreation and @ref sequencer for
			/// more information. The special keyword "{{end}}" gets replaced by the default
			/// duration for the subtitle being displayed.
			/// </summary>
			public string defaultSequence = "Camera(default); required Camera(default,listener)@{{end}}";

			/// <summary>
			/// The format to use for the <c>entrytag</c> keyword.
			/// </summary>
			public EntrytagFormat entrytagFormat = EntrytagFormat.ActorName_ConversationID_EntryID;

			/// <summary>
			/// Set <c>true</c> to disable the internal sequencer commands -- for example, if you
			/// want to replace them with your own.
			/// </summary>
			public bool disableInternalSequencerCommands = false;
		}
		
		/// <summary>
		/// The camera settings.
		/// </summary>
		public CameraSettings cameraSettings = new CameraSettings();
		
		[System.Serializable]
		public class InputSettings {
			
			/// <summary>
			/// If <c>true</c>, always forces the response menu even if there's only one response.
			/// If <c>false</c>, you can use the <c>[f]</c> tag to force a response.
			/// </summary>
			public bool alwaysForceResponseMenu = true;
			
			/// <summary>
			/// If not <c>0</c>, the duration in seconds that the player has to choose a response; 
			/// otherwise the currently-focused response is auto-selected. If no response is
			/// focused (e.g., hovered over), the first response is auto-selected. If <c>0</c>,
			/// there is no timeout; the player can take as long as desired to choose a response.
			/// </summary>
			public float responseTimeout = 0f;
			
			/// <summary>
			/// The response timeout action.
			/// </summary>
			public ResponseTimeoutAction responseTimeoutAction = ResponseTimeoutAction.ChooseFirstResponse;

			/// <summary>
			/// The em tag to wrap around old responses. A response is old if its SimStatus 
			/// is "WasDisplayed". You can change this from EmTag.None if you want to visually
			/// mark old responses in the player response menu.
			/// </summary>
			public EmTag emTagForOldResponses = EmTag.None;
			
			/// <summary>
			/// The buttons QTE (Quick Time Event) buttons. QTE 0 & 1 default to the buttons
			/// Fire1 and Fire2.
			/// </summary>
			public string[] qteButtons = new string[] { "Fire1", "Fire2" };
			
			/// <summary>
			/// The key and/or button that allows the player to cancel sequences or exit the 
			/// conversation.
			/// </summary>
			public InputTrigger cancel = new InputTrigger(KeyCode.Escape);
		}
		
		/// <summary>
		/// The input settings.
		/// </summary>
		public InputSettings inputSettings = new InputSettings();
		
		[System.Serializable]
		public class AlertSettings {
			
			/// <summary>
			/// Set <c>true</c> to allow the dialogue UI to show alerts during conversations.
			/// </summary>
			public bool allowAlertsDuringConversations = false;
		
			/// <summary>
			/// How often to check if the Lua Variable['Alert'] has been set. To disable
			/// automatic monitoring, set this to <c>0</c>.
			/// </summary>
			public float alertCheckFrequency = 0f;
			
		}
		
		/// <summary>
		/// The gameplay alert message settings.
		/// </summary>
		public AlertSettings alertSettings = new AlertSettings();
		
	}
	
	/// <summary>
	/// Response timeout action specifies what to do if the response menu times out.
	/// </summary>
	public enum ResponseTimeoutAction { 
		/// <summary>
		/// Auto-select the first menu choice.
		/// </summary>
		ChooseFirstResponse, 
		
		/// <summary>
		/// End of conversation.
		/// </summary>
		EndConversation };

	public enum EmTag {
		None,
		Em1,
		Em2,
		Em3,
		Em4
	}

}
