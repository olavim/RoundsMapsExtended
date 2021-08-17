using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(Box), "Box", "Dynamic")]
    public class EditorBoxSpecification : BoxSpecification
    {
        protected override void OnDeserialize(Box data, GameObject target)
        {
            base.OnDeserialize(data, target);
            target.AddComponent<BoxActionHandler>();
        }
    }
}
