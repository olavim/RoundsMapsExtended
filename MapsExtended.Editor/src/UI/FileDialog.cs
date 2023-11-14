using MapsExt.Utils;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public sealed class FileDialog : MonoBehaviour
	{
		public static void OpenDialog(Action<string> cb)
		{
			var wrapperGo = new GameObject("Open Dialog");
			DontDestroyOnLoad(wrapperGo);

			var canvas = wrapperGo.AddComponent<Canvas>();
			var bg = wrapperGo.AddComponent<Image>();
			wrapperGo.AddComponent<GraphicRaycaster>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.overrideSorting = true;
			canvas.sortingOrder = 500;

			bg.color = new Color32(0, 0, 0, 200);

			var fileBrowserGo = GameObject.Instantiate(Assets.OpenDialogPrefab, wrapperGo.transform);
			var fileBrowser = fileBrowserGo.GetComponent<FileBrowser>();

			string inactivePath = Path.Combine(BepInEx.Paths.GameRootPath, "maps");
			string activePath = BepInEx.Paths.PluginPath;

			fileBrowser.SetOptions("Personal Maps", "Plugin Maps");
			fileBrowser.SetPath(inactivePath);

			fileBrowser.PathSelect.onValueChanged.AddListener(val =>
			{
				string newPath = val == 0 ? inactivePath : activePath;
				fileBrowser.SetPath(newPath);
			});

			fileBrowser.OpenButton.onClick.AddListener(() =>
			{
				if (fileBrowser.SelectedPath != null)
				{
					MapsExt.Utils.GameObjectUtils.DestroyImmediateSafe(wrapperGo);
					cb(fileBrowser.SelectedPath);
				}
			});

			fileBrowser.CloseButton.onClick.AddListener(() => MapsExt.Utils.GameObjectUtils.DestroyImmediateSafe(wrapperGo));
		}

		public static void SaveDialog(Action<string> cb)
		{
			var wrapperGo = new GameObject("Save Dialog");
			DontDestroyOnLoad(wrapperGo);

			var canvas = wrapperGo.AddComponent<Canvas>();
			var bg = wrapperGo.AddComponent<Image>();
			wrapperGo.AddComponent<GraphicRaycaster>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;

			bg.color = new Color32(0, 0, 0, 200);

			var saveDialogGo = GameObject.Instantiate(Assets.SaveDialogPrefab, wrapperGo.transform);
			var saveDialog = saveDialogGo.GetComponent<SaveDialog>();

			saveDialog.Title.text = "Save As...";

			saveDialog.SaveButton.onClick.AddListener(() =>
			{
				if (saveDialog.TextField.text?.Length > 0)
				{
					MapsExt.Utils.GameObjectUtils.DestroyImmediateSafe(wrapperGo);
					cb(saveDialog.TextField.text);
				}
			});

			saveDialog.CloseButton.onClick.AddListener(() => MapsExt.Utils.GameObjectUtils.DestroyImmediateSafe(wrapperGo));
		}
	}
}
