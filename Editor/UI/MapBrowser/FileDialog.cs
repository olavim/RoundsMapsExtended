using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Jotunn.Utils;

namespace MapsExtended.UI.MapBrowser
{
    public class FileDialog : MonoBehaviour
    {
        private static GameObject fileBrowserPrefab;

        public static void Open(Action<string> cb)
        {
            if (!FileDialog.fileBrowserPrefab)
            {
                var bundle = AssetUtils.LoadAssetBundleFromResources("uielements", typeof(FileDialog).Assembly);
                FileDialog.fileBrowserPrefab = bundle.LoadAsset<GameObject>("assets/prefabs/file browser.prefab");
            }

            var wrapperGo = new GameObject("File Dialog");
            DontDestroyOnLoad(wrapperGo);

            var canvas = wrapperGo.AddComponent<Canvas>();
            var bg = wrapperGo.AddComponent<Image>();
            wrapperGo.AddComponent<GraphicRaycaster>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            bg.color = new Color32(0, 0, 0, 200);

            var fileBrowserGo = GameObject.Instantiate(FileDialog.fileBrowserPrefab, wrapperGo.transform);
            var fileBrowser = fileBrowserGo.GetComponent<FileBrowser>();

            string inactivePath = Path.Combine(BepInEx.Paths.GameRootPath, "maps");
            string activePath = BepInEx.Paths.PluginPath;

            fileBrowser.SetOptions("Inactive Maps", "Active Maps");
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
    }
}
