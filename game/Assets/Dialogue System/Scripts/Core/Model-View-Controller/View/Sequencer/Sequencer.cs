using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem.SequencerCommands;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// A sequencer plays sequences of commands such as camera cuts, animation, audio, activating
	/// game objects, etc. You can use the sequencer to play cutscenes or perform game actions.
	/// The dialogue system uses a sequencer to play a sequence for every line of dialogue. If the
	/// dialogue author hasn't specified a sequence for a line of dialogue, the dialogue system 
	/// will generate a basic, default sequence that aims the camera at the speaker.
	/// 
	/// See also: @ref sequencer
	/// 
	/// Each sequence command is implemented as a coroutine. You can add new commands by defining
	/// extension methods that are coroutines.
	/// 
	/// Out of the box, Sequencer supports the following commands:
	/// - None()
	/// - Delay(seconds)
	/// - Camera(angle[, gameobject|speaker|listener[, duration]])
	/// - Animation(animation[, gameobject|speaker|listener])
	/// - AnimatorBool(animatorParameter[, true|false[, gameobject|speaker|listener]])
	/// - Audio(clip[, gameobject|speaker|listener])
	/// - MoveTo(target[, subject[, duration]])
	/// - LookAt([target[, subject]])
	/// - SendMessage(methodName[, arg[, gameobject|speaker|listener]])
	/// - SetActive(gameobject[, true|false|flip])
	/// - SetEnabled(component[, true|false|flip[, subject]])
	/// - SetPortrait(gameobject)
	/// - ShowAlert([duration])
	/// </summary>
	public class Sequencer : MonoBehaviour {

		/// <summary>
		/// This handler is called when the sequence is done playing.
		/// </summary>
		public event Action FinishedSequenceHandler = null;
		
		/// <summary>
		/// A constant defining the name of the default camera angles prefab in case the cameraAngles property isn't set.
		/// </summary>
		private const string DefaultCameraAnglesResourceName = "Default Camera Angles";
		
		/// <summary>
		/// Indicates whether a sequence is currently playing. The dialogue system can queue up any number of actions
		/// using the Play() method. This property returns true if any actions are scheduled or active.
		/// </summary>
		/// <value>
		/// <c>true</c> if is playing; otherwise, <c>false</c>.
		/// </value>
		public bool IsPlaying { 
			get { return isPlaying; } 
		}
		
		public GameObject CameraAngles {
			get { return cameraAngles; } 
		}
		
		public Camera SequencerCamera {
			get { return sequencerCamera; }
		}

		public Transform SequencerCameraTransform {
			get { return (alternateSequencerCameraObject != null) ? alternateSequencerCameraObject.transform : sequencerCamera.transform; }
		}
		
		public Transform Speaker {
			get { return speaker; }
		}
		
		public Transform Listener {
			get { return listener; }
		}

		public Vector3 OriginalCameraPosition {
			get { return originalCameraPosition; }
		}

		public Quaternion OriginalCameraRotation {
			get { return originalCameraRotation; }
		}

		public string entrytag { get; set; }

		/// <summary>
		/// Set <c>true</c> to disable the internal sequencer commands -- for example,
		/// if you want to replace them all with your own.
		/// </summary>
		public bool disableInternalSequencerCommands = false;
		
		/// <summary>
		/// <c>true</c> if the sequencer has taken control of the main camera at some point. Used to restore the 
		/// original camera position when the sequencer is closed.
		/// </summary>
		private bool hasCameraControl = false;
		
		private Camera originalCamera = null;
		
		/// <summary>
		/// The original camera position before the sequencer took control. If the sequencer doesn't take control
		/// of the camera, this property is ignored.
		/// </summary>
		private Vector3 originalCameraPosition = Vector3.zero;
		
		/// <summary>
		/// The original camera rotation before the sequencer took control. If the sequencer doesn't take control
		/// of the camera, this property is ignored.
		/// </summary>
		private Quaternion originalCameraRotation = Quaternion.identity;

		/// <summary>
		/// The original orthographicSize before the sequencer took control.
		/// </summary>
		private float originalOrthographicSize = 16;
		
		private Transform speaker = null;
		
		private Transform listener = null;
		
		private List<QueuedSequencerCommand> queuedCommands = new List<QueuedSequencerCommand>();
		
		private List<SequencerCommand> activeCommands = new List<SequencerCommand>();
		
		private bool informParticipants = false;
		
		private bool closeWhenFinished = false;

		private Camera sequencerCameraSource = null;

		private Camera sequencerCamera = null;

		private GameObject alternateSequencerCameraObject = null;
		
		private GameObject cameraAngles = null;

		private bool isUsingMainCamera = false;

		private bool isPlaying = false;

		private const float InstantThreshold = 0.001f;

		private static Dictionary<string, System.Type> cachedComponentTypes = new Dictionary<string, Type>();

		/// <summary>
		/// Sends OnSequencerMessage(message) to the Dialogue Manager. Since sequencers are usually on 
		/// the Dialogue Manager object, this is a convenient way to send a message to a sequencer.
		/// You can use this method if your sequence is waiting for a message.
		/// </summary>
		/// <param name="message">Message to send.</param>
		public static void Message(string message) {
			DialogueManager.Instance.SendMessage("OnSequencerMessage", message, SendMessageOptions.DontRequireReceiver);
		}
		
		public void UseCamera(Camera sequencerCamera, GameObject cameraAngles) {
			UseCamera(sequencerCamera, null, cameraAngles);
		}
		
		public void UseCamera(Camera sequencerCamera, GameObject alternateSequencerCameraObject, GameObject cameraAngles) {
			this.originalCamera = Camera.main;
			this.sequencerCameraSource = sequencerCamera;
			this.alternateSequencerCameraObject = alternateSequencerCameraObject;
			this.cameraAngles = cameraAngles;
			GetCamera();
			GetCameraAngles();
		}

		private void GetCameraAngles() {
			if (cameraAngles == null) cameraAngles = DialogueManager.LoadAsset(DefaultCameraAnglesResourceName) as GameObject;
		}
		
		private void GetCamera() {
			if (sequencerCamera == null) {
				if (alternateSequencerCameraObject != null) {
					isUsingMainCamera = true;
					sequencerCamera = alternateSequencerCameraObject.GetComponent<Camera>();
				} else if (sequencerCameraSource != null) {
					GameObject source = sequencerCameraSource.gameObject;
					GameObject sequencerCameraObject = Instantiate(source, source.transform.position, source.transform.rotation) as GameObject;
					sequencerCamera = sequencerCameraObject.GetComponent<Camera>();
					if (sequencerCamera != null) {
						sequencerCamera.transform.parent = this.transform;
						sequencerCamera.gameObject.SetActive(false);
						isUsingMainCamera = false;
					} else {
						Destroy(sequencerCameraObject);
					}
				}
				if (sequencerCamera == null) {
					sequencerCamera = UnityEngine.Camera.main;
					isUsingMainCamera = true;
				}
				// Make sure a sequencerCamera exists:
				if (sequencerCamera == null) {
					GameObject go = new GameObject("Sequencer Camera", typeof(Camera), typeof(GUILayer), typeof(AudioListener));
					sequencerCamera = go.GetComponent<Camera>();
					isUsingMainCamera = true;
				}
			}
			// Make sure a camera is tagged MainCamera; use sequencerCamera if no other:
			if (UnityEngine.Camera.main == null && sequencerCamera != null) {
				sequencerCamera.tag = "MainCamera";
				isUsingMainCamera = true;
			}
		}

		private void DestroyCamera() {
			if ((sequencerCamera != null) && !isUsingMainCamera) {
				sequencerCamera.gameObject.SetActive(false);
				Destroy(sequencerCamera.gameObject, 1);
				sequencerCamera = null;
			}
		}
		
		/// <summary>
		/// Restores the original camera position. Waits two frames first, to allow any
		/// active, required actions to finish.
		/// </summary>
		private IEnumerator RestoreCamera() {
			yield return null;
			yield return null;
			ReleaseCameraControl();
		}

		/// <summary>
		/// Switches the sequencer camera to a different camera object immediately.
		/// Restores the previous camera first.
		/// </summary>
		/// <param name="newCamera">New camera.</param>
		public void SwitchCamera(Camera newCamera) {
			if ((sequencerCamera != null) && !isUsingMainCamera) {
				Destroy(sequencerCamera.gameObject, 1);
			}
			ReleaseCameraControl();
			hasCameraControl = false;
			originalCamera = null;
			originalCameraPosition = Vector3.zero;
			originalCameraRotation = Quaternion.identity;
			originalOrthographicSize = 16;
			sequencerCameraSource = null;
			sequencerCamera = null;
			alternateSequencerCameraObject = null;
			isUsingMainCamera = false;
			UseCamera(newCamera, cameraAngles);
			TakeCameraControl();
		}

		/// <summary>
		/// Takes control of the camera.
		/// </summary>
		public void TakeCameraControl() {
			if (hasCameraControl) return;
			hasCameraControl = true;
			if (alternateSequencerCameraObject != null) {
				originalCamera = sequencerCamera;
				originalCameraPosition = alternateSequencerCameraObject.transform.position;
				originalCameraRotation = alternateSequencerCameraObject.transform.rotation;
			} else {
				originalCamera = UnityEngine.Camera.main;
				originalCameraPosition = UnityEngine.Camera.main.transform.position;
				originalCameraRotation = UnityEngine.Camera.main.transform.rotation;
				originalOrthographicSize = sequencerCamera.orthographicSize;
				originalCamera.gameObject.SetActive(false);
				sequencerCamera.gameObject.SetActive(true);
			}
		}
		
		/// <summary>
		/// Releases control of the camera.
		/// </summary>
		private void ReleaseCameraControl() {
			if (!hasCameraControl) return;
			hasCameraControl = false;
			if (alternateSequencerCameraObject != null) {
				alternateSequencerCameraObject.transform.position = originalCameraPosition;
				alternateSequencerCameraObject.transform.rotation = originalCameraRotation;
			} else {
				sequencerCamera.transform.position = originalCameraPosition;
				sequencerCamera.transform.rotation = originalCameraRotation;
				sequencerCamera.orthographicSize = originalOrthographicSize;
				sequencerCamera.gameObject.SetActive(false);
				originalCamera.gameObject.SetActive(true);
			}
		}
		
		/// <summary>
		/// Opens this instance. Simply resets hasCameraControl.
		/// </summary>
		public void Open() {
			entrytag = string.Empty;
			GetCamera();
			hasCameraControl = false;
			GetCameraAngles();
		}
		
		/// <summary>
		/// Closes and destroy this sequencer. Stops all actions and restores the original camera 
		/// position.
		/// </summary>
		public void Close() {
			if (FinishedSequenceHandler != null) FinishedSequenceHandler();
			FinishedSequenceHandler = null;
			Stop();
			StartCoroutine(RestoreCamera());
			Destroy(this, 1);
		}

		public void OnDestroy() {
			DestroyCamera();
		}
		
		public void Update() {
			if (isPlaying) {
				CheckQueuedCommands();
				CheckActiveCommands();
				if ((queuedCommands.Count == 0) && (activeCommands.Count == 0)) FinishSequence();
			}
		}
		
		private void FinishSequence() {
			isPlaying = false;
			if (FinishedSequenceHandler != null) FinishedSequenceHandler();
			if (informParticipants) InformParticipants("OnSequenceEnd");
			if (closeWhenFinished) {
				FinishedSequenceHandler = null;
				Close();
			}
		}
		
		public void SetParticipants(Transform speaker, Transform listener) {
			this.speaker = speaker;
			this.listener = listener;
		}
		
		private void InformParticipants(string message) {
			if (speaker != null) speaker.BroadcastMessage(message, speaker, SendMessageOptions.DontRequireReceiver);
			if ((listener != null) && (listener != speaker)) listener.BroadcastMessage(message, speaker, SendMessageOptions.DontRequireReceiver);
		}
		
		/// <summary>
		/// Parses a sequence string and plays the individual commands.
		/// </summary>
		/// <param name='sequence'>
		/// The sequence to play, in the form:
		/// 
		/// <code>
		/// \<sequence\> ::= \<statement\> ; \<statement\> ; ...
		/// </code>
		/// 
		/// <code>
		/// \<statement\> ::= [required] \<command\>( \<arg\>, \<arg\>, ... ) [@\<time\>] [->Message(Y)]
		/// </code>
		/// 
		/// For example, the sequence below shows a wide angle shot of the speaker reloading and
		/// firing, and then cuts to a closeup of the listener.
		/// 
		/// <code>
		/// Camera(Wide); Animation(Reload); Animation(Fire)@2; required Camera(Closeup, listener)@3.5
		/// </code>
		/// </param>
		public void PlaySequence(string sequence) {
			isPlaying = true;
			if (string.IsNullOrEmpty(sequence)) return;
				
			// (This parser has grown quite unwieldy. In the future, I'll probably replace it
			// with a more formal parser using ANTLR or Coco/R.)

			// Substitute entrytag:
			if (!string.IsNullOrEmpty(entrytag) && sequence.Contains("entrytag")) {
				sequence = sequence.Replace("entrytag", entrytag);
			}

			// Split sequence into statements separated by semicolons:
			string[] statements = System.Text.RegularExpressions.Regex.Split(sequence, @"\s*;\s*");
			foreach (string statement in statements) {

				// Extract "->Message(Y)" from the end if it occurs. At the end of the command, it
				// will send a message Y to the sequencer.
				string endMessage = null;
				if (statement.Contains("->Message(")) {
					string endMessageCommand = statement.Substring(statement.IndexOf("->Message("));
					endMessage = endMessageCommand.Substring("->Message(".Length, endMessageCommand.Length - ("->Message(".Length + 1));
					endMessage.Trim();
				}
				
				// Parse the statement into "required command ( args ) @ time" (required, args & time are optional).
				// The time parameter can be a float or "Message(X)" where X is the message to wait for.
				string[] tokens = System.Text.RegularExpressions.Regex.Split(statement, @"\(|\)|\@");
				
				// Build a command out of the statement's tokens:
				string[] commandParts = tokens[0].Split(null);
				if (!((1 <= commandParts.Length) && (commandParts.Length <= 2))) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequence syntax error: {1}", new System.Object[] { DialogueDebug.Prefix, statement }));
					continue;
				}
				bool required = (string.Compare(commandParts[0], "required", System.StringComparison.OrdinalIgnoreCase) == 0);
				string command = commandParts[commandParts.Length - 1].Trim();
				string[] parameters = (tokens.Length >= 2) ? System.Text.RegularExpressions.Regex.Split(tokens[1], @"\s*,\s*") : null;
				float time = 0;
				string message = null;
				if (tokens.Length >= 4) { // tokens[2] is an empty token between ")@".
					if (string.Equals(tokens[3].Trim(), "Message", StringComparison.OrdinalIgnoreCase)) {
						if (tokens.Length >= 5) {
							message = tokens[4].Trim();
							time = string.IsNullOrEmpty(message) ? 0 : 365f * 86400f; // One year -- essentially forever, since we're really waiting for the message.
						} else {
							if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequence syntax error in @Message((): {1}", new System.Object[] { DialogueDebug.Prefix, statement }));
							continue;
						}
					} else {
						float.TryParse(tokens[3], out time); 
					}
				}
	
				// Send the command to the sequencer to play:
				PlayCommand(command, required, time, message, endMessage, parameters);
			}
		}
		
		public void PlaySequence(string sequence, bool informParticipants, bool destroyWhenDone) {
			this.closeWhenFinished = destroyWhenDone;
			this.informParticipants = informParticipants;
			if (informParticipants) InformParticipants("OnSequenceStart");
			PlaySequence(sequence);
		}		
		
		public void PlaySequence(string sequence, Transform speaker, Transform listener, bool informParticipants, bool destroyWhenDone) {
			SetParticipants(speaker, listener);
			PlaySequence(sequence, informParticipants, destroyWhenDone);
		}
		
		/// <summary>
		/// Schedules a command to be played.
		/// </summary>
		/// <param name='commandName'>
		/// The command to play. See @ref sequencerCommands for the list of valid commands.
		/// </param>
		/// <param name='required'>
		/// If <c>true</c>, the command will play even if Stop() is called. If this command absolutely must run (for example, 
		/// setting up the final camera angle at the end of the sequence), set required to true.
		/// </param>
		/// <param name='time'>
		/// The time delay in seconds at which to start the command. If time is <c>0</c>, the command starts immediately.
		/// </param>
		/// <param name='args'>
		/// An array of arguments for the command. Pass <c>null</c> if no arguments are required.
		/// </param>
		/// <example>
		/// // At the 2 second mark, cut the camera to a closeup of the listener.
		/// string[] args = new string[] { "Closeup", "listener" };
		/// Play("Camera", true, 2, args);
		/// </example>
		public void PlayCommand(string commandName, bool required, float time, string message, string endMessage, params string[] args) {
			if (DialogueDebug.LogInfo) {
				if (args != null) {
					if (string.IsNullOrEmpty(message) && string.IsNullOrEmpty(endMessage)) {
						Debug.Log(string.Format("{0}: Sequencer.Play( {1}{2}({3})@{4} )", new System.Object[] { DialogueDebug.Prefix, (required ? "required " : string.Empty), commandName, string.Join(", ", args), time }));
					} else if (string.IsNullOrEmpty(endMessage)) {
						Debug.Log(string.Format("{0}: Sequencer.Play( {1}{2}({3})@Message({4}) )", new System.Object[] { DialogueDebug.Prefix, (required ? "required " : string.Empty), commandName, string.Join(", ", args), message }));
					} else if (string.IsNullOrEmpty(message)) {
						Debug.Log(string.Format("{0}: Sequencer.Play( {1}{2}({3})->Message({4}) )", new System.Object[] { DialogueDebug.Prefix, (required ? "required " : string.Empty), commandName, string.Join(", ", args), endMessage }));
					} else {
						Debug.Log(string.Format("{0}: Sequencer.Play( {1}{2}({3})@Message({4})->Message({5}) )", new System.Object[] { DialogueDebug.Prefix, (required ? "required " : string.Empty), commandName, string.Join(", ", args), message, endMessage }));
					}
				}
			}
			isPlaying = true;
			if ((time <= InstantThreshold) && !IsTimePaused() && string.IsNullOrEmpty(message)) {
				ActivateCommand(commandName, endMessage, args);
			} else {
				queuedCommands.Add(new QueuedSequencerCommand(commandName, args, DialogueTime.time + time, message, endMessage, required));
			}
		}

		private bool IsTimePaused() {
			//---Was: return (DialogueTime.Mode == DialogueTime.TimeMode.Gameplay) && Tools.ApproximatelyZero(Time.timeScale);
			return DialogueTime.IsPaused;
		}
		
		private void ActivateCommand(string commandName, string endMessage, string[] args) {
			float duration = 0;
			if (string.IsNullOrEmpty(commandName)) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer received a blank string as a command name", new System.Object[] { DialogueDebug.Prefix }));
			} else if (HandleCommandInternally(commandName, args, out duration)) {
				if (!string.IsNullOrEmpty(endMessage)) StartCoroutine(SendTimedSequencerMessage(endMessage, duration));
			} else {
				//---Was (pre-Unity 5) SequencerCommand command = gameObject.AddComponent(string.Format("SequencerCommand{0}", new System.Object[] { commandName })) as SequencerCommand;
				System.Type componentType = FindSequencerCommandType(commandName);
				SequencerCommand command = (componentType == null) ? null : gameObject.AddComponent(componentType) as SequencerCommand;
				if (command != null) {
					command.Initialize(this, endMessage, args);
					activeCommands.Add(command);
				} else {
					//if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Can't find any built-in sequencer command named {1}() or a sequencer command component named SequencerCommand{1}()", new System.Object[] { DialogueDebug.Prefix, commandName }));
				}
			}
		}

		private System.Type FindSequencerCommandType(string commandName) {
			if (cachedComponentTypes.ContainsKey(commandName)) {
				return cachedComponentTypes[commandName];
			} else {
				var componentType = FindSequencerCommandType(commandName, "DialogueSystem");
				if (componentType == null) {
					componentType = FindSequencerCommandType(commandName, "Assembly-CSharp");
				}
				if (componentType != null) {
					cachedComponentTypes.Add(commandName, componentType);
				}
				return componentType;
			}
		}

		private System.Type FindSequencerCommandType(string commandName, string assemblyName) {
			System.Type componentType = FindSequencerCommandType("PixelCrushers.DialogueSystem.SequencerCommands.", commandName, assemblyName);
			if (componentType != null) return componentType;
			componentType = FindSequencerCommandType("PixelCrushers.DialogueSystem.", commandName, assemblyName);
			if (componentType != null) return componentType;
			componentType = FindSequencerCommandType(string.Empty, commandName, assemblyName);
			return componentType;
		}

		private System.Type FindSequencerCommandType(string namespacePrefix, string commandName, string assemblyName) {
			string fullPath = string.Format("{0}SequencerCommand{1},{2}", new System.Object[] { namespacePrefix, commandName, assemblyName });
			return Type.GetType(fullPath, false, false);
		}

		private IEnumerator SendTimedSequencerMessage(string endMessage, float delay) {
			yield return new WaitForSeconds(delay);
			Sequencer.Message(endMessage);
		}
		
		private void ActivateCommand(QueuedSequencerCommand queuedCommand) {
			ActivateCommand(queuedCommand.command, queuedCommand.endMessage, queuedCommand.parameters);
		}
		
		private void CheckQueuedCommands() {
			if ((queuedCommands.Count > 0) && !IsTimePaused()) {
				float now = DialogueTime.time;
				foreach (var queuedCommand in queuedCommands) {
					if (now >= queuedCommand.startTime) ActivateCommand(queuedCommand.command, queuedCommand.endMessage, queuedCommand.parameters);
				}
				queuedCommands.RemoveAll(queuedCommand => (now >= queuedCommand.startTime));
			}
		}
		
		public void OnSequencerMessage(string message) {
			try {
				if ((queuedCommands.Count > 0) && !IsTimePaused() && !string.IsNullOrEmpty(message)) {
					int count = queuedCommands.Count;
					for (int i = count - 1; i >= 0; i--) {
						var queuedCommand = queuedCommands[i];
						if (string.Equals(message, queuedCommand.messageToWaitFor)) {
							ActivateCommand(queuedCommand.command, queuedCommand.endMessage, queuedCommand.parameters);
						}
					}
					queuedCommands.RemoveAll(queuedCommand => (string.Equals(message, queuedCommand.messageToWaitFor)));
				}
			} catch (Exception e) {
				// We don't care if the collection is modified:
				bool ignore = (e is InvalidOperationException || e is ArgumentOutOfRangeException);
				if (!ignore) throw;
			}
		}
		
		private void CheckActiveCommands() {
			if (activeCommands.Count > 0) {
				List<SequencerCommand> done = activeCommands.FindAll(command => !command.IsPlaying);
				if (done.Count > 0) {
					foreach (SequencerCommand command in done) {
						if (command != null) {
							if (!string.IsNullOrEmpty(command.endMessage)) Sequencer.Message(command.endMessage);
							Destroy(command);
						}
					}
					activeCommands.RemoveAll(command => done.Contains(command));
				}
			}
		}
		
		/// <summary>
		/// Stops all scheduled and active commands.
		/// </summary>
		public void Stop() {
			StopQueued();
			StopActive();
		}
		
		public void StopQueued() {
			foreach (var queuedCommand in queuedCommands) {
				if (queuedCommand.required) ActivateCommand(queuedCommand.command, queuedCommand.endMessage, queuedCommand.parameters);
			}
			queuedCommands.Clear();
		}
		
		public void StopActive() {
			foreach (var command in activeCommands) {
				if (command != null) {
					if (!string.IsNullOrEmpty(command.endMessage)) Sequencer.Message(command.endMessage);
					Destroy(command, 0.1f);
				}
			}
			activeCommands.Clear();
		}
		
		/// <summary>
		/// Attempts to handles the command internally so the sequencer doesn't have to farm out 
		/// the work to a SequencerCommand component.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this method could handle the command internally; otherwise <c>false</c>.
		/// </returns>
		/// <param name='commandName'>
		/// The command to try to play.
		/// </param>
		/// <param name='args'>
		/// The arguments to the command.
		/// </param>
		private bool HandleCommandInternally(string commandName, string[] args, out float duration) {
			duration = 0;
			if (disableInternalSequencerCommands) return false;
			if (string.Equals(commandName, "None") || string.IsNullOrEmpty(commandName)) {
				return true;
			} else if (string.Equals(commandName, "Camera")) {
				return TryHandleCameraInternally(commandName, args);
			} else if (string.Equals(commandName, "Animation")) {
				return HandleAnimationInternally(commandName, args, out duration);
			} else if (string.Equals(commandName, "AnimatorController")) {
				return HandleAnimatorControllerInternally(commandName, args);
			} else if (string.Equals(commandName, "AnimatorLayer")) {
				return TryHandleAnimatorLayerInternally(commandName, args);
			} else if (string.Equals(commandName, "AnimatorBool")) {
				return HandleAnimatorBoolInternally(commandName, args);
			} else if (string.Equals(commandName, "AnimatorInt")) {
				return HandleAnimatorIntInternally(commandName, args);
			} else if (string.Equals(commandName, "AnimatorFloat")) {
				return TryHandleAnimatorFloatInternally(commandName, args);
			} else if (string.Equals(commandName, "AnimatorTrigger")) {
				return HandleAnimatorTriggerInternally(commandName, args);
			} else if (string.Equals(commandName, "AnimatorPlay")) {
				return HandleAnimatorPlayInternally(commandName, args);
			} else if (string.Equals(commandName, "Audio")) {
				return HandleAudioInternally(commandName, args);
			} else if (string.Equals(commandName, "MoveTo")) {
				return TryHandleMoveToInternally(commandName, args);
			} else if (string.Equals(commandName, "LookAt")) {
				return TryHandleLookAtInternally(commandName, args);
			} else if (string.Equals(commandName, "SendMessage")) {
				return HandleSendMessageInternally(commandName, args);
			} else if (string.Equals(commandName, "SetActive")) {
				return HandleSetActiveInternally(commandName, args);
			} else if (string.Equals(commandName, "SetEnabled")) {
				return HandleSetEnabledInternally(commandName, args);
			} else if (string.Equals(commandName, "SetPortrait")) {
				return HandleSetPortraitInternally(commandName, args);
			} else if (string.Equals(commandName, "ShowAlert")) {
				return HandleShowAlertInternally(commandName, args);
			}
			return false;
		}
		
		private bool TryHandleCameraInternally(string commandName, string[] args) {
			float duration = SequencerTools.GetParameterAsFloat(args, 2, 0);
			if (duration < InstantThreshold) {
				
				// Handle right now:
				string angle = SequencerTools.GetParameter(args, 0, "default");
				Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), speaker, listener);

				// Get the angle:
				bool isDefault = string.Equals(angle, "default");
				if (isDefault) angle = SequencerTools.GetDefaultCameraAngle(subject);
				bool isOriginal = string.Equals(angle, "original");
				Transform angleTransform = isOriginal
					? originalCamera.transform
					: ((cameraAngles != null) ? cameraAngles.transform.Find(angle) : null);
				bool isLocalTransform = true;
				if (angleTransform == null) {
					isLocalTransform = false;
					GameObject go = GameObject.Find(angle);
					if (go != null) angleTransform = go.transform;
				}

				// Log:
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: Camera({1}, {2}, {3}s)", new System.Object[] { DialogueDebug.Prefix, angle, Tools.GetObjectName(subject), duration }));
				if ((angleTransform == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Camera angle '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, angle }));
				if ((subject == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Camera subject '{1}' wasn't found.", new System.Object[] { DialogueDebug.Prefix, SequencerTools.GetParameter(args, 1) }));
			
				// If we have a camera angle and subject, move the camera to it:
				TakeCameraControl();
				if (isOriginal) {
					SequencerCameraTransform.rotation = OriginalCameraRotation;
					SequencerCameraTransform.position = OriginalCameraPosition;
				} else if (angleTransform != null && subject != null) {
					Transform cameraTransform = SequencerCameraTransform;
					if (isLocalTransform) {
						cameraTransform.rotation = subject.rotation * angleTransform.localRotation;
						cameraTransform.position = subject.position + subject.rotation * angleTransform.localPosition;
					} else {
						cameraTransform.rotation = angleTransform.rotation;
						cameraTransform.position = angleTransform.position;
					}
				}
				return true;
			} else {
				return false;
			}
 		}
		
		/// <summary>
		/// Handles the "Animation(animation[, gameobject|speaker|listener[, finalAnimation]])" action.
		/// 
		/// Arguments:
		/// -# Name of a legacy animation in the Animation component.
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
		/// </summary>
		private bool HandleAnimationInternally(string commandName, string[] args, out float duration) {
			duration = 0;

			// If the command has >2 args (last is finalAnimation), need to handle in the coroutine version:
			if ((args != null) && (args.Length > 2)) return false;
			
			string animation = SequencerTools.GetParameter(args, 0);
			Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), speaker, listener);
			Animation anim = (subject == null) ? null : subject.GetComponent<Animation>();
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: Animation({1}, {2})", new System.Object[] { DialogueDebug.Prefix, animation, Tools.GetObjectName(subject) }));
			if (subject == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Animation() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
			} else if (anim == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Animation() command: no Animation component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
			} else if (string.IsNullOrEmpty(animation)) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Animation() command: Animation name is blank.", new System.Object[] { DialogueDebug.Prefix }));
			} else {
				anim.CrossFade(animation);
				duration = anim[animation].length;
			}
			return true;
		}
		
		/// <summary>
		/// Handles the "AnimatorController(controllerName[, gameobject|speaker|listener])" action.
		/// 
		/// Arguments:
		/// -# Path to an animator controller inside a Resources folder.
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
		/// </summary>
		private bool HandleAnimatorControllerInternally(string commandName, string[] args) {
			string controllerName = SequencerTools.GetParameter(args, 0);
			Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), speaker, listener);
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorController({1}, {2})", new System.Object[] { DialogueDebug.Prefix, controllerName, Tools.GetObjectName(subject) }));
				
			// Load animator controller:
			RuntimeAnimatorController animatorController = null;
			try {
				animatorController = Instantiate(DialogueManager.LoadAsset(controllerName)) as RuntimeAnimatorController;
			} catch (Exception) {
			}
			if (subject == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorController() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
			} else if (animatorController == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorController() command: failed to load animator controller '{1}'.", new System.Object[] { DialogueDebug.Prefix, controllerName }));
			} else {
				Animator animator = subject.GetComponentInChildren<Animator>();
				if (animator == null) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorController() command: No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
				} else {
					animator.runtimeAnimatorController = animatorController;
				}
			}
			return true;
		}
		
		/// <summary>
		/// Handles the "AnimatorLayer(layerIndex[, weight[, gameobject|speaker|listener[, duration]]])" 
		/// action if duration is zero.
		/// 
		/// Arguments:
		/// -# Index number of a layer on the subject's animator controller. Default: 1.
		/// -# (Optional) New weight. Default: <c>1f</c>.
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
		/// -# (Optional) Duration in seconds to smooth to the new weight.
		/// </summary>
		private bool TryHandleAnimatorLayerInternally(string commandName, string[] args) {
			float duration = SequencerTools.GetParameterAsFloat(args, 3, 0);
			if (duration < InstantThreshold) {
				
				int layerIndex = SequencerTools.GetParameterAsInt(args, 0, 1);
				float weight = SequencerTools.GetParameterAsFloat(args, 1, 1f);
				Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), speaker, listener);
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorLayer({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, layerIndex, weight, Tools.GetObjectName(subject) }));
				if (subject == null) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorLayer() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
				} else {
					Animator animator = subject.GetComponentInChildren<Animator>();
					if (animator == null) {
						if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorLayer(): No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
					} else {
						animator.SetLayerWeight(layerIndex, weight);
					}
				}
				return true;
				
			} else {
				return false;
			}
		}
		
		/// <summary>
		/// Handles the "AnimatorBool(animatorParameter[, true|false[, gameobject|speaker|listener]])" action.
		/// 
		/// Arguments:
		/// -# Name of a Mecanim animator parameter.
		/// -# (Optional) True or false. Default: <c>true</c>.
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
		/// </summary>
		private bool HandleAnimatorBoolInternally(string commandName, string[] args) {
			string animatorParameter = SequencerTools.GetParameter(args, 0);
			bool parameterValue = SequencerTools.GetParameterAsBool(args, 1, true);
			Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), speaker, listener);
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorBool({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, animatorParameter, parameterValue, Tools.GetObjectName(subject) }));
			if (subject == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorBool() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
			} else if (string.IsNullOrEmpty(animatorParameter)) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorBool() command: animator parameter name is blank.", new System.Object[] { DialogueDebug.Prefix }));
			} else {
				Animator animator = subject.GetComponentInChildren<Animator>();
				if (animator == null) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
				} else {
					animator.SetBool(animatorParameter, parameterValue);
				}
			}
			return true;
		}
		
		/// <summary>
		/// Handles the "AnimatorInt(animatorParameter[, value[, gameobject|speaker|listener]])" action.
		/// 
		/// Arguments:
		/// -# Name of a Mecanim animator parameter.
		/// -# (Optional) Integer value. Default: <c>1</c>.
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
		/// </summary>
		private bool HandleAnimatorIntInternally(string commandName, string[] args) {
			string animatorParameter = SequencerTools.GetParameter(args, 0);
			int parameterValue = SequencerTools.GetParameterAsInt(args, 1, 1);
			Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), speaker, listener);
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorInt({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, animatorParameter, parameterValue, Tools.GetObjectName(subject) }));
			if (subject == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorInt() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
			} else if (string.IsNullOrEmpty(animatorParameter)) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorInt() command: animator parameter name is blank.", new System.Object[] { DialogueDebug.Prefix }));
			} else {
				Animator animator = subject.GetComponentInChildren<Animator>();
				if (animator == null) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
				} else {
					animator.SetInteger(animatorParameter, parameterValue);
				}
			}
			return true;
		}
		
		/// <summary>
		/// Handles the "AnimatorFloat(animatorParameter[, value[, gameobject|speaker|listener[, duration]]])" 
		/// action if duration is zero.
		/// 
		/// Arguments:
		/// -# Name of a Mecanim animator parameter.
		/// -# (Optional) Float value. Default: <c>1f</c>.
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
		/// -# (Optional) Duration in seconds to smooth to the value.
		/// </summary>
		private bool TryHandleAnimatorFloatInternally(string commandName, string[] args) {
			float duration = SequencerTools.GetParameterAsFloat(args, 3, 0);
			if (duration < InstantThreshold) {
				
				string animatorParameter = SequencerTools.GetParameter(args, 0);
				float parameterValue = SequencerTools.GetParameterAsFloat(args, 1, 1f);
				Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), speaker, listener);
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorFloat({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, animatorParameter, parameterValue, Tools.GetObjectName(subject) }));
				if (subject == null) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorFloat() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
				} else if (string.IsNullOrEmpty(animatorParameter)) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorFloat() command: animator parameter name is blank.", new System.Object[] { DialogueDebug.Prefix }));
				} else {
					Animator animator = subject.GetComponentInChildren<Animator>();
					if (animator == null) {
						if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
					} else {
						animator.SetFloat(animatorParameter, parameterValue);
					}
				}
				return true;
				
			} else {
				return false;
			}
		}
		
		/// <summary>
		/// Handles the "AnimatorTrigger(animatorParameter[, gameobject|speaker|listener])" action,
		/// which sets a trigger parameter on a subject's Animator.
		/// 
		/// Arguments:
		/// -# Name of a Mecanim animator state.
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
		/// </summary>
		private bool HandleAnimatorTriggerInternally(string commandName, string[] args) {
			string animatorParameter = SequencerTools.GetParameter(args, 0);
			Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), speaker, listener);
			Animator animator = (subject != null) ? subject.GetComponentInChildren<Animator>() : null;
			if (animator == null) {
				if (DialogueDebug.LogWarnings) Debug.Log(string.Format("{0}: Sequencer: AnimatorTrigger({1}, {2}): No Animator found on {2}", new System.Object[] { DialogueDebug.Prefix, animatorParameter, (subject != null) ? subject.name : SequencerTools.GetParameter(args, 1) }));
			} else {
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorTrigger({1}, {2})", new System.Object[] { DialogueDebug.Prefix, animatorParameter, subject }));
			}
			if (animator != null) animator.SetTrigger(animatorParameter);
			return true;
		}
		
		/// <summary>
		/// Handles the "AnimatorPlay(stateName[, gameobject|speaker|listener[, [crossfadeDuration]])" action.
		/// 
		/// Arguments:
		/// -# Name of a Mecanim animator state.
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
		/// -# (Optional) Crossfade duration. Default: 0 (play immediately).
		/// </summary>
		private bool HandleAnimatorPlayInternally(string commandName, string[] args) {
			string stateName = SequencerTools.GetParameter(args, 0);
			Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), speaker, listener);
			float crossfadeDuration = SequencerTools.GetParameterAsFloat(args, 2);
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: AnimatorPlay({1}, {2})", new System.Object[] { DialogueDebug.Prefix, stateName, Tools.GetObjectName(subject) }));
			if (subject == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorPlay() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
			} else if (string.IsNullOrEmpty(stateName)) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: AnimatorPlay() command: state name is blank.", new System.Object[] { DialogueDebug.Prefix }));
			} else {
				Animator animator = subject.GetComponentInChildren<Animator>();
				if (animator == null) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: No Animator component found on {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
				} else {
					if (Tools.ApproximatelyZero(crossfadeDuration)) {
						animator.Play(stateName);
					} else {
						animator.CrossFade(stateName, crossfadeDuration);
					}
				}
			}
			return true;
		}
		
		/// <summary>
		/// Handles the "Audio(clip[, gameobject|speaker|listener])" action. This action loads the 
		/// specified clip from Resources into the subject's audio source component and plays it.
		/// 
		/// Arguments:
		/// -# Path to the clip (inside a Resources folder).
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. 
		/// Default: speaker.
		/// </summary>
		private bool HandleAudioInternally(string commandName, string[] args) {
			string clipName = SequencerTools.GetParameter(args, 0);
			Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), speaker, listener);

			// Skip if muted:
			if (SequencerTools.IsAudioMuted()) {
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: Audio({1}, {2}): skipping; audio is muted", new System.Object[] { DialogueDebug.Prefix, clipName, subject }));
				return true;
			} else {
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: Audio({1}, {2})", new System.Object[] { DialogueDebug.Prefix, clipName, subject }));
			}

			// Load clip:
			AudioClip clip = DialogueManager.LoadAsset(clipName) as AudioClip;
			if ((clip == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Audio() command: clip is null.", new System.Object[] { DialogueDebug.Prefix }));
	
			// Play clip:
			if (clip != null) {
				AudioSource audioSource = SequencerTools.GetAudioSource(subject);
				if (audioSource == null) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: Audio() command: can't find or add AudioSource to {1}.", new System.Object[] { DialogueDebug.Prefix, subject.name }));
				} else {
					if (Tools.ApproximatelyZero(audioSource.volume)) audioSource.volume = 1f;
					audioSource.clip = clip;
					audioSource.Play();
				}
			}
			return true;
		}
		
		/// <summary>
		/// Tries to handle the "MoveTo(target, [, subject[, duration]])" action. This action matches the 
		/// subject to the target's position and rotation.
		/// 
		/// Arguments:
		/// -# The target. 
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. 
		/// -# (Optional) Duration in seconds.
		/// Default: speaker.
		/// </summary>
		private bool TryHandleMoveToInternally(string commandName, string[] args) {
			float duration = SequencerTools.GetParameterAsFloat(args, 2, 0);
			if (duration < InstantThreshold) {
				
				// Handle now:
				Transform target = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 0), speaker, listener);
				Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), speaker, listener);
				if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: MoveTo({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, target, subject, duration }));
				if ((subject == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: MoveTo() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
				if ((target == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: MoveTo() command: target is null.", new System.Object[] { DialogueDebug.Prefix }));
				if (subject != null && target != null) {
					subject.position = target.position;
					subject.rotation = target.rotation;
				}
				return true;
			} else {
				return false;
			}
		}
		
		/// <summary>
		/// Tries to handle the "LookAt([target[, subject[, duration[, allAxes]]]])" action. This action
		/// rotates the subject to look at a target. If target is omitted, this action rotates
		/// the speaker and listener to look at each other.
		/// 
		/// Arguments:
		/// -# Target to look at. Can be speaker, listener, or the name of a game object. Default: listener.
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
		/// -# (Optional) Duration in seconds.
		/// -# (Optional) allAxes to rotate on all axes (otherwise stays upright).
		/// </summary>
		private bool TryHandleLookAtInternally(string commandName, string[] args) {
			float duration = SequencerTools.GetParameterAsFloat(args, 2, 0);
			bool yAxisOnly = (string.Compare(SequencerTools.GetParameter(args, 3), "allAxes", System.StringComparison.OrdinalIgnoreCase) != 0);

			if (duration < InstantThreshold) {
				
				// Handle now:
				if ((args == null) || ((args.Length == 1) && string.IsNullOrEmpty(args[0]))) {
					// Handle empty args (speaker and listener look at each other):
					if ((speaker != null) && (listener != null)) {
						if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: LookAt() [speaker<->listener]", new System.Object[] { DialogueDebug.Prefix }));
						DoLookAt(speaker, listener.position, yAxisOnly); //speaker.transform.LookAt(listener.position);
						DoLookAt(listener, speaker.position, yAxisOnly); //listener.transform.LookAt(speaker.position);
					}
				} else {
					// Otherwise handle subject and target:
					Transform target = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 0), speaker, listener);
					Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 1), speaker, listener);
					if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: LookAt({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, target, subject, duration }));
					if ((subject == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: LookAt() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
					if ((target == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: LookAt() command: target is null.", new System.Object[] { DialogueDebug.Prefix }));
					if ((subject != target) && (subject != null) && (target != null)) {
						DoLookAt(subject, target.position, yAxisOnly); //subject.transform.LookAt(target.position);
					}
				}
				return true;

			} else {
				return false;
			}
		}

		private void DoLookAt(Transform subject, Vector3 position, bool yAxisOnly) {
			if (yAxisOnly) {
				subject.LookAt(new Vector3(position.x, subject.position.y, position.z));
			} else {
				subject.LookAt(position);
			}
		}
		
		/// <summary>
		/// Handles the "SendMessage(methodName[, arg[, gameobject|speaker|listener]])" action.
		/// This action calls GameObject.SendMessage(methodName, arg) on the subject. Doesn't 
		/// require receiver.
		/// 
		/// Arguments:
		/// -# A methodName to run on the subject.
		/// -# (Optional) A string argument to pass to the method.
		/// -# (Optional) The subject; can be speaker, listener, or the name of a game object. Default: speaker.
		/// </summary>
		private bool HandleSendMessageInternally(string commandName, string[] args) {
			string methodName = SequencerTools.GetParameter(args, 0);
			string arg = SequencerTools.GetParameter(args, 1);
			Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), speaker, listener);
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: SendMessage({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, methodName, arg, subject }));
			if ((subject == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SendMessage() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
			if (string.IsNullOrEmpty(methodName) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SendMessage() command: message is blank.", new System.Object[] { DialogueDebug.Prefix }));
			if (subject != null && !string.IsNullOrEmpty(methodName)) subject.SendMessage(methodName, arg, SendMessageOptions.DontRequireReceiver);
			return true;
		}
		
		/// <summary>
		/// Handles the "SetActive(gameobject[, true|false|flip])" action.
		/// 
		/// Arguments:
		/// -# The name of a game object. Can't be speaker or listener, since they're involved in the conversation.
		/// -# (Optional) true, false, or flip (negate the current value).
		/// </summary>
		private bool HandleSetActiveInternally(string commandName, string[] args) {
			GameObject subject = SequencerTools.FindSpecifier(SequencerTools.GetParameter(args, 0));
			string arg = SequencerTools.GetParameter(args, 1);
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: SetActive({1}, {2})", new System.Object[] { DialogueDebug.Prefix, subject, arg }));
			if (subject == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetActive() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
			} else if ((subject == speaker) || (subject == listener)) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetActive() command: subject cannot be speaker or listener.", new System.Object[] { DialogueDebug.Prefix }));
			} else {
				bool newValue = true;
				if (!string.IsNullOrEmpty(arg)) {
					if (string.Equals(arg.ToLower(), "false")) newValue = false;
					else if (string.Equals(arg.ToLower(), "flip")) newValue = !subject.activeSelf;
				}
				subject.SetActive(newValue);
			}
			return true;
		}
		
		/// <summary>
		/// Handles the "SetEnabled(component[, true|false|flip[, subject]])" action.
		/// 
		/// Arguments:
		/// -# The name of a component on the subject.
		/// -# (Optional) true, false, or flip (negate the current value).
		/// -# (Optional) The subject (speaker, listener, or the name of a game object); defaults to speaker.
		/// </summary>
		private bool HandleSetEnabledInternally(string commandName, string[] args) {
			string componentName = SequencerTools.GetParameter(args, 0);
			string arg = SequencerTools.GetParameter(args, 1);
			Transform subject = SequencerTools.GetSubject(SequencerTools.GetParameter(args, 2), speaker, listener);
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: SetEnabled({1}, {2}, {3})", new System.Object[] { DialogueDebug.Prefix, componentName, arg, subject }));
			if (subject == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetEnabled() command: subject is null.", new System.Object[] { DialogueDebug.Prefix }));
			} else {
				Component component = subject.GetComponent(componentName) as Component;
				if (component == null) {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Sequencer: SetEnabled() command: component '{1}' not found on {2}.", new System.Object[] { DialogueDebug.Prefix, componentName, subject.name }));
				} else {
					Toggle state = Toggle.True;
					if (!string.IsNullOrEmpty(arg)) {
						if (string.Equals(arg.ToLower(), "false")) state = Toggle.False;
						else if (string.Equals(arg.ToLower(), "flip")) state = Toggle.Flip;
					}
					Tools.SetComponentEnabled(component, state);
				}
			}
			return true;
		}
		
		/// <summary>
		/// Handles the "SetPortrait(actorName, textureName)" action.
		/// 
		/// Arguments:
		/// -# The name of an actor in the dialogue database.
		/// -# The name of a texture that can be loaded from Resources, or 'default'.
		/// </summary>
		private bool HandleSetPortraitInternally(string commandName, string[] args) {
			string actorName = SequencerTools.GetParameter(args, 0);
			string textureName = SequencerTools.GetParameter(args, 1);
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: SetPortrait({1}, {2})", new System.Object[] { DialogueDebug.Prefix, actorName, textureName }));
			Actor actor = DialogueManager.MasterDatabase.GetActor(actorName);
			bool isDefault = string.Equals(textureName, "default");
			bool isPicTag = (textureName != null) && textureName.StartsWith("pic=");
			Texture2D texture = isDefault ? null 
				: (isPicTag ? actor.GetPortraitTexture(Tools.StringToInt(textureName.Substring("pic=".Length)))
				: DialogueManager.LoadAsset(textureName) as Texture2D);
			if (DialogueDebug.LogWarnings) {
				if (actor == null) Debug.LogWarning(string.Format("{0}: Sequencer: SetPortrait() command: actor '{1}' not found.", new System.Object[] { DialogueDebug.Prefix, actorName }));
				if ((texture == null) && !isDefault) Debug.LogWarning(string.Format("{0}: Sequencer: SetPortrait() command: texture '{1}' not found.", new System.Object[] { DialogueDebug.Prefix, textureName }));
			}
			if (actor != null) {
				if (isDefault) {
					DialogueLua.SetActorField(actorName, "Current Portrait", string.Empty);
				} else {
					if (texture != null) DialogueLua.SetActorField(actorName, "Current Portrait", textureName);
				}
				DialogueManager.Instance.SetActorPortraitTexture(actorName, texture);
			}
			return true;
		}

		/// <summary>
		/// Handles the "ShowAlert([duration])" action.
		/// 
		/// Arguments:
		/// -# (Optional) Duration.
		/// </summary>
		private bool HandleShowAlertInternally(string commandName, string[] args) {
			bool hasDuration = ((args.Length > 0) && !string.IsNullOrEmpty(args[0]));
			float duration = hasDuration ? SequencerTools.GetParameterAsFloat(args, 0) : 0;
			if (DialogueDebug.LogInfo) {
				if (hasDuration) {
					Debug.Log(string.Format("{0}: Sequencer: ShowAlert({1})", new System.Object[] { DialogueDebug.Prefix, duration }));
				} else {
					Debug.Log(string.Format("{0}: Sequencer: ShowAlert()", new System.Object[] { DialogueDebug.Prefix }));
				}
			}
			try {
				string message = Lua.Run("return Variable['Alert']").AsString;
				if (!string.IsNullOrEmpty(message)) {
					Lua.Run("Variable['Alert'] = ''");
					if (hasDuration) {
						DialogueManager.ShowAlert(message, duration);
					} else {
						DialogueManager.ShowAlert(message);
					}
				}
			} catch (Exception) {
			}
			return true;
		}
		
	}

}
