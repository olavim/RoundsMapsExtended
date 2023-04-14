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
		private static AssetBundle s_mapObjectBundle;
		private static readonly Dictionary<string, TargetSyncedStore<int>> s_syncStores = new();

		public static TObj LoadCustomAsset<TObj>(string name) where TObj : UnityEngine.Object
		{
			return s_mapObjectBundle.LoadAsset<TObj>(name);
		}

		private readonly Dictionary<Type, IMapObjectSerializer> _dataSerializers = new();
		private readonly Dictionary<Type, IMapObject> _mapObjects = new();

		private string _networkID;

		protected virtual void Awake()
		{
			if (s_mapObjectBundle == null)
			{
				s_mapObjectBundle = AssetUtils.LoadAssetBundleFromResources("mapobjects", typeof(MapObjectManager).Assembly);
			}
		}

		public void SetNetworkID(string id)
		{
			if (this._networkID != null)
			{
				s_syncStores.Remove(this._networkID);
			}

			this._networkID = id;
			s_syncStores.Add(id, new TargetSyncedStore<int>());
		}

		public void RegisterMapObject(Type dataType, IMapObject mapObject, IMapObjectSerializer serializer)
		{
			if (mapObject.Prefab == null)
			{
				throw new ArgumentException("Prefab cannot be null");
			}

			if (this._mapObjects.ContainsKey(dataType))
			{
				throw new ArgumentException($"{dataType.Name} is already registered");
			}

			this._mapObjects[dataType] = mapObject;

			if (mapObject.Prefab.GetComponent<PhotonMapObject>())
			{
				PhotonNetwork.PrefabPool.RegisterPrefab(this.GetInstanceName(dataType), mapObject.Prefab);

				// PhotonMapObject has a hard-coded prefab name prefix / resource location
				PhotonNetwork.PrefabPool.RegisterPrefab("4 Map Objects/" + this.GetInstanceName(dataType), mapObject.Prefab);
			}

			this._dataSerializers.Add(dataType, serializer);
		}

		private string GetInstanceName(Type type)
		{
			return $"{this._networkID}/{type.FullName}";
		}

		public MapObjectData Serialize(GameObject go)
		{
			var mapObjectInstance =
				go.GetComponent<MapObjectInstance>() ??
				throw new ArgumentException("Cannot serialize GameObject: missing MapObjectInstance");
			return this.Serialize(mapObjectInstance);
		}

		public MapObjectData Serialize(MapObjectInstance mapObjectInstance)
		{
			if (mapObjectInstance == null)
			{
				throw new ArgumentException("Cannot serialize null MapObjectInstance");
			}

			if (mapObjectInstance.DataType == null)
			{
				throw new ArgumentException($"Cannot serialize MapObjectInstance ({mapObjectInstance.gameObject.name}): missing dataType");
			}

			var serializer =
				this._dataSerializers.GetValueOrDefault(mapObjectInstance.DataType, null) ??
				throw new ArgumentException($"Map object type not registered: {mapObjectInstance.DataType}");
			return serializer.Serialize(mapObjectInstance);
		}

		public void Deserialize(MapObjectData data, GameObject target)
		{
			var serializer =
				this._dataSerializers.GetValueOrDefault(data.GetType(), null) ??
				throw new ArgumentException($"Map object type not registered: {data.GetType()}");
			serializer.Deserialize(data, target);
		}

		public void Instantiate(MapObjectData data, Transform parent, Action<GameObject> onInstantiate = null)
		{
			var mapObject = this._mapObjects[data.GetType()];
			var prefab = mapObject.Prefab;

			var syncStore = s_syncStores[this._networkID];
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
							mapObject.OnInstantiate(networkInstance);
							this.Deserialize(data, networkInstance);

							onInstantiate?.Invoke(networkInstance);

							int viewID = networkInstance.GetComponent<PhotonView>().ViewID;

							// Communicate the photon instantiated map object to other clients
							NetworkingManager.RPC_Others(typeof(MapObjectManager), nameof(RPC_SyncInstantiation), this._networkID, instantiationID, viewID);
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

			mapObject.OnInstantiate(instance);
			this.Deserialize(data, instance);

			// The onInstantiate callback might be called twice: once for the "client-side" instance, and once for the networked instance
			onInstantiate?.Invoke(instance);
		}

		private IEnumerator SyncInstantiation(object target, int instantiationID, MapObjectData data, Action<GameObject> onInstantiate)
		{
			var syncStore = s_syncStores[this._networkID];
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
			s_syncStores[networkID].Set(instantiationID, viewID);
		}
	}
}
