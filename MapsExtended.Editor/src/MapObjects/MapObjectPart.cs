using MapsExt.Editor.Events;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	public sealed class MapObjectPart : MonoBehaviour
	{
		public Collider2D Collider => this.GetComponent<Collider2D>();

		private void Awake()
		{
			this.gameObject.GetOrAddComponent<MapObjectPartHandler>();

			if (this.GetComponent<Collider2D>() == null)
			{
				this.gameObject.AddComponent<BoxCollider2D>();
			}
		}
	}
}
