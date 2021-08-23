using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(MapsExt.MapObjects.Saw), "Saw", "Static")]
	public class EditorSaw : SawSpecification, IEditorMapObjectSpecification
	{
		protected override void Deserialize(MapsExt.MapObjects.Saw data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}
