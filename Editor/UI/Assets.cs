using UnityEngine;
using Jotunn.Utils;

namespace MapsExt.Editor.UI
{
	public static class Assets
	{
		private static readonly AssetBundle bundle;

		public static GameObject OpenDialogPrefab => Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/file browser.prefab");
		public static GameObject SaveDialogPrefab => Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/save dialog.prefab");
		public static GameObject KeyframeSettingsPrefab => Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/keyframe settings.prefab");
		public static GameObject FoldoutPrefab => Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/foldout.prefab");
		public static GameObject InspectorDividerPrefab => Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/inspectordivider.prefab");
		public static GameObject InspectorVector2Prefab => Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/inspectorvector2.prefab");
		public static GameObject InspectorQuaternionPrefab => Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/inspectorquaternion.prefab");
		public static GameObject InspectorBooleanPrefab => Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/inspectorboolean.prefab");
		public static GameObject InspectorButtonPrefab => Assets.bundle.LoadAsset<GameObject>("assets/mapsextendededitor/inspectorbutton.prefab");

		static Assets()
		{
			Assets.bundle = AssetUtils.LoadAssetBundleFromResources("uielements", typeof(Assets).Assembly);
		}
	}
}
