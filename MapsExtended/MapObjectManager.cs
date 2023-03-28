using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using Jotunn.Utils;
using MapsExt.MapObjects;
using Photon.Pun;

namespace MapsExt
{
	public class MapObjectManager : MonoBehaviour
	{
		private static AssetBundle mapObjectBundle;
		private static readonly Dictionary<string, TargetSyncedStore<int>> syncStores = new Dictionary<string, TargetSyncedStore<int>>();

		public static TObj LoadCustomAsset<TObj>(string name) where TObj : UnityEngine.Object
		{
			return MapObjectManager.mapObjectBundle.LoadAsset<TObj>(name);
		}

		private readonly Dictionary<Type, IMapObjectSerializer> dataSerializers = new Dictionary<Type, IMapObjectSerializer>();
		private readonly Dictionary<Type, IMapObject> mapObjects = new Dictionary<Type, IMapObject>();

		private string NetworkID { get; set; }

		protected virtual void Awake()
		{
			if (MapObjectManager.mapObjectBundle == null)
			{
				MapObjectManager.mapObjectBundle = AssetUtils.LoadAssetBundleFromResources("mapobjects", typeof(MapObjectManager).Assembly);
			}
		}

		public void SetNetworkID(string id)
		{
			if (this.NetworkID != null)
			{
				MapObjectManager.syncStores.Remove(this.NetworkID);
			}

			this.NetworkID = id;
			MapObjectManager.syncStores.Add(id, new TargetSyncedStore<int>());
		}

		public void RegisterMapObject(Type dataType, IMapObject mapObject, IMapObjectSerializer serializer)
		{
			if (mapObject.Prefab == null)
			{
				throw new Exception($"Cannot register map object {dataType.Name}: Prefab cannot be null");
			}

			this.mapObjects.Add(dataType, mapObject);

			if (mapObject.Prefab.GetComponent<PhotonMapObject>())
			{
				PhotonNetwork.PrefabPool.RegisterPrefab(this.GetInstanceName(dataType), mapObject.Prefab);

				// PhotonMapObject has a hard-coded prefab name prefix / resource location
				PhotonNetwork.PrefabPool.RegisterPrefab("4 Map Objects/" + this.GetInstanceName(dataType), mapObject.Prefab);
			}

			this.dataSerializers.Add(dataType, serializer);
		}

		private string GetInstanceName(Type type)
		{
			return $"{this.NetworkID}/{type.FullName}";
		}

		public MapObjectData Serialize(GameObject go)
		{
			var mapObjectInstance = go.GetComponent<MapObjectInstance>();

			if (mapObjectInstance == null)
			{
				throw new ArgumentException($"Cannot serialize GameObject: missing MapObjectInstance");
			}

			return this.Serialize(mapObjectInstance);
		}

		public MapObjectData Serialize(MapObjectInstance mapObjectInstance)
		{
			if (mapObjectInstance == null)
			{
				throw new ArgumentException($"Cannot serialize null MapObjectInstance");
			}

			if (mapObjectInstance.dataType == null)
			{
				throw new ArgumentException($"Cannot serialize MapObjectInstance ({mapObjectInstance.gameObject.name}): missing dataType");
			}

			var serializer = this.dataSerializers.GetValueOrDefault(mapObjectInstance.dataType, null);

			if (serializer == null)
			{
				throw new ArgumentException($"Map object type not registered: {mapObjectInstance.dataType}");
			}

			return serializer.Serialize(mapObjectInstance);
		}

		public void Deserialize(MapObjectData data, GameObject target)
		{
			var serializer = this.dataSerializers.GetValueOrDefault(data.GetType(), null);

			if (serializer == null)
			{
				throw new ArgumentException($"Map object type not registered: {data.GetType()}");
			}

			serializer.Deserialize(data, target);
		}

		public void Instantiate(MapObjectData data, Transform parent, Action<GameObject> onInstantiate = null)
		{
			var mapObject = this.mapObjects[data.GetType()];
			var prefab = mapObject.Prefab;

			var syncStore = MapObjectManager.syncStores[this.NetworkID];
			int instantiationID = syncStore.Allocate(parent);

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
							this.Deserialize(data, networkInstance);

							mapObject.OnInstantiate(instance);

							onInstantiate?.Invoke(networkInstance);

							int viewID = networkInstance.GetComponent<PhotonView>().ViewID;

							// Communicate the photon instantiated map object to other clients
							NetworkingManager.RPC_Others(typeof(MapObjectManager), nameof(MapObjectManager.RPC_SyncInstantiation), this.NetworkID, instantiationID, viewID);
						});
					}
					else
					{
						// Call onInstantiate once the master client has communicated the photon instantiated map object
						this.StartCoroutine(this.SyncInstantiation(parent, instantiationID, data, onInstantiate));
					}
				}
			}
			else
			{
				/* We don't need to care about the photon instantiation dance (see above comment) when instantiating PhotonMapObjects
				 * after the map transition has already been done.
				 *
				 * The "lateInstantiated" flag is checked in a PhotonMapObject patch to initialize some required properties.
				 */
				instance = PhotonNetwork.Instantiate(this.GetInstanceName(data.GetType()), Vector3.zero, Quaternion.identity, 0, new object[] { "lateInstantiated" });
				instance.transform.SetParent(parent);
			}


			instance.name = this.GetInstanceName(data.GetType());

			this.Deserialize(data, instance);

			mapObject.OnInstantiate(instance);

			// The onInstantiate callback might be called twice: once for the "client-side" instance, and once for the networked instance
			onInstantiate?.Invoke(instance);
		}

		private IEnumerator SyncInstantiation(object target, int instantiationID, MapObjectData data, Action<GameObject> onInstantiate)
		{
			var syncStore = MapObjectManager.syncStores[this.NetworkID];
			yield return syncStore.WaitForValue(target, instantiationID);

			if (syncStore.TargetEquals(target))
			{
				int viewID = syncStore.Get(instantiationID);
				var networkInstance = PhotonNetwork.GetPhotonView(viewID).gameObject;

				this.Deserialize(data, networkInstance);

				onInstantiate?.Invoke(networkInstance);
			}
		}

		[UnboundRPC]
		public static void RPC_SyncInstantiation(string networkID, int instantiationID, int viewID)
		{
			MapObjectManager.syncStores[networkID].Set(instantiationID, viewID);
		}
	}
}
