using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
    [MapsExtendedEditorMapObject(typeof(Spawn))]
    public class EditorSpawnSpecification : SpawnSpecification
    {
        protected override void Deserialize(Spawn data, GameObject target)
        {
            base.Deserialize(data, target);
            target.gameObject.AddComponent<Visualizers.SpawnVisualizer>();
            target.gameObject.AddComponent<SpawnActionHandler>();
            target.transform.SetAsLastSibling();
        }
    }
}
