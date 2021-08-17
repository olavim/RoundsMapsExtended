using UnityEngine;

namespace MapsExt.MapObjects
{
    public class Rope : MapObject
    {
        public Vector3 startPosition = Vector3.up;
        public Vector3 endPosition = Vector3.down;
    }

    [MapsExtendedMapObject(typeof(Rope))]
    public class RopeSpecification : MapObjectSpecification<Rope>
    {
        public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Rope");

        protected override void OnDeserialize(Rope data, GameObject target)
        {
            target.transform.position = data.startPosition;
            target.transform.GetChild(0).position = data.endPosition;
        }

        protected override Rope OnSerialize(GameObject instance)
        {
            return new Rope
            {
                startPosition = instance.transform.position,
                endPosition = instance.transform.GetChild(0).position
            };
        }
    }
}
