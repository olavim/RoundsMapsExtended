using MapsExt.MapObjects;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObjectBlueprint("Ball", "Dynamic")]
	public class EditorBallBP : EditorSpatialMapObjectBlueprint<Ball> { }

	[EditorMapObjectBlueprint("Box", "Dynamic")]
	public class EditorBoxBP : EditorSpatialMapObjectBlueprint<Box> { }

	[EditorMapObjectBlueprint("Box (Background)", "Dynamic")]
	public class EditorBoxBackgroundBP : EditorSpatialMapObjectBlueprint<BoxBackground> { }

	[EditorMapObjectBlueprint("Ground", "Static")]
	public class EditorGroundBP : EditorSpatialMapObjectBlueprint<Ground> { }

	[EditorMapObjectBlueprint("Ground (Circle)", "Static")]
	public class EditorGroundCircleBP : EditorSpatialMapObjectBlueprint<GroundCircle> { }

	[EditorMapObjectBlueprint("Saw", "Static")]
	public class EditorSawBP : EditorSpatialMapObjectBlueprint<MapsExt.MapObjects.Saw> { }

	[EditorMapObjectBlueprint("Saw", "Dynamic")]
	public class EditorSawDynamicBP : EditorSpatialMapObjectBlueprint<SawDynamic> { }
}
