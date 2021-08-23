using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(Ball), "Ball", "Dynamic")]
	public class EditorBallSpecification : BallSpecification, IEditorMapObjectSpecification
	{
		protected override void Deserialize(Ball data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}
