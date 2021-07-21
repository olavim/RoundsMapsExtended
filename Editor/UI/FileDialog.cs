using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Jotunn.Utils;

namespace MapsExtended.UI
{
    public class FileDialog : MonoBehaviour
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

            fileBrowser.pathSelect.onValueChanged.AddListener(val =>
            {
                string newPath = val == 0 ? inactivePath : activePath;
                fileBrowser.SetPath(newPath);
            });

            fileBrowser.openButton.onClick.AddListener(() =>
            {
                if (fileBrowser.selectedPath != null)
                {
                    GameObject.Destroy(wrapperGo);
                    cb(fileBrowser.selectedPath);
                }
            });

            fileBrowser.closeButton.onClick.AddListener(() =>
            {
                GameObject.Destroy(wrapperGo);
            });
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

            saveDialog.title.text = "Save As...";

            saveDialog.saveButton.onClick.AddListener(() =>
            {
                if (saveDialog.textField.text?.Length > 0)
                {
                    GameObject.Destroy(wrapperGo);
                    cb(saveDialog.textField.text);
                }
            });

            saveDialog.closeButton.onClick.AddListener(() =>
            {
                GameObject.Destroy(wrapperGo);
            });
        }
    }
}
