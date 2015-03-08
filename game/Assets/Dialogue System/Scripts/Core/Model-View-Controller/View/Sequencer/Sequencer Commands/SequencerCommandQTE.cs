using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands {
	
	/// <summary>
	/// Implements sequencer command: "QTE(index, duration, luaVariable, luaValue)", which presents
	/// a timed opportunity to perform a Quick Time Event.
	/// 
	/// Arguments:
	/// -# The index number of the QTE indicator. (QTE indicators are defined the dialogue UI.)
	/// -# The duration to display the QTE indicator.
	/// -# The Lua variable to set if the QTE is triggered.
	/// -# The value to set the variable to. If not trigger, the variable is set to a blank string.
	/// </summary>
	public class SequencerCommandQTE : SequencerCommand {
		
		private int index;
		private float stopTime;
		private string button;
		private string variableName;
		private string variableQTEValue;
		private FieldType variableType;
		
		public void Start() {
			index = GetParameterAsInt(0);
			DialogueManager.DialogueUI.ShowQTEIndicator(index);
			button = (index < DialogueManager.DisplaySettings.inputSettings.qteButtons.Length) 
				? DialogueManager.DisplaySettings.inputSettings.qteButtons[index]
				: null;
			float duration = GetParameterAsFloat(1);
			stopTime = DialogueTime.time + duration;
			variableName = GetParameter(2);
			variableQTEValue = GetParameter(3);
			variableType = GetVariableType();
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Sequencer: QTE({1}, {2}, {3}, {4})", new System.Object[] { DialogueDebug.Prefix, index, duration, variableName, variableQTEValue }));
			Lua.Run(string.Format("Variable[\"{0}\"] = \"\"", new System.Object[] { variableName }), DialogueDebug.LogInfo);
		}
		
		private FieldType GetVariableType() {
			float temp;
			if ((string.Compare(variableQTEValue, "true", System.StringComparison.OrdinalIgnoreCase) == 0) ||
				(string.Compare(variableQTEValue, "false", System.StringComparison.OrdinalIgnoreCase) == 0)) {
				return FieldType.Boolean;
			} else if (float.TryParse(variableQTEValue, out temp)) {
				return FieldType.Number;
			} else {
				return FieldType.Text;
			}
		}
		
		public void Update() {
			if (!string.IsNullOrEmpty(button) && Input.GetButtonDown(button)) {
				Lua.Run(string.Format("Variable[\"{0}\"] = {1}", new System.Object[] { variableName, DialogueLua.ValueAsString(variableType, variableQTEValue) }), DialogueDebug.LogInfo);
				DialogueManager.Instance.SendMessage("OnConversationContinue", SendMessageOptions.DontRequireReceiver);
				Stop();
			}
			else if (DialogueTime.time >= stopTime) {
				Stop();
			}
		}
		
		public void OnDestroy() {
			DialogueManager.DialogueUI.HideQTEIndicator(index);
		}
		
	}

}
