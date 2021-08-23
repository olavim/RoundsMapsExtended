using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(GroundCircle), "Ground (Circle)", "Static")]
	public class EditorGroundCircleSpecification : GroundCircleSpecification, IEditorMapObjectSpecification
	{
		protected override void Deserialize(GroundCircle data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}
