using UnityEngine;
using MapsExt.MapObjects;
using System;
using UnboundLib;
using MapsExt.Editor.ActionHandlers;

namespace MapsExt.Editor.MapObjects
{
	[Obsolete("Deprecated")]
	public static class EditorSpatialSerializer
	{
		public static void Serialize(GameObject instance, SpatialMapObject target) { }

		public static void Deserialize(SpatialMapObject data, GameObject target)
		{
			target.GetOrAddComponent<SelectionHandler>();
			target.GetOrAddComponent<PositionHandler>();
			target.GetOrAddComponent<SizeHandler>();
			target.GetOrAddComponent<ActionHandlers.RotationHandler>();

			GameObjectUtils.DisableRigidbody(target);
			if (target.GetComponent<Damagable>())
			{
				GameObject.Destroy(target.GetComponent<Damagable>());
			}
		}

		public static SerializerAction<T> BuildSerializer<T>(SerializerAction<T> action) where T : SpatialMapObject
		{
			SerializerAction<T> result = null;

			result += (instance, target) => EditorSpatialSerializer.Serialize(instance, target);
			result += (instance, target) => action(instance, target);

			return result;
		}

		public static DeserializerAction<T> BuildDeserializer<T>(DeserializerAction<T> action) where T : SpatialMapObject
		{
			DeserializerAction<T> result = null;

			result += (data, target) => EditorSpatialSerializer.Deserialize(data, target);
			result += (data, target) => action(data, target);

			return result;
		}
	}
}
