using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MapsExt
{
	class PrefabPool : IPunPrefabPool
	{
		public void Destroy(GameObject gameObject)
		{
			throw new NotImplementedException();
		}

		public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
		{
			throw new NotImplementedException();
		}

		public void RegisterPrefab(string prefabID, GameObject prefab)
		{
			throw new NotImplementedException();
		}
	}
}
