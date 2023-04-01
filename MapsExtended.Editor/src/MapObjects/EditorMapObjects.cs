using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObject(typeof(BallData), "Ball", Category = "Dynamic")]
	public class EditorBall : Ball
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			GameObjectUtils.DisableRigidbody(instance);
		}
	}

	[EditorMapObject(typeof(BoxData), "Box", Category = "Dynamic")]
	public class EditorBox : Box
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			GameObjectUtils.DisableRigidbody(instance);
		}
	}

	[EditorMapObject(typeof(BoxBackgroundData), "Box (Background)", Category = "Dynamic")]
	public class EditorBoxBackground : BoxBackground
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			GameObjectUtils.DisableRigidbody(instance);
		}
	}

	[EditorMapObject(typeof(BoxDestructibleData), "Box (Destructible)", Category = "Dynamic")]
	public class EditorBoxDestructible : BoxDestructible
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			GameObjectUtils.DisableRigidbody(instance);
			GameObject.Destroy(instance.GetComponent<Damagable>());
		}
	}

	[EditorMapObject(typeof(GroundData), "Ground", Category = "Static")]
	public class EditorGround : Ground { }

	[EditorMapObject(typeof(GroundCircleData), "Ground (Circle)", Category = "Static")]
	public class EditorGroundCircle : GroundCircle { }

	[EditorMapObject(typeof(SawData), "Saw", Category = "Static")]
	public class EditorSaw : MapsExt.MapObjects.Saw { }

	[EditorMapObject(typeof(SawDynamicData), "Saw", Category = "Dynamic")]
	public class EditorSawDynamic : SawDynamic
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			GameObjectUtils.DisableRigidbody(instance);
		}
	}

	[EditorMapObject(typeof(SpawnData), "Spawn")]
	public class EditorSpawn : Spawn
	{
		public override void OnInstantiate(GameObject instance)
		{
			base.OnInstantiate(instance);
			instance.GetOrAddComponent<Visualizers.SpawnVisualizer>();
		}
	}

	[EditorMapObject(typeof(RopeData), "Rope")]
	public class EditorRope : Rope
	{
		public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Editor Rope");
	}
}
