using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObject("Ball", "Dynamic")]
	public class EditorBall : Ball { }

	[EditorMapObject("Box", "Dynamic")]
	public class EditorBox : Box { }

	[EditorMapObject("Box (Background)", "Dynamic")]
	public class EditorBoxBackground : BoxBackground { }

	[EditorMapObject("Box (Destructible)", "Dynamic")]
	public class EditorBoxDestructible : BoxDestructible { }

	[EditorMapObject("Ground", "Static")]
	public class EditorGround : Ground { }

	[EditorMapObject("Ground (Circle)", "Static")]
	public class EditorGroundCircle : GroundCircle { }

	[EditorMapObject("Saw", "Static")]
	public class EditorSaw : MapsExt.MapObjects.Saw { }

	[EditorMapObject("Saw", "Dynamic")]
	public class EditorSawDynamic : SawDynamic { }

	[EditorMapObject("Spawn")]
	public class EditorSpawn : Spawn
	{
		public override void OnInstantiate(GameObject instance)
		{
			instance.GetOrAddComponent<Visualizers.SpawnVisualizer>();
		}
	}

	[EditorMapObject("Rope")]
	public class EditorRope : IMapObject<RopeData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Editor Rope");

		public void OnInstantiate(GameObject instance) { }
	}
}
