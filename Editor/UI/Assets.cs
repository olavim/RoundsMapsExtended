using UnityEngine;
using Jotunn.Utils;

namespace MapsExt.Editor.UI
{
	public static class Assets
	{
		private static readonly AssetBundle bundle;

		public static GameObject OpenDialogPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/file browser.prefab");
			}
		}

		public static GameObject SaveDialogPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/save dialog.prefab");
			}
		}

		public static GameObject KeyframeSettingsPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/keyframe settings.prefab");
			}
		}

		public static GameObject FoldoutPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/foldout.prefab");
			}
		}

		static Assets()
		{
			Assets.bundle = AssetUtils.LoadAssetBundleFromResources("uielements", typeof(Assets).Assembly);
		}
	}
}
