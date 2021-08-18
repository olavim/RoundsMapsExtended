using MapsExt.MapObjects;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(Box), "Box", "Dynamic")]
    public class EditorBoxSpecification : BoxSpecification
    {
        protected override void OnDeserialize(Box data, GameObject target)
        {
            base.OnDeserialize(data, target);
            target.GetOrAddComponent<BoxActionHandler>();
        }
    }
}
