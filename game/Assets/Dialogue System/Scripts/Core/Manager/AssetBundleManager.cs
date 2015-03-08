using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This class manages a list of asset bundles. It's used by DialogueSystemController.
	/// </summary>
	public class AssetBundleManager {

		private HashSet<AssetBundle> bundles = new HashSet<AssetBundle>();

		public void RegisterAssetBundle(AssetBundle bundle) {
			if (bundle == null) return;
			bundles.Add(bundle);
		}

		public void UnregisterAssetBundle(AssetBundle bundle) {
			if (bundle == null) return;
			bundles.Remove(bundle);
		}

		public UnityEngine.Object Load(string name) {
			foreach (var bundle in bundles) {
				if (bundle.Contains(name)) {
					return LoadFromBundle(bundle, name);
				}
			}
			return Resources.Load(name);
		}

		private UnityEngine.Object LoadFromBundle(AssetBundle bundle, string name) {
			#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			return bundle.Load(name);
			#else
			return bundle.LoadAsset(name);
			#endif
		}

	}

}
