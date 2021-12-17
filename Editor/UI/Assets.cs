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
				return Assets.bundle.LoadAsset<GameObject>("assets/ui/file browser.prefab");
			}
		}

		public static GameObject SaveDialogPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/ui/save dialog.prefab");
			}
		}

		public static GameObject ToolbarPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/ui/toolbar.prefab");
			}
		}

		public static GameObject WindowPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/ui/window.prefab");
			}
		}

		public static GameObject AnimationWindowPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/ui/animation window.prefab");
			}
		}

		public static GameObject KeyframeSettingsPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/ui/keyframe settings.prefab");
			}
		}

		public static GameObject FoldoutPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/ui/foldout.prefab");
			}
		}

		public static GameObject MapObjectInspectorPrefab
		{
			get
			{
				return Assets.bundle.LoadAsset<GameObject>("assets/ui/inspector.prefab");
			}
		}

		static Assets()
		{
			Assets.bundle = AssetUtils.LoadAssetBundleFromResources("uielements", typeof(FileDialog).Assembly);
		}
	}
}
