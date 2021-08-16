using UnityEngine;

namespace MapsExt.MapObjects
{
    public class BoxBackground : PhysicalMapObject { }

    [MapsExtendedMapObject(typeof(BoxBackground))]
    public class BoxBackgroundSpecification : PhysicalMapObjectSpecification<BoxBackground>
    {
        public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box_BG");
    }
}
