using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Jotunn.Utils;

namespace MapsExtended.UI
{
    public class FileDialog : MonoBehaviour
    {
        private static readonly AssetBundle bundle;
        private static GameObject openDialogPrefab;
        private static GameObject saveDialogPrefab;

        static FileDialog()
        {
            FileDialog.bundle = AssetUtils.LoadAssetBundleFromResources("uielements", typeof(FileDialog).Assembly);
        }

        public static void OpenDialog(Action<string> cb)
        {
            if (!FileDialog.openDialogPrefab)
            {
                FileDialog.openDialogPrefab = FileDialog.bundle.LoadAsset<GameObject>("assets/prefabs/file browser.prefab");
            }

            var wrapperGo = new GameObject("Open Dialog");
            DontDestroyOnLoad(wrapperGo);

            var canvas = wrapperGo.AddComponent<Canvas>();
            var bg = wrapperGo.AddComponent<Image>();
            wrapperGo.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 500;

            bg.color = new Color32(0, 0, 0, 200);

            var fileBrowserGo = GameObject.Instantiate(FileDialog.openDialogPrefab, wrapperGo.transform);
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
            if (!FileDialog.saveDialogPrefab)
            {
                FileDialog.saveDialogPrefab = FileDialog.bundle.LoadAsset<GameObject>("assets/prefabs/save dialog.prefab");
            }

            var wrapperGo = new GameObject("Save Dialog");
            DontDestroyOnLoad(wrapperGo);

            var canvas = wrapperGo.AddComponent<Canvas>();
            var bg = wrapperGo.AddComponent<Image>();
            wrapperGo.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            bg.color = new Color32(0, 0, 0, 200);

            var saveDialogGo = GameObject.Instantiate(FileDialog.saveDialogPrefab, wrapperGo.transform);
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
