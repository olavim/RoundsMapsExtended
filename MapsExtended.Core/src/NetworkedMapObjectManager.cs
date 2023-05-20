using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnboundLib;
using UnboundLib.Networking;
using MapsExt.MapObjects;
using Photon.Pun;

namespace MapsExt
{
	public sealed class NetworkedMapObjectManager : MapObjectManager
	{
		private static readonly Dictionary<string, TargetSyncedStore<int>> s_syncStores = new();

		[UnboundRPC]
		public static void RPC_SyncInstantiation(string networkID, int instantiationID, int viewID)
		{
			s_syncStores[networkID].Set(instantiationID, viewID);
		}

		private string _networkID;

		public override void RegisterMapObject(Type dataType, IMapObject mapObject, IMapObjectSerializer serializer)
		{
			base.RegisterMapObject(dataType, mapObject, serializer);

			if (mapObject.Prefab.GetComponent<PhotonMapObject>())
			{
				PhotonNetwork.PrefabPool.RegisterPrefab(this.GetInstanceName(dataType), mapObject.Prefab);

				// PhotonMapObject has a hard-coded prefab name prefix / resource location
				PhotonNetwork.PrefabPool.RegisterPrefab("4 Map Objects/" + this.GetInstanceName(dataType), mapObject.Prefab);
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

		private string GetInstanceName(Type type)
		{
			return $"{this._networkID}/{type.FullName}";
		}

		public override void Instantiate(MapObjectData data, Transform parent, Action<GameObject> onInstantiate = null)
		{
			var mapObject = this.GetMapObject(data.GetType());
			var prefab = mapObject.Prefab;

			var syncStore = s_syncStores[this._networkID];
			int instantiationID = syncStore.Allocate(parent);

			var instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);

			if (prefab.GetComponent<PhotonMapObject>() != null)
			{
				if (PhotonNetwork.IsMasterClient)
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
					MapsExtended.AddPhotonInstantiateListener(instance.GetComponent<PhotonMapObject>(), networkInstance =>
					{
						mapObject.OnInstantiate(networkInstance);
						this.WriteMapObject(data, networkInstance);

						onInstantiate?.Invoke(networkInstance);

						int viewID = networkInstance.GetComponent<PhotonView>().ViewID;

						// Communicate the photon instantiated map object to other clients
						NetworkingManager.RPC_Others(typeof(NetworkedMapObjectManager), nameof(RPC_SyncInstantiation), this._networkID, instantiationID, viewID);
					});
				}
				else
				{
					// Call onInstantiate once the master client has communicated the photon instantiated map object
					this.StartCoroutine(this.SyncInstantiation(parent, instantiationID, data, onInstantiate));
				}
			}

			instance.name = this.GetInstanceName(data.GetType());

			mapObject.OnInstantiate(instance);
			this.WriteMapObject(data, instance);

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

				this.WriteMapObject(data, networkInstance);

				onInstantiate?.Invoke(networkInstance);
			}
		}
	}
}
