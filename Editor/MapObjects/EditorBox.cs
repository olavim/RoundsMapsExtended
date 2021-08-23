using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(Box), "Box", "Dynamic")]
	public class EditorBoxSpecification : BoxSpecification, IEditorMapObjectSpecification
	{
		protected override void Deserialize(Box data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}
