using System.Collections.Generic;
using UnityEngine;

namespace MapsExt
{
	public static class GameObjectUtils
	{
		public static void DestroyImmediateSafe(GameObject obj)
		{
			obj.transform.SetParent(null, true);
			obj.name = "$destroyed";
			GameObject.Destroy(obj);
			obj.SetActive(false);
		}

		public static void DestroyChildrenImmediateSafe(GameObject obj)
		{
			var children = new List<GameObject>();
			foreach (Transform child in obj.transform)
			{
				children.Add(child.gameObject);
			}
			foreach (var child in children)
			{
				GameObjectUtils.DestroyImmediateSafe(child);
			}
		}

		public static void DisableRigidbody(GameObject go)
		{
			var rigidbody = go.GetComponent<Rigidbody2D>();
			if (rigidbody != null)
			{
				rigidbody.simulated = true;
				rigidbody.velocity = Vector2.zero;
				rigidbody.angularVelocity = 0;
				rigidbody.isKinematic = true;
			}
		}
	}
}
