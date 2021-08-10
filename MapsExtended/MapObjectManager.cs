using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using Jotunn.Utils;
using MapsExtended.Transformers;
using Photon.Pun;

namespace MapsExtended
{
    public class MapObjectManager : NetworkedBehaviour
    {
        public static MapObjectManager instance;

        private readonly string prefabPrefix = "MapsExtended:";
        private readonly Dictionary<string, GameObject> mapObjects = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, List<Tuple<Type, Action<object>>>> mapObjectComponents = new Dictionary<string, List<Tuple<Type, Action<object>>>>();
        private readonly TargetSyncedStore<int> syncedInstantiations = new TargetSyncedStore<int>();

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
            mapObjectName = mapObjectName.Replace(" ", "_");

            this.mapObjects.Add(mapObjectName, prefab);
            this.RegisterMapObjectComponent<MapObject>(mapObjectName, c => c.mapObjectName = mapObjectName);

            if (prefab.GetComponent<PhotonMapObject>())
            {
                PhotonNetwork.PrefabPool.RegisterPrefab(prefabPrefix + mapObjectName, prefab);

                // PhotonMapObject has a hard-coded prefab name prefix / resource location
                PhotonNetwork.PrefabPool.RegisterPrefab("4 Map Objects/" + prefabPrefix + mapObjectName, prefab);
            }
        }

        public void RegisterMapObjectComponent<T>(string mapObjectName, Action<T> onInstantiate = null) where T : Component
        {
            mapObjectName = mapObjectName.Replace(" ", "_");

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
            mapObjectName = mapObjectName.Replace(" ", "_");
            return this.mapObjects[mapObjectName];
        }

        private void AddMapObjectComponents(string mapObjectName, GameObject instance)
        {
            mapObjectName = mapObjectName.Replace(" ", "_");

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

        public void Instantiate(string mapObjectName, Transform parent, Action<GameObject> onInstantiate)
        {
            mapObjectName = mapObjectName.Replace(" ", "_");
            int instantiationID = this.syncedInstantiations.Allocate(parent);

            var prefab = this.GetMapObject(mapObjectName);
            bool isMapObjectNetworked = prefab.GetComponent<PhotonMapObject>() != null;
            bool isMapSpawned = MapManager.instance.currentMap?.Map.wasSpawned == true;

            GameObject instance;

            if (!isMapSpawned || !isMapObjectNetworked)
            {
                instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);

                if (isMapObjectNetworked)
                {
                    /* PhotonMapObjects (networked map objects like movable boxes) are first instantiated client-side, which is what we
                     * see during the map transition animation. After the transition is done, the client-side instance is removed and a
                     * networked (photon instantiated) version is spawned in its place. This means we need to do weird shit to get the
                     * "actual" map object instance, since the instance we get from `GameObject.Instantiate` above is in this case not
                     * the instance we care about.
                     * 
                     * Communicating the photon instantiated map object to other clients is an additional hurdle. The solution here uses
                     * a dynamically generated "instantiation ID", which is basically how many times this `Instantiate` method has been
                     * called for the provided `parent` (map) in succession. The master client later communicates the view ID of the
                     * instantiated map object to other clients with the instantiation ID. Clients can then find the instantiated
                     * map object with the view ID and reply to the caller of this `Instantiate` with the onInstantiate callback.
                     */
                    if (PhotonNetwork.IsMasterClient)
                    {
                        MapsExtended.instance.OnPhotonMapObjectInstantiate(instance.GetComponent<PhotonMapObject>(), networkInstance =>
                        {
                            this.AddMapObjectComponents(mapObjectName, networkInstance);
                            onInstantiate(networkInstance);

                            int viewID = networkInstance.GetComponent<PhotonView>().ViewID;

                            // Communicate the photon instantiated map object to other clients
                            this.photonView.RPC("RPC_SyncInstantiation", RpcTarget.Others, instantiationID, viewID);
                        });
                    }
                    else
                    {
                        // Call onInstantiate once the master client has communicated the photon instantiated map object
                        this.StartCoroutine(this.SyncInstantiation(parent, instantiationID, mapObjectName, onInstantiate));
                    }
                }
            }
            else
            {
                /* We don't need to care about the photon instantiation dance (see above comment) when instantiating PhotonMapObjects
                 * after the map transition has already been done.
                 */
                instance = PhotonNetwork.Instantiate(prefabPrefix + mapObjectName, Vector3.zero, Quaternion.identity);

                MapsExtended.instance.ExecuteAfterFrames(1, () =>
                {
                    instance.transform.SetParent(parent);
                });
            }


            instance.name = prefabPrefix + mapObjectName;

            // The onInstantiate callback might be called twice: once for the "client-side" instance, and once for the networked instance
            this.AddMapObjectComponents(mapObjectName, instance);
            onInstantiate(instance);
        }

        private IEnumerator SyncInstantiation(object target, int instantiationID, string mapObjectName, Action<GameObject> onInstantiate)
        {
            yield return this.syncedInstantiations.WaitForValue(target, instantiationID);

            if (this.syncedInstantiations.TargetEquals(target))
            {
                int viewID = this.syncedInstantiations.Get(instantiationID);
                var networkInstance = PhotonNetwork.GetPhotonView(viewID).gameObject;

                this.AddMapObjectComponents(mapObjectName, networkInstance);
                onInstantiate(networkInstance);
            }
        }

        [PunRPC]
        public void RPC_SyncInstantiation(int instantiationID, int viewID)
        {
            this.syncedInstantiations.Set(instantiationID, viewID);
        }
    }
}
