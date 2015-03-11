using UnityEngine;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This holds a quest title. It's automatically added to Track and Abandon
	/// buttons to allow them to carry data about which quest they apply to.
	/// </summary>
	public class UnityUIQuestTitle : MonoBehaviour {

		public string questTitle;

	}

}