using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// The current state of a conversation, which can also be thought of as the current position 
	/// in the dialogue tree.
	/// </summary>
	public class ConversationState {
		
		/// <summary>
		/// The subtitle of the current dialogue entry.
		/// </summary>
		public Subtitle subtitle;
		
		/// <summary>
		/// The NPC responses linked from the current dialogue entry. This array may be empty.
		/// Typically, a conversation state will have either NPC responses or PC responses but not
		/// both.
		/// </summary>
		public Response[] npcResponses;
		
		/// <summary>
		/// The PC responses linked from the current dialogue entry. This array may be empty.
		/// Typically, a conversation state will have either NPC responses or PC responses but not
		/// both.
		/// </summary>
		public Response[] pcResponses;

		/// <summary>
		/// Indicates whether the current state has any NPC responses.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has any NPC responses; otherwise, <c>false</c>.
		/// </value>
		public bool HasNPCResponse { 
			get { return (npcResponses != null) && (npcResponses.Length > 0); }
		}
		
		/// <summary>
		/// Gets the first NPC response.
		/// </summary>
		/// <value>
		/// The first NPC response.
		/// </value>
		public Response FirstNPCResponse {
			get { return HasNPCResponse ? npcResponses[0] : null; }
		}
		
		/// <summary>
		/// Indicates whether the current state has any PC responses.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has PC responses; otherwise, <c>false</c>.
		/// </value>
		public bool HasPCResponses { 
			get { return (pcResponses != null) && (pcResponses.Length > 0); }
		}
		
		/// <summary>
		/// Indicates whether the current state has a PC auto-response, which means it has only a
		/// single response and that response does not specify "force menu."
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance has PC auto response; otherwise, <c>false</c>.
		/// </value>
		public bool HasPCAutoResponse {
			get { return (pcResponses != null) && (pcResponses.Length == 1) && !pcResponses[0].formattedText.forceMenu; }
		}
		
		/// <summary>
		/// Gets the PC auto response.
		/// </summary>
		/// <value>
		/// The PC auto response, or null if the state doesn't have one.
		/// </value>
		public Response PCAutoResponse {
			get { return HasPCAutoResponse ? pcResponses[0] : null; }
		}

		/// <summary>
		/// Indicates whether this state has any responses (PC or NPC).
		/// </summary>
		/// <value><c>true</c> if this instance has any responses; otherwise, <c>false</c>.</value>
		public bool HasAnyResponses {
			get { return HasNPCResponse || HasPCResponses; }
		}
		
		/// <summary>
		/// Indicates whether this state corresponds to a group dialogue entry.
		/// </summary>
		/// <value>
		/// <c>true</c> if a group; otherwise, <c>false</c>.
		/// </value>
		public bool IsGroup { get; set; }
		
		/// <summary>
		/// Initializes a new ConversationState.
		/// </summary>
		/// <param name='subtitle'>
		/// Subtitle of the current dialogue entry.
		/// </param>
		/// <param name='npcResponses'>
		/// NPC responses.
		/// </param>
		/// <param name='pcResponses'>
		/// PC responses.
		/// </param>
		public ConversationState(Subtitle subtitle, Response[] npcResponses, Response[] pcResponses, bool isGroup = false) {
			this.subtitle = subtitle;
			this.npcResponses = npcResponses;
			this.pcResponses = pcResponses;
			this.IsGroup = isGroup;
		}
		
	}

}
