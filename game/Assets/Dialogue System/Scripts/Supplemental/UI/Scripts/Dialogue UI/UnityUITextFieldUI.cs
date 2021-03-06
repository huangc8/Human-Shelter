using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// Unity UI text field UI implementation.
	/// </summary>
	[AddComponentMenu("Dialogue System/UI/Unity UI/Dialogue/Text Field UI")]
	public class UnityUITextFieldUI : MonoBehaviour, ITextFieldUI {

		/// <summary>
		/// The (optional) panel. If your text field UI contains more than a label and text field, you should
		/// assign the panel, too.
		/// </summary>
		public Graphic panel;
		
		/// <summary>
		/// The label that will contain any label text prompting the user what to enter.
		/// </summary>
		public Text label;
		
		/// <summary>
		/// The text field.
		/// </summary>
		public InputField textField;

		/// <summary>
		/// The accept key.
		/// </summary>
		public KeyCode acceptKey = KeyCode.Return;

		/// <summary>
		/// The cancel key.
		/// </summary>
		public KeyCode cancelKey = KeyCode.Escape;

		/// <summary>
		/// This delegate must be called when the player accepts the input in the text field.
		/// </summary>
		private AcceptedTextDelegate acceptedText = null;

		private bool isAwaitingInput = false;

		void Start() {
			if (DialogueDebug.LogWarnings && (textField == null)) Debug.LogWarning(string.Format("{0}: No InputField is assigned to the text field UI {1}. TextInput() sequencer commands or [var?=] won't work.", new object[] { DialogueDebug.Prefix, name }));
			Hide();
		}
		
		/// <summary>
		/// Starts the text input field.
		/// </summary>
		/// <param name="labelText">The label text.</param>
		/// <param name="text">The current value to use for the input field.</param>
		/// <param name="maxLength">Max length, or <c>0</c> for unlimited.</param>
		/// <param name="acceptedText">The delegate to call when accepting text.</param>
		public void StartTextInput(string labelText, string text, int maxLength, AcceptedTextDelegate acceptedText) {
			if (label != null) label.text = labelText;
			if (textField != null) {
				/*
				textField.text = text;
				textField.characterLimit = maxLength;
				*/
			}
			this.acceptedText = acceptedText;
			Show();
			isAwaitingInput = true;
		}

		public void Update() {
			if (isAwaitingInput) { 
				if (Input.GetKeyDown(acceptKey)) {
					AcceptTextInput();
				} else if (Input.GetKeyDown(cancelKey)) {
					CancelTextInput();
				}
			}
		}

		/// <summary>
		/// Cancels the text input field.
		/// </summary>
		public void CancelTextInput() {
			isAwaitingInput = false;
			Hide();
		}
		
		/// <summary>
		/// Accepts the text input and calls the accept handler delegate.
		/// </summary>
		public void AcceptTextInput() {
			isAwaitingInput = false;
			if (acceptedText != null) {
				//if (textField != null) acceptedText(textField.text);
				acceptedText = null;
			}
			Hide();
		}

		private void Show() {
			SetActive(true);
		}

		private void Hide() {
			SetActive(false);
		}

		private void SetActive(bool value) {
			if (textField != null) textField.enabled = value;
			if (panel != null) {
				Tools.SetGameObjectActive(panel, value);
			} else {
				Tools.SetGameObjectActive(label, value);
				Tools.SetGameObjectActive(textField, value);
			}
		}
		
	}

}
