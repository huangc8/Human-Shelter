using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem {

	public enum EntrytagFormat {
		/// <summary>
		/// Format entrytag as "ActorName_ConversationID_EntryID".
		/// Special characters in ActorName will be replaced with underscores.
		/// Example: <c>Private_Hart_9_42</c>
		/// </summary>
		ActorName_ConversationID_EntryID,

		/// <summary>
		/// Format entrytag as "ConversationTitle_EntryID".
		/// Special characters in ConversationTitle will be replaced with underscores.
		/// Example: <c>Boardroom_Discussion_42</c>
		/// </summary>
		ConversationTitle_EntryID,

		/// <summary>
		/// Format entrytag Adventure Creator-style as "(ActorName)(LineNumber)".
		/// Special characters in ActorName will be replaced with underscores.
		/// The Dialogue System will attempt to assign a unique line number to 
		/// every entry using the formula ConversationID*500 + EntryID.
		/// Example: <c>Player42</c>
		/// </summary>
		ActorNameLineNumber
	}

}
