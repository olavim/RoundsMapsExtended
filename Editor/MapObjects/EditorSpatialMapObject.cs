using UnityEngine;
using MapsExt.MapObjects;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObjectSpec(typeof(Ball), "Ball", "Dynamic")]
	public static class EditorBallSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => BallSpec.Prefab;
		[EditorMapObjectSerializer]
		public static SerializerAction<Ball> Serialize => EditorSpatialSerializer.BuildSerializer<Ball>(BallSpec.Serialize);
		[EditorMapObjectDeserializer]
		public static DeserializerAction<Ball> Deserialize => EditorSpatialSerializer.BuildDeserializer<Ball>(BallSpec.Deserialize);
	}

	[EditorMapObjectSpec(typeof(Box), "Box", "Dynamic")]
	public static class EditorBoxSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => BoxSpec.Prefab;
		[EditorMapObjectSerializer]
		public static SerializerAction<Box> Serialize => EditorSpatialSerializer.BuildSerializer<Box>(BoxSpec.Serialize);
		[EditorMapObjectDeserializer]
		public static DeserializerAction<Box> Deserialize => EditorSpatialSerializer.BuildDeserializer<Box>(BoxSpec.Deserialize);
	}

	[EditorMapObjectSpec(typeof(BoxBackground), "Box (Background)", "Dynamic")]
	public static class EditorBoxBackgroundSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => BoxBackgroundSpec.Prefab;
		[EditorMapObjectSerializer]
		public static SerializerAction<BoxBackground> Serialize => EditorSpatialSerializer.BuildSerializer<BoxBackground>(BoxBackgroundSpec.Serialize);
		[EditorMapObjectDeserializer]
		public static DeserializerAction<BoxBackground> Deserialize => EditorSpatialSerializer.BuildDeserializer<BoxBackground>(BoxBackgroundSpec.Deserialize);
	}

	[EditorMapObjectSpec(typeof(BoxDestructible), "Box (Destructible)", "Dynamic")]
	public static class EditorBoxDestructibleSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => BoxDestructibleSpec.Prefab;
		[EditorMapObjectSerializer]
		public static SerializerAction<BoxDestructible> Serialize => EditorSpatialSerializer.BuildSerializer<BoxDestructible>(BoxDestructibleSpec.Serialize);
		[EditorMapObjectDeserializer]
		public static DeserializerAction<BoxDestructible> Deserialize => EditorSpatialSerializer.BuildDeserializer<BoxDestructible>(BoxDestructibleSpec.Deserialize);
	}

	[EditorMapObjectSpec(typeof(Ground), "Ground", "Static")]
	public static class EditorGroundSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => GroundSpec.Prefab;
		[EditorMapObjectSerializer]
		public static SerializerAction<Ground> Serialize => EditorSpatialSerializer.BuildSerializer<Ground>(GroundSpec.Serialize);
		[EditorMapObjectDeserializer]
		public static DeserializerAction<Ground> Deserialize => EditorSpatialSerializer.BuildDeserializer<Ground>(GroundSpec.Deserialize);
	}

	[EditorMapObjectSpec(typeof(GroundCircle), "Ground (Circle)", "Static")]
	public static class EditorGroundCircleSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => GroundCircleSpec.Prefab;
		[EditorMapObjectSerializer]
		public static SerializerAction<GroundCircle> Serialize => EditorSpatialSerializer.BuildSerializer<GroundCircle>(GroundCircleSpec.Serialize);
		[EditorMapObjectDeserializer]
		public static DeserializerAction<GroundCircle> Deserialize => EditorSpatialSerializer.BuildDeserializer<GroundCircle>(GroundCircleSpec.Deserialize);
	}

	[EditorMapObjectSpec(typeof(MapsExt.MapObjects.Saw), "Saw", "Static")]
	public static class EditorSawSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => SawSpec.Prefab;
		[EditorMapObjectSerializer]
		public static SerializerAction<MapsExt.MapObjects.Saw> Serialize => EditorSpatialSerializer.BuildSerializer<MapsExt.MapObjects.Saw>(SawSpec.Serialize);
		[EditorMapObjectDeserializer]
		public static DeserializerAction<MapsExt.MapObjects.Saw> Deserialize => EditorSpatialSerializer.BuildDeserializer<MapsExt.MapObjects.Saw>(SawSpec.Deserialize);
	}

	[EditorMapObjectSpec(typeof(SawDynamic), "Saw", "Dynamic")]
	public static class EditorSawDynamicSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => SawDynamicSpec.Prefab;
		[EditorMapObjectSerializer]
		public static SerializerAction<SawDynamic> Serialize => EditorSpatialSerializer.BuildSerializer<SawDynamic>(SawDynamicSpec.Serialize);
		[EditorMapObjectDeserializer]
		public static DeserializerAction<SawDynamic> Deserialize => EditorSpatialSerializer.BuildDeserializer<SawDynamic>(SawDynamicSpec.Deserialize);
	}
}
