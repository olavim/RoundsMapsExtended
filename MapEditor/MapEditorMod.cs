using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MapEditor
{
    [BepInDependency("com.willis.rounds.unbound", "2.1.4")]
    [BepInPlugin(ModId, "MapEditor", Version)]
    public class MapEditorMod : BaseUnityPlugin
    {
        private const string ModId = "io.olavim.rounds.mapeditor";
        public const string Version = "1.0.0";

        public static MapEditorMod instance;

        private int newMapID = -1;
        public Dictionary<string, GameObject> mapObjects = new Dictionary<string, GameObject>();

        public void Awake()
        {
            MapEditorMod.instance = this;

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                if (mode == LoadSceneMode.Single)
                {
                    this.newMapID = -1;
                }
            };

            AssetUtils.LoadAssetBundleFromResources("mapeditor", typeof(MapEditorMod).Assembly);
            var objectBundle = AssetUtils.LoadAssetBundleFromResources("mapobjects", typeof(MapEditorMod).Assembly);

            this.mapObjects.Add("Ground", objectBundle.LoadAsset<GameObject>("Ground"));
            this.mapObjects.Add("Box", Resources.Load<GameObject>("4 Map Objects/Box"));
            this.mapObjects.Add("Destructible Box", Resources.Load<GameObject>("4 Map Objects/Box_Destructible"));
            this.mapObjects.Add("Background Box", Resources.Load<GameObject>("4 Map Objects/Box_BG"));
            this.mapObjects.Add("Saw", Resources.Load<GameObject>("4 Map Objects/MapObject_Saw_Stat"));

            this.mapObjects["Saw"].AddComponent<Transformers.SawTransformer>();
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (this.newMapID == -1)
                {
                    this.InsertNewMap();
                }

                this.StartCoroutine(this.AddEditorOnLevelLoad());
                MapManager.instance.LoadLevelFromID(this.newMapID);
            }
        }

        private void InsertNewMap()
        {
            this.newMapID = MapManager.instance.levels.Length;
            var list = MapManager.instance.levels.ToList();
            list.Add("NewMap");
            MapManager.instance.levels = list.ToArray();
        }

        private IEnumerator AddEditorOnLevelLoad()
        {
            while (MapManager.instance.currentLevelID != this.newMapID)
            {
                yield return null;
            }

            var go = MapManager.instance.currentMap.Map.gameObject;
            go.transform.position = Vector3.zero;

            if (!go.GetComponent<MapEditor>())
            {
                go.AddComponent<MapEditor>();
            }
        }
    }
}
