using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(BoxBackground), "Box (Background)", "Dynamic")]
	public class EditorBoxBackgroundSpecification : BoxBackgroundSpecification, IEditorMapObjectSpecification
	{
		protected override void Deserialize(BoxBackground data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<BoxActionHandler>();
		}
	}
}
