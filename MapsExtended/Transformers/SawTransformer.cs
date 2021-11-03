using UnityEngine;
using MapsExt.MapObjects;

namespace MapsExt.Transformers
{
	public class SawTransformer : MonoBehaviour
	{
		public void Start()
		{
			var collider = this.GetComponent<CircleCollider2D>();
			float oldRadius = collider.radius;
			collider.radius = 0.5f;

			float ratio = (collider.radius / oldRadius);
			this.transform.GetChild(0).localScale *= ratio;

			this.GetComponent<MapObjectInstance>().FixShadow();
		}
	}
}
