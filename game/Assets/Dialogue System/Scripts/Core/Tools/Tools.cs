using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// A static class of general purpose functions used by the dialogue system.
	/// </summary>
	public static class Tools {

		public static bool IsPrefab(GameObject go) {
			if (go == null) return false;
			return (go != GameObjectHardFind(go.name));
			//--- Was:
			//return (go != null)
			//	? (GameObject.Find(go.name) == null)
			//	: false;
		}
		
		/// <summary>
		/// Utility function to convert a hex string to byte value.
		/// </summary>
		/// <returns>
		/// The byte value of the hex string.
		/// </returns>
		/// <param name='hex'>
		/// The hex string (e.g., "f0").
		/// </param>
		public static byte HexToByte(string hex) {
			return byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
		}
		
		/// <summary>
		/// Determines whether an object is a numeric type.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the object is a numeric type; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='o'>
		/// The object to check.
		/// </param>
		public static bool IsNumber(object o) {
			return (o is int) || (o is float) || (o is double);
		}
		
		/// <summary>
		/// Converts a string to an int.
		/// </summary>
		/// <returns>
		/// The int, or <c>0</c> if the string can't be parsed to an int.
		/// </returns>
		/// <param name='s'>
		/// The string.
		/// </param>
		public static int StringToInt(string s) {
			int result = 0;
			int.TryParse(s, out result);
			return result;
		}
		
		/// <summary>
		/// Converts a string to a float.
		/// </summary>
		/// <returns>
		/// The float, or <c>0</c> if the string can't be parsed to a float.
		/// </returns>
		/// <param name='s'>
		/// The string.
		/// </param>
		public static float StringToFloat(string s) {
			float result = 0;
			float.TryParse(s, out result);
			return result;
		}

		/// <summary>
		/// Converts a string to a bool.
		/// </summary>
		/// <returns>
		/// The bool, or <c>false</c> if the string can't be parsed to a bool.
		/// </returns>
		/// <param name='s'>
		/// The string.
		/// </param>
		public static bool StringToBool(string s) {
			return (string.Compare(s, "True", System.StringComparison.OrdinalIgnoreCase) == 0);
		}
		
		/// <summary>
		/// Determines if a string is null, empty or whitespace.
		/// </summary>
		/// <returns><c>true</c> if the string null, empty or whitespace; otherwise, <c>false</c>.</returns>
		/// <param name="s">The string to check.</param>
		public static bool IsStringNullOrEmptyOrWhitespace(string s) {
			return string.IsNullOrEmpty(s) || string.IsNullOrEmpty(s.Trim());
		}

		/// <summary>
		/// Gets the name of the object, or null if the object is null.
		/// </summary>
		/// <returns>
		/// The object name.
		/// </returns>
		/// <param name='o'>
		/// The object.
		/// </param>
		public static string GetObjectName(UnityEngine.Object o) {
			return (o != null) ? o.name : "null";
		}
		
		/// <summary>
		/// Gets the name of a component's GameObject.
		/// </summary>
		/// <returns>The game object name.</returns>
		/// <param name="c">A component</param>
		public static string GetGameObjectName(Component c) {
			return (c == null) ? string.Empty : c.name;
		}

		/// <summary>
		/// Gets the full name of a GameObject, following the hierarchy down from the root.
		/// </summary>
		/// <returns>The full name.</returns>
		/// <param name="go">A GameObject.</param>
		public static string GetFullName(GameObject go) {
			string fullName = string.Empty;
			if (go != null) {
				fullName = go.name;
				Transform t = go.transform.parent;
				while (t != null) {
					fullName = t.name + '.' + fullName;
					t = t.parent;
				}
			}
			return fullName;
		}
		
		/// <summary>
		/// Returns the first non-null argument. This function replaces C#'s null-coalescing
		/// operator (??), which doesn't work with component properties because, under the hood, 
		/// they're always non-null.
		/// </summary>
		/// <param name='args'>
		/// List of elements to select from.
		/// </param>
		public static Transform Select(params Transform[] args) {
			for (int i = 0; i < args.Length; i++) {
				if (args[i] != null) {
					return args[i];
				}
			}
			return null;
		}
	
		/// <summary>
		/// Returns the first non-null argument. This function replaces C#'s null-coalescing
		/// operator (??), which doesn't work with component properties because, under the hood, 
		/// they're always non-null.
		/// </summary>
		/// <param name='args'>
		/// List of elements to select from.
		/// </param>
		public static MonoBehaviour Select(params MonoBehaviour[] args) {
			for (int i = 0; i < args.Length; i++) {
				if (args[i] != null) {
					return args[i];
				}
			}
			return null;
		}

		/// <summary>
		/// Sends a message to all GameObjects in the scene.
		/// </summary>
		/// <param name="message">Message.</param>
		public static void SendMessageToEveryone(string message) {
			GameObject[] gameObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			foreach (GameObject go in gameObjects) {
				go.SendMessage(message, SendMessageOptions.DontRequireReceiver);
			}
		}

		/// <summary>
		/// Sets the component's game object active or inactive.
		/// </summary>
		/// <param name="component">Component.</param>
		/// <param name="value">The value to set.</param>
		public static void SetGameObjectActive(Component component, bool value) {
			if (component != null) component.gameObject.SetActive(value);
		}
		
		/// <summary>
		/// Checks if a float value is approximately zero (accounting for rounding error).
		/// </summary>
		/// <returns>
		/// <c>true</c> if the value is approximately zero.
		/// </returns>
		/// <param name='x'>
		/// The float to check.
		/// </param>
		public static bool ApproximatelyZero(float x) {
			return (x < 0.0001f);
		}
		
		/// <summary>
		/// Converts a web color string to a Color.
		/// </summary>
		/// <returns>
		/// The color.
		/// </returns>
		/// <param name='colorCode'>
		/// A web RGB-format color code of the format "\#rrggbb", where rr, gg, and bb are 
		/// hexadecimal values (e.g., \#ff0000 for red).
		/// </param>
		public static Color WebColor(string colorCode) {
			byte r = (colorCode.Length > 2) ? Tools.HexToByte(colorCode.Substring(1,2)) : (byte) 0;
			byte g = (colorCode.Length > 4) ? Tools.HexToByte(colorCode.Substring(3,2)) : (byte) 0;
			byte b = (colorCode.Length > 6) ? Tools.HexToByte(colorCode.Substring(5,2)) : (byte) 0;
			return new Color32(r, g, b, 255);
		}
		
		/// <summary>
		/// Converts a color of to a web color string.
		/// </summary>
		/// <returns>
		/// The web RGB-format color code of the format "\#rrggbb".
		/// </returns>
		/// <param name='color'>
		/// Color.
		/// </param>
		public static string ToWebColor(Color color) {
			return string.Format("#{0:x2}{1:x2}{2:x2}{3:x2}", (int) (255 * color.r), (int) (255 * color.g), (int) (255 * color.b), (int) (255 * color.a));
		}
		
		/// <summary>
		/// Determines whether an animation clip is in the animation list.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the clip is in the animation list.
		/// </returns>
		/// <param name='animation'>
		/// The legacy Animation component.
		/// </param>
		/// <param name='clipName'>
		/// The clip name.
		/// </param>
		public static bool IsClipInAnimations(Animation animation, string clipName) {
			if (animation != null) {
				foreach (AnimationState state in animation) {
					if (string.Equals(state.name, clipName)) return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Finds an in-scene GameObject even if it's inactive, as long as the inactive
		/// GameObject is a child of an active GameObject.
		/// </summary>
		/// <returns>The GameObject.</returns>
		/// <param name="str">Name of the GameObject.</param>
		/// <remarks>Code by cawas: http://answers.unity3d.com/questions/48252/how-to-find-inactive-gameobject.html</remarks>
		static public GameObject GameObjectHardFind (string str) {
			GameObject result = null;
			foreach ( GameObject root in GameObject.FindObjectsOfType(typeof(GameObject)) ) {
				if (root.transform.parent == null) { // means it's a root GO
					result = GameObjectHardFind(root, str, 0);
					if (result != null) break;
				}
			}
			return result;
		}
		static public GameObject GameObjectHardFind (string str, string tag) {
			GameObject result = null;
			foreach ( GameObject parent in GameObject.FindGameObjectsWithTag(tag) ) {
				result = GameObjectHardFind(parent, str, 0);
				if (result != null) break;
			}
			return result;
		}
		static private GameObject GameObjectHardFind (GameObject item, string str, int index) {
			if (index == 0 && item.name == str) return item;
			if (index < item.transform.childCount) {
				GameObject result = GameObjectHardFind(item.transform.GetChild(index).gameObject, str, 0);
				if (result == null) {
					return GameObjectHardFind(item, str, ++index);
				} else {
					return result;
				}
			}
			return null;
		}

		/// <summary>
		/// Like GetComponentInChildren(), but also searches parents.
		/// </summary>
		/// <returns>The component, or <c>null</c> if not found.</returns>
		/// <param name="gameObject">Game object to search.</param>
		/// <typeparam name="T">The component type.</typeparam>
		public static T GetComponentAnywhere<T>(GameObject gameObject) where T : Component {
			if (!gameObject) return null;
			T component = gameObject.GetComponentInChildren<T>();
			if (component) return component;
			Transform ancestor = gameObject.transform.parent;
			while (!component && ancestor) {
				component = ancestor.GetComponentInChildren<T>();
				ancestor = ancestor.parent;
			}
			return component;
		}

		/// <summary>
		/// Gets the height of the game object based on its collider. This only works if the
		/// game object has a CharacterController, CapsuleCollider, BoxCollider, or SphereCollider.
		/// </summary>
		/// <returns>The game object height if it has a recognized type of collider; otherwise <c>0</c>.</returns>
		/// <param name="gameObject">Game object.</param>
		public static float GetGameObjectHeight(GameObject gameObject) {
			CharacterController controller = gameObject.GetComponent<CharacterController>();
			if (controller != null) {
				return controller.height;
			} else {
				CapsuleCollider capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
				if (capsuleCollider != null) {
					return capsuleCollider.height;
				} else {
					BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
					if (boxCollider != null) {
						return boxCollider.center.y + boxCollider.size.y;
					} else {
						SphereCollider sphereCollider = gameObject.GetComponent<SphereCollider>();
						if (sphereCollider != null) {
							return sphereCollider.center.y + sphereCollider.radius;
						}
					}
				}
			}
			return 0;
		}

		/// <summary>
		/// Sets a component's enabled state to a specified state.
		/// </summary>
		/// <param name="component">Component to set.</param>
		/// <param name="state">State to set the component to (true, false, or flip).</param>
		public static void SetComponentEnabled(Component component, Toggle state) {
			bool newValue;
			if (component == null) return;
			if (component is Renderer) {
				Renderer targetRenderer = component as Renderer;
				newValue = ToggleTools.GetNewValue(targetRenderer.enabled, state);
				targetRenderer.enabled = newValue;
			} else if (component is Collider) {
				Collider targetCollider = component as Collider;
				newValue = ToggleTools.GetNewValue(targetCollider.enabled, state);
				targetCollider.enabled = newValue;
			} else if (component is Animation) {
				Animation animationComponent = component as Animation;
				newValue = ToggleTools.GetNewValue(animationComponent.enabled, state);
				animationComponent.enabled = newValue;
			} else if (component is Animator) {
				Animator animator = component as Animator;
				newValue = ToggleTools.GetNewValue(animator.enabled, state);
				animator.enabled = newValue;
			} else if (component is AudioSource) {
				AudioSource audioSource = component as AudioSource;
				newValue = ToggleTools.GetNewValue(audioSource.enabled, state);
				audioSource.enabled = newValue;
			} else if (component is Behaviour) {
				Behaviour behaviour = component as Behaviour;
				newValue = ToggleTools.GetNewValue(behaviour.enabled, state);
				behaviour.enabled = newValue;
			} else {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Don't know how to enable/disable {1}.{2}", new System.Object[] { DialogueDebug.Prefix, component.name, component.GetType().Name }));
				return;
			}
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: {1}.{2}.enabled = {3}", new System.Object[] { DialogueDebug.Prefix, component.name, component.GetType().Name, newValue }));
		}

		public static bool IsCursorActive() {
			return IsCursorVisible() && !IsCursorLocked();
		}

		public static void SetCursorActive(bool value) {
			ShowCursor(value);
			LockCursor(!value);
		}
		
		#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6

		public static bool IsCursorVisible() {
			return Screen.showCursor;
		}

		public static bool IsCursorLocked() {
			return Screen.lockCursor;
		}

		public static void ShowCursor(bool value) {
			Screen.showCursor = value;
		}

		public static void LockCursor(bool value) {
			Screen.lockCursor = value;
		}

		#else

		public static bool IsCursorVisible() {
			return Cursor.visible;
		}
		
		public static bool IsCursorLocked() {
			return Cursor.lockState != CursorLockMode.None;
		}

		private static CursorLockMode previousLockMode = CursorLockMode.Locked;
		
		public static void ShowCursor(bool value) {
			Cursor.visible = value;
		}
		
		public static void LockCursor(bool value) {
			if (value == false && IsCursorLocked()) {
				previousLockMode = Cursor.lockState;
			}
			Cursor.lockState = value ? previousLockMode : CursorLockMode.None;
		}
		
		#endif
		
		
	}
	
}
