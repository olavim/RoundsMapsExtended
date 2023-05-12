using UnityEngine;
using Jotunn.Utils;

namespace MapsExt.Editor.UI
{
	public static class Assets
	{
		private static readonly AssetBundle s_bundle;

		public static GameObject OpenDialogPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/file browser.prefab");
		public static GameObject SaveDialogPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/save dialog.prefab");
		public static GameObject KeyframeSettingsPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/keyframe settings.prefab");
		public static GameObject FoldoutPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/foldout.prefab");
		public static GameObject InspectorDividerPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/inspectordivider.prefab");
		public static GameObject InspectorVector2Prefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/inspectorvector2.prefab");
		public static GameObject InspectorQuaternionPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/inspectorquaternion.prefab");
		public static GameObject InspectorBooleanPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/inspectorboolean.prefab");
		public static GameObject InspectorButtonPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/inspectorbutton.prefab");

		static Assets()
		{
			s_bundle = AssetUtils.LoadAssetBundleFromResources("uielements", typeof(Assets).Assembly);
		}
	}
}
