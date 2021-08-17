using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(BoxBackground), "Box (Background)", "Dynamic")]
    public class EditorBoxBackgroundSpecification : BoxBackgroundSpecification
    {
        protected override void OnDeserialize(BoxBackground data, GameObject target)
        {
            base.OnDeserialize(data, target);
            target.AddComponent<BoxActionHandler>();
        }
    }
}
