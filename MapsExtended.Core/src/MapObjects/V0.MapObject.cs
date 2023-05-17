using System;
using UnityEngine;

namespace MapsExt.MapObjects
{
	[Obsolete("Deprecated")]
	public delegate void SerializerAction<T>(GameObject instance, T target) where T : MapObjectData;

	[Obsolete("Deprecated")]
	public delegate void DeserializerAction<T>(T data, GameObject target) where T : MapObjectData;
}
