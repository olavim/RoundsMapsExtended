using MapsExt.MapObjects;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObject("Ball", "Dynamic")]
	public class EditorBall : Ball { }

	[EditorMapObject("Box", "Dynamic")]
	public class EditorBox : Box { }

	[EditorMapObject("Box (Background)", "Dynamic")]
	public class EditorBoxBackground : BoxBackground { }

	[EditorMapObject("Ground", "Static")]
	public class EditorGround : Ground { }

	[EditorMapObject("Ground (Circle)", "Static")]
	public class EditorGroundCircle : GroundCircle { }

	[EditorMapObject("Saw", "Static")]
	public class EditorSaw : MapsExt.MapObjects.Saw { }

	[EditorMapObject("Saw", "Dynamic")]
	public class EditorSawDynamic : SawDynamic { }
}
