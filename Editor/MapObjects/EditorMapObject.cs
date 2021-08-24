using UnityEngine;
using MapsExt.MapObjects;
using HarmonyLib;
using UnboundLib;

namespace MapsExt.Editor.MapObjects
{
	public interface IEditorMapObjectSpecification : IMapObjectSpecification
	{
		new GameObject Prefab { get; }

		new void Deserialize(MapObject data, GameObject target);

		new void Serialize(GameObject instance, MapObject target);
	}

	public abstract class BasicEditorMapObjectSpecification<T> : IEditorMapObjectSpecification
		where T : IMapObjectSpecification
	{
		private IMapObjectSpecification baseSpec = AccessTools.CreateInstance<T>();

		public virtual GameObject Prefab => this.baseSpec.Prefab;

		public virtual void Deserialize(MapObject data, GameObject target)
		{
			this.baseSpec.Deserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}

		public virtual void Serialize(GameObject instance, MapObject target)
		{
			this.baseSpec.Serialize(instance, target);
		}
	}

	[MapsExtendedEditorMapObject(typeof(Ball), "Ball", "Dynamic")]
	public class EditorBallSpecification : BasicEditorMapObjectSpecification<BallSpecification> { }

	[MapsExtendedEditorMapObject(typeof(Box), "Box", "Dynamic")]
	public class EditorBoxSpecification : BasicEditorMapObjectSpecification<BoxSpecification> { }

	[MapsExtendedEditorMapObject(typeof(BoxBackground), "Box (Background)", "Dynamic")]
	public class EditorBoxBackgroundSpecification : BasicEditorMapObjectSpecification<BoxBackgroundSpecification> { }

	[MapsExtendedEditorMapObject(typeof(BoxDestructible), "Box (Destructible)", "Dynamic")]
	public class EditorBoxDestructibleSpecification : BasicEditorMapObjectSpecification<BoxDestructibleSpecification> { }

	[MapsExtendedEditorMapObject(typeof(Ground), "Ground", "Static")]
	public class EditorGroundSpecification : BasicEditorMapObjectSpecification<GroundSpecification> { }

	[MapsExtendedEditorMapObject(typeof(GroundCircle), "Ground (Circle)", "Static")]
	public class EditorGroundCircleSpecification : BasicEditorMapObjectSpecification<GroundCircleSpecification> { }

	[MapsExtendedEditorMapObject(typeof(MapsExt.MapObjects.Saw), "Saw", "Static")]
	public class EditorSaw : BasicEditorMapObjectSpecification<SawSpecification> { }

	[MapsExtendedEditorMapObject(typeof(SawDynamic), "Saw", "Dynamic")]
	public class EditorSawDynamic : BasicEditorMapObjectSpecification<SawDynamicSpecification> { }
}
