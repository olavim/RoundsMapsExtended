using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using Jotunn.Utils;
using MapsExt.MapObjects;
using Photon.Pun;
using HarmonyLib;

namespace MapsExt
{
	public class MapObjectManager : MonoBehaviour
	{
		private class Spec
		{
			public GameObject prefab;
			public SerializerAction<MapObject> serializer;
			public DeserializerAction<MapObject> deserializer;
		}

		private static AssetBundle mapObjectBundle;
		private static Dictionary<string, TargetSyncedStore<int>> syncStores = new Dictionary<string, TargetSyncedStore<int>>();

		public static TObj LoadCustomAsset<TObj>(string name) where TObj : UnityEngine.Object
		{
			return MapObjectManager.mapObjectBundle.LoadAsset<TObj>(name);
		}

		private string NetworkID { get; set; }

		private readonly Dictionary<Type, Spec> specs = new Dictionary<Type, Spec>();

		private void Awake()
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

		public void RegisterType(Type dataType, GameObject prefab)
		{
			if (!typeof(MapObject).IsAssignableFrom(dataType))
			{
				throw new ArgumentException($"Invalid data type '{dataType.FullName}': data type must be assignable from '{typeof(MapObject).FullName}'");
			}

			var spec = new Spec();
			spec.prefab = prefab;
			this.specs.Add(dataType, spec);

			if (prefab.GetComponent<PhotonMapObject>())
			{
				PhotonNetwork.PrefabPool.RegisterPrefab(this.GetInstanceName(dataType), prefab);

				// PhotonMapObject has a hard-coded prefab name prefix / resource location
				PhotonNetwork.PrefabPool.RegisterPrefab("4 Map Objects/" + this.GetInstanceName(dataType), prefab);
			}
		}

		public void RegisterSerializer(Type dataType, SerializerAction<MapObject> serializer)
		{
			if (this.specs.TryGetValue(dataType, out Spec spec))
			{
				spec.serializer = serializer;
			}
		}

		public void RegisterDeserializer(Type dataType, DeserializerAction<MapObject> deserializer)
		{
			if (this.specs.TryGetValue(dataType, out Spec spec))
			{
				spec.deserializer = deserializer;
			}
		}

		private string GetInstanceName(Type type)
		{
			return $"{this.NetworkID}/{type.FullName}";
		}

		public MapObject Serialize(MapObjectInstance mapObjectInstance)
		{
			if (mapObjectInstance == null)
			{
				throw new ArgumentException($"Cannot serialize null MapObjectInstance");
			}

			if (mapObjectInstance.dataType == null)
			{
				throw new ArgumentException($"Cannot serialize MapObjectInstance ({mapObjectInstance.gameObject.name}) because it's missing a dataType");
			}

			if (this.specs.TryGetValue(mapObjectInstance.dataType, out Spec spec))
			{
				try
				{
					var data = (MapObject) AccessTools.CreateInstance(mapObjectInstance.dataType);
					BaseMapObjectSerializer.Serialize(mapObjectInstance.gameObject, data);
					spec.serializer(mapObjectInstance.gameObject, data);
					return data;
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"Could not serialize map object instance with specification: {spec.GetType()}");
					ex.Rethrow();
					throw;
				}
			}
			else
			{
				throw new ArgumentException($"Specification not found for type {mapObjectInstance.dataType}");
			}
		}

		public void Deserialize(MapObject data, GameObject target)
		{
			BaseMapObjectSerializer.Deserialize(data, target);
			var spec = this.specs[data.GetType()];
			spec.deserializer(data, target);
		}

		public void Instantiate(MapObject data, Transform parent, Action<GameObject> onInstantiate = null)
		{
			var spec = this.specs[data.GetType()];

			var syncStore = MapObjectManager.syncStores[this.NetworkID];
			int instantiationID = syncStore.Allocate(parent);

			bool isMapObjectNetworked = spec.prefab.GetComponent<PhotonMapObject>() != null;
			bool isMapSpawned = MapManager.instance.currentMap?.Map.wasSpawned == true;

			GameObject instance;

			if (!isMapSpawned || !isMapObjectNetworked)
			{
				instance = GameObject.Instantiate(spec.prefab, Vector3.zero, Quaternion.identity, parent);

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
				 */
				instance = PhotonNetwork.Instantiate(this.GetInstanceName(data.GetType()), Vector3.zero, Quaternion.identity);

				MapsExtended.instance.ExecuteAfterFrames(1, () =>
				{
					instance.transform.SetParent(parent);
				});
			}


			instance.name = this.GetInstanceName(data.GetType());

			this.Deserialize(data, instance);

			// The onInstantiate callback might be called twice: once for the "client-side" instance, and once for the networked instance
			onInstantiate?.Invoke(instance);
		}

		private IEnumerator SyncInstantiation(object target, int instantiationID, MapObject data, Action<GameObject> onInstantiate)
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
