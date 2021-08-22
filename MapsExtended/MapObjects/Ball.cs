using UnityEngine;

namespace MapsExt.MapObjects
{
	public class Ball : PhysicalMapObject { }

	[MapsExtendedMapObject(typeof(Ball))]
	public class BallSpecification : PhysicalMapObjectSpecification<Ball>
	{
		public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Ball_Big");
	}
}
