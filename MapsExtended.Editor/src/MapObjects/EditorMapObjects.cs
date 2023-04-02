using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObject(typeof(BallData), "Ball", Category = "Dynamic")]
	public sealed class EditorBall : Ball
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			GameObjectUtils.DisableRigidbody(instance);
		}
	}

	[EditorMapObject(typeof(BoxData), "Box", Category = "Dynamic")]
	public sealed class EditorBox : Box
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			GameObjectUtils.DisableRigidbody(instance);
		}
	}

	[EditorMapObject(typeof(BoxBackgroundData), "Box (Background)", Category = "Dynamic")]
	public sealed class EditorBoxBackground : BoxBackground
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			GameObjectUtils.DisableRigidbody(instance);
		}
	}

	[EditorMapObject(typeof(BoxDestructibleData), "Box (Destructible)", Category = "Dynamic")]
	public sealed class EditorBoxDestructible : BoxDestructible
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			GameObjectUtils.DisableRigidbody(instance);
			GameObject.Destroy(instance.GetComponent<Damagable>());
		}
	}

	[EditorMapObject(typeof(GroundData), "Ground", Category = "Static")]
	public sealed class EditorGround : Ground { }

	[EditorMapObject(typeof(GroundCircleData), "Ground (Circle)", Category = "Static")]
	public sealed class EditorGroundCircle : GroundCircle { }

	[EditorMapObject(typeof(SawData), "Saw", Category = "Static")]
	public sealed class EditorSaw : MapsExt.MapObjects.Saw { }

	[EditorMapObject(typeof(SawDynamicData), "Saw", Category = "Dynamic")]
	public sealed class EditorSawDynamic : SawDynamic
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			GameObjectUtils.DisableRigidbody(instance);
		}
	}

	[EditorMapObject(typeof(SpawnData), "Spawn")]
	public sealed class EditorSpawn : Spawn
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			instance.GetOrAddComponent<Visualizers.SpawnVisualizer>();
		}
	}

	[EditorMapObject(typeof(RopeData), "Rope")]
	public sealed class EditorRope : Rope
	{
		public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Editor Rope");
	}
}
