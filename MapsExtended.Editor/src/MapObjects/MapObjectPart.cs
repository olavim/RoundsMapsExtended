using MapsExt.Editor.Events;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	public sealed class MapObjectPart : MonoBehaviour
	{
		public Collider2D Collider { get; private set; }

		private void Awake()
		{
			this.Collider = this.GetComponent<Collider2D>() ?? this.gameObject.AddComponent<BoxCollider2D>();
			this.gameObject.GetOrAddComponent<MapObjectPartHandler>();
		}
	}
}
