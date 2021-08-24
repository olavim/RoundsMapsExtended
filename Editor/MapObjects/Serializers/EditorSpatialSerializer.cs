using UnityEngine;
using MapsExt.MapObjects;
using UnboundLib;

namespace MapsExt.Editor.MapObjects
{
	public static class EditorSpatialSerializer
	{
		public static void Serialize(GameObject instance, SpatialMapObject target) { }

		public static void Deserialize(SpatialMapObject data, GameObject target)
		{
			target.GetOrAddComponent<SpatialActionHandler>();
		}

		// Helper methods to make simple editor map object specs less verbose to write
		internal static SerializerAction<T> BuildSerializer<T>(SerializerAction<T> action) where T : SpatialMapObject
		{
			SerializerAction<T> result = null;

			result += (instance, target) => EditorSpatialSerializer.Serialize(instance, (T)target);
			result += (instance, target) => action(instance, (T)target);

			return result;
		}

		internal static DeserializerAction<T> BuildDeserializer<T>(DeserializerAction<T> action) where T : SpatialMapObject
		{
			DeserializerAction<T> result = null;

			result += (data, target) => EditorSpatialSerializer.Deserialize((T)data, target);
			result += (data, target) => action((T)data, target);

			return result;
		}
	}
}
