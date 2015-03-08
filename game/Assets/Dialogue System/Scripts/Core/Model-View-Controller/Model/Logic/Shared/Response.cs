using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// Info about a response, which is a link from one dialogue entry to another. Responses are 
	/// created by the ConversationModel and passed by the ConversationView to a dialogue UI to be
	/// displayed in a response menu (for player responses) or spoken by an NPC (for NPC 
	/// responses). When the player selects a response, IDialogueUI calls back with a
	/// SelectedResponseEventArgs record containing the selected response.
	/// </summary>
	public class Response {
		
		/// <summary>
		/// The formatted text of the response.
		/// </summary>
		public FormattedText formattedText;
		
		/// <summary>
		/// The dialogue entry that this response links to. In other words, if this response is
		/// selected, the dialogue system will go to this dialogue entry.
		/// </summary>
		public DialogueEntry destinationEntry;
	
		/// <summary>
		/// Initializes a new Response.
		/// </summary>
		/// <param name='formattedText'>
		/// Formatted text.
		/// </param>
		/// <param name='destinationEntry'>
		/// Destination entry.
		/// </param>
		public Response(FormattedText formattedText, DialogueEntry destinationEntry) {
			this.formattedText = formattedText;
			this.destinationEntry = destinationEntry;
		}
		
	}

}
