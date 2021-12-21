using UnityEngine;
using MapsExt.MapObjects;
using MapsExt.UI;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObjectSpec(typeof(BoxDestructible), "Box (Destructible)", "Dynamic")]
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
