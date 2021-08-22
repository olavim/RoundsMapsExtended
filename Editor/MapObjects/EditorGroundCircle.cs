using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(GroundCircle), "Ground (Circle)", "Static")]
    public class EditorGroundCircleSpecification : GroundCircleSpecification
    {
        protected override void OnDeserialize(GroundCircle data, GameObject target)
        {
            base.OnDeserialize(data, target);
            target.GetOrAddComponent<BoxActionHandler>();
        }
    }
}
