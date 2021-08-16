using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(MapsExt.MapObjects.Saw))]
    public class EditorSaw : SawSpecification
    {
        protected override void Deserialize(MapsExt.MapObjects.Saw data, GameObject target)
        {
            base.Deserialize(data, target);
            target.AddComponent<BoxActionHandler>();
        }
    }
}
