using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(Box))]
    public class EditorBoxSpecification : BoxSpecification
    {
        protected override void Deserialize(Box data, GameObject target)
        {
            base.Deserialize(data, target);
            target.AddComponent<BoxActionHandler>();
        }
    }
}
