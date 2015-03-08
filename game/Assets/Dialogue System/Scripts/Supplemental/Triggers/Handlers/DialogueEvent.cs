namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// This bit mask enum defines the dialogue events.
	/// </summary>
	[System.Flags]
	public enum DialogueEvent {
		OnBark = 0x1,
		OnConversation = 0x2,
		OnSequence = 0x4
	}
	
}
