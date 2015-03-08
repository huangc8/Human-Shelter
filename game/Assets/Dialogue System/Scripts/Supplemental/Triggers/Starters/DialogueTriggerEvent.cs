namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// A bit mask enum that defines the events that can trigger barks, conversations, and 
	/// sequences.
	/// </summary>
	[System.Flags]
	public enum DialogueTriggerEvent {
		
		/// <summary>
		/// Trigger when the GameObject receives an OnBarkEnd message
		/// </summary>
		OnBarkEnd = 0x1,
		
		/// <summary>
		/// Trigger when the GameObject receives an OnConversationEnd message
		/// </summary>
		OnConversationEnd = 0x2,
		
		/// <summary>
		/// Trigger when the GameObject receives an OnSequenceEnd message
		/// </summary>
		OnSequenceEnd = 0x4,
		
		/// <summary>
		/// Trigger when another collider enters this GameObject's trigger collider
		/// </summary>
		OnTriggerEnter = 0x8,
		
		/// <summary>
		/// Trigger when the GameObject starts (e.g., at the start of the level)
		/// </summary>
		OnStart = 0x10,
		
		/// <summary>
		/// Trigger when the GameObject receives an OnUse message (e.g., from the Selector component)
		/// </summary>
		OnUse = 0x20,
		
		/// <summary>
		/// Trigger when the trigger script is enabled (allows retriggering if you disable and 
		/// re-enable the script or deactivate and re-activate its GameObject.
		/// </summary>
		OnEnable = 0x40
	}
	
}
