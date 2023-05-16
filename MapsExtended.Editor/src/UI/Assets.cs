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
		public static GameObject InspectorVector2InputPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/inspectorvector2input.prefab");
		public static GameObject InspectorSliderInputPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/inspectorsliderinput.prefab");
		public static GameObject InspectorToggleInputPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/inspectortoggleinput.prefab");
		public static GameObject InspectorButtonPrefab => s_bundle.LoadAsset<GameObject>("assets/_mapsextendededitor/inspectorbutton.prefab");

		static Assets()
		{
			s_bundle = AssetUtils.LoadAssetBundleFromResources("uielements", typeof(Assets).Assembly);
		}
	}
}
