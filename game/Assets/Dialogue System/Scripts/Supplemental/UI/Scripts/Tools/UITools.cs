using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	public static class UITools {

		public static int GetAnimatorNameHash(AnimatorStateInfo animatorStateInfo) {
			#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			return animatorStateInfo.nameHash;
			#else
			return animatorStateInfo.fullPathHash;
			#endif
		}

	}

}
