using System;
using UnityEngine;

namespace MapsExt.MapObjects
{
	[Obsolete("Use MapObjectData instead")]
	public class MapObject : MapObjectData
	{
		public bool active;

		protected override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			this.Active = this.active;
		}

		protected override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			this.active = this.Active;
		}
	}

	[Obsolete("Deprecated")]
	public delegate void SerializerAction<T>(GameObject instance, T target) where T : MapObjectData;

	[Obsolete("Deprecated")]
	public delegate void DeserializerAction<T>(T data, GameObject target) where T : MapObjectData;
}
