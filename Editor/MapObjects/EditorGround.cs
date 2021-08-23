using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(Ground), "Ground", "Static")]
	public class EditorGroundSpecification : GroundSpecification, IEditorMapObjectSpecification
	{
		protected override void Deserialize(Ground data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}
