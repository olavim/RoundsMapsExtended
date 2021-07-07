using System;
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
        private readonly Dictionary<string, List<Tuple<Type, Action<object>>>> mapObjectComponents = new Dictionary<string, List<Tuple<Type, Action<object>>>>();

        public void Awake()
        {
            MapObjectManager.instance = this;

            var objectBundle = AssetUtils.LoadAssetBundleFromResources("mapobjects", typeof(MapObjectManager).Assembly);
            this.RegisterMapObject("Ground", objectBundle.LoadAsset<GameObject>("Ground"));
            this.RegisterMapObject("Box", Resources.Load<GameObject>("4 Map Objects/Box"));
            this.RegisterMapObject("Destructible Box", Resources.Load<GameObject>("4 Map Objects/Box_Destructible"));
            this.RegisterMapObject("Background Box", Resources.Load<GameObject>("4 Map Objects/Box_BG"));
            this.RegisterMapObject("Saw", Resources.Load<GameObject>("4 Map Objects/MapObject_Saw_Stat"));

            this.RegisterMapObjectComponent<SawTransformer>("Saw");
        }

        public void RegisterMapObject(string mapObjectName, GameObject prefab)
        {
            this.mapObjects.Add(mapObjectName, prefab);
            this.RegisterMapObjectComponent<MapObject>(mapObjectName, c => c.mapObjectName = mapObjectName);
        }

        public void RegisterMapObjectComponent<T>(string mapObjectName, Action<T> onInstantiate = null) where T : Component
        {
            if (!this.mapObjectComponents.ContainsKey(mapObjectName))
            {
                this.mapObjectComponents.Add(mapObjectName, new List<Tuple<Type, Action<object>>>());
            }

            Action<object> middleware = obj => onInstantiate?.Invoke((T) obj);
            var tuple = new Tuple<Type, Action<object>>(typeof(T), middleware);
            this.mapObjectComponents[mapObjectName].Add(tuple);
        }

        public GameObject GetMapObject(string mapObjectName)
        {
            return this.mapObjects[mapObjectName];
        }

        public void AddMapObjectComponents(string mapObjectName, GameObject instance)
        {
            if (this.mapObjectComponents.ContainsKey(mapObjectName))
            {
                instance.SetActive(false);
                foreach (var tuple in this.mapObjectComponents[mapObjectName])
                {
                    var c = instance.AddComponent(tuple.Item1);
                    var action = tuple.Item2;
                    action.Invoke(c);
                }
                instance.SetActive(true);
            }
        }

        public string[] GetMapObjects()
        {
            return this.mapObjects.Keys.ToArray();
        }
    }
}
