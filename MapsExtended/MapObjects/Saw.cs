using MapsExtended.Transformers;
using UnityEngine;

namespace MapsExtended.MapObjects
{
    public class Saw : PhysicalMapObject { }

    [MapsExtendedMapObject(typeof(Saw))]
    public class SawSpecification : PhysicalMapObjectSpecification<Saw>
    {
        public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/MapObject_Saw_Stat");

        protected override void Deserialize(Saw data, GameObject target)
        {
            base.Deserialize(data, target);
            target.AddComponent<SawTransformer>();
        }
    }
}
