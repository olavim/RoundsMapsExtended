using UnityEngine;

namespace MapsExt.MapObjects
{
	public abstract class DamageableMapObject : SpatialMapObject
	{
		public bool damageableByEnvironment = false;
	}

	public class DamageableMapObjectInstance : MonoBehaviour
	{
		public bool damageableByEnvironment;
	}
}
