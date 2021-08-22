using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(Ball), "Ball", "Dynamic")]
	public class EditorBallSpecification : BallSpecification
	{
		protected override void OnDeserialize(Ball data, GameObject target)
		{
			base.OnDeserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}
