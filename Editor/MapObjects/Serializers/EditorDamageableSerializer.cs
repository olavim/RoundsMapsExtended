using UnityEngine;
using MapsExt.MapObjects;
using UnboundLib;
using MapsExt.Editor.UI;
using MapsExt.Editor.Commands;

namespace MapsExt.Editor.MapObjects
{
	public static class EditorDamageableSerializer
	{
		public static void Serialize(GameObject instance, DamageableMapObject target) { }

		public static void Deserialize(DamageableMapObject data, GameObject target)
		{
			target.GetOrAddComponent<DamageableInspectorSpec>();
		}

		// Helper methods to make simple editor map object specs less verbose to write
		internal static SerializerAction<T> BuildSerializer<T>(SerializerAction<T> action) where T : DamageableMapObject
		{
			SerializerAction<T> result = null;

			result += (instance, target) => EditorSpatialSerializer.Serialize(instance, (T) target);
			result += (instance, target) => EditorDamageableSerializer.Serialize(instance, (T) target);
			result += (instance, target) => action(instance, (T) target);

			return result;
		}

		internal static DeserializerAction<T> BuildDeserializer<T>(DeserializerAction<T> action) where T : DamageableMapObject
		{
			DeserializerAction<T> result = null;

			result += (data, target) => EditorSpatialSerializer.Deserialize((T) data, target);
			result += (data, target) => EditorDamageableSerializer.Deserialize((T) data, target);
			result += (data, target) => action((T) data, target);

			return result;
		}
	}

	public class DamageableInspectorSpec : InspectorSpec
	{
		[MapObjectInspector.BooleanProperty("Damageable by Environment", typeof(SetDamageableByEnvironmentCommand))]
		public bool damageableByEnvironment => this.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment;
	}
}
