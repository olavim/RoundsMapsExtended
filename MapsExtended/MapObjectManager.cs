using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Jotunn.Utils;
using MapsExtended.Transformers;

namespace MapsExtended
{
    public class MapObjectManager : MonoBehaviour
    {
        public static MapObjectManager instance;

        private readonly Dictionary<string, GameObject> mapObjects = new Dictionary<string, GameObject>();

        public void Awake()
        {
            MapObjectManager.instance = this;

            var objectBundle = AssetUtils.LoadAssetBundleFromResources("mapobjects", typeof(MapObjectManager).Assembly);
            this.RegisterMapObject<BoxTransformer>("Ground", objectBundle.LoadAsset<GameObject>("Ground"));
            this.RegisterMapObject<BoxTransformer>("Box", Resources.Load<GameObject>("4 Map Objects/Box"));
            this.RegisterMapObject<BoxTransformer>("Destructible Box", Resources.Load<GameObject>("4 Map Objects/Box_Destructible"));
            this.RegisterMapObject<BoxTransformer>("Background Box", Resources.Load<GameObject>("4 Map Objects/Box_BG"));
            this.RegisterMapObject<SawTransformer>("Saw", Resources.Load<GameObject>("4 Map Objects/MapObject_Saw_Stat"));
        }

        public void RegisterMapObject<T>(string name, GameObject prefab) where T : Component
        {
            this.mapObjects.Add(name, prefab);
            prefab.AddComponent<T>();

            var obj = prefab.AddComponent<MapObject>();
            obj.mapObjectName = name;
        }

        public GameObject GetMapObject(string name)
        {
            return this.mapObjects[name];
        }

        public string[] GetMapObjects()
        {
            return this.mapObjects.Keys.ToArray();
        }
    }
}
