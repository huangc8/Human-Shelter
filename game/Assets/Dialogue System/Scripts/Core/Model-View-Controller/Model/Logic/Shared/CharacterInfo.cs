using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Contains information about a conversation participant that the dialogue UI or Sequencer may
	/// need.
	/// </summary>
	public class CharacterInfo {
		
		/// <summary>
		/// The actor ID of the character.
		/// </summary>
		public int id;
		
		/// <summary>
		/// The name of the actor as defined in the dialogue database.
		/// </summary>
		public string nameInDatabase;
		
		/// <summary>
		/// The type of the character (PC or NPC).
		/// </summary>
		public CharacterType characterType;
		
		/// <summary>
		/// Indicates whether this character is a player (PC).
		/// </summary>
		/// <value>
		/// <c>true</c> if this is a player; otherwise, <c>false</c> for an NPC.
		/// </value>
		public bool IsPlayer { get { return characterType == CharacterType.PC; } }
		
		/// <summary>
		/// Indicates whether this character is an NPC.
		/// </summary>
		/// <value>
		/// <c>true</c> if this is an NPC; otherwise, <c>false</c> for a player.
		/// </value>
		public bool IsNPC { get { return characterType == CharacterType.NPC; } }
		
		/// <summary>
		/// The transform of the character's GameObject.
		/// </summary>
		public Transform transform;
		
		/// <summary>
		/// The portrait texture of the character.
		/// </summary>
		public Texture2D portrait;
		
		/// <summary>
		/// Gets the character's name.
		/// </summary>
		/// <value>
		/// If the character info has been provided a non-null transform, this property returns the
		/// name of the transform's game object. Otherwise it returns the name of the actor in the
		/// dialogue database.
		/// </value>
		public string Name { get; private set; }
		
		/// <summary>
		/// Initializes a new CharacterInfo.
		/// </summary>
		/// <param name='actorID'>
		/// Actor ID.
		/// </param>
		/// <param name='actorName'>
		/// Name of the actor as defined in the dialogue database.
		/// </param>
		/// <param name='transform'>
		/// Transform.
		/// </param>
		/// <param name='characterType'>
		/// Character type.
		/// </param>
		/// <param name='portrait'>
		/// Portrait.
		/// </param>
		public CharacterInfo(int id, string nameInDatabase, Transform transform, CharacterType characterType, Texture2D portrait) {
			this.id = id;
			this.nameInDatabase = nameInDatabase;
			this.characterType = characterType;
			this.portrait = portrait;
			this.transform = transform;
			if ((transform == null) && !string.IsNullOrEmpty(nameInDatabase)) {
				GameObject go = SequencerTools.FindSpecifier(nameInDatabase);
				if (go != null) this.transform = go.transform;
			}
			Name = (transform == null) ? nameInDatabase : OverrideActorName.GetActorName(transform);
		}

		/// <summary>
		/// Gets the pic override portrait. Dialogue text can include <c>[pic=#]</c> tags.
		/// This number corresponds to the actor's portrait (if picNum == 1) or 
		/// alternatePortraits (if picNum >= 2).
		/// </summary>
		/// <returns>The pic override portrait.</returns>
		/// <param name="picNum">Pic number.</param>
		public Texture2D GetPicOverride(int picNum) {
			if (picNum < 2) return portrait;
			int alternatePortraitIndex = picNum - 2;
			Actor actor = DialogueManager.MasterDatabase.GetActor(id);
			return ((actor != null) && (alternatePortraitIndex < actor.alternatePortraits.Count))
				? actor.alternatePortraits[alternatePortraitIndex]
				: portrait;
		}
		
	}

}
