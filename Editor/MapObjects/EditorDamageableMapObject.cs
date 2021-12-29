using UnityEngine;
using MapsExt.MapObjects;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObjectSpec(typeof(BoxDestructible), "Box (Destructible)", "Dynamic")]
	[EditorInspectorSpec(typeof(DamageableInspectorSpec))]
	public static class EditorBoxDestructibleSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => BoxDestructibleSpec.Prefab;
		[EditorMapObjectSerializer]
		public static SerializerAction<BoxDestructible> Serialize => EditorDamageableSerializer.BuildSerializer<BoxDestructible>(BoxDestructibleSpec.Serialize);
		[EditorMapObjectDeserializer]
		public static DeserializerAction<BoxDestructible> Deserialize => EditorDamageableSerializer.BuildDeserializer<BoxDestructible>(BoxDestructibleSpec.Deserialize);
	}
}
