using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObject(typeof(BallData), "Ball", Category = "Dynamic")]
	public class EditorBall : Ball { }

	[EditorMapObject(typeof(BoxData), "Box", Category = "Dynamic")]
	public class EditorBox : Box { }

	[EditorMapObject(typeof(BoxBackgroundData), "Box (Background)", Category = "Dynamic")]
	public class EditorBoxBackground : BoxBackground { }

	[EditorMapObject(typeof(BoxDestructibleData), "Box (Destructible)", Category = "Dynamic")]
	public class EditorBoxDestructible : BoxDestructible { }

	[EditorMapObject(typeof(GroundData), "Ground", Category = "Static")]
	public class EditorGround : Ground { }

	[EditorMapObject(typeof(GroundCircleData), "Ground (Circle)", Category = "Static")]
	public class EditorGroundCircle : GroundCircle { }

	[EditorMapObject(typeof(SawData), "Saw", Category = "Static")]
	public class EditorSaw : MapsExt.MapObjects.Saw { }

	[EditorMapObject(typeof(SawDynamicData), "Saw", Category = "Dynamic")]
	public class EditorSawDynamic : SawDynamic { }

	[EditorMapObject(typeof(SpawnData), "Spawn")]
	public class EditorSpawn : Spawn
	{
		public override void OnInstantiate(GameObject instance)
		{
			instance.GetOrAddComponent<Visualizers.SpawnVisualizer>();
		}
	}

	[EditorMapObject(typeof(RopeData), "Rope")]
	public class EditorRope : IMapObject
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Editor Rope");

		public void OnInstantiate(GameObject instance) { }
	}
}
