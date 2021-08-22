using System;
using UnityEngine;
using UnboundLib;
using System.Collections.Generic;

namespace MapsExt.MapObjects
{
    public class MapObject
    {
        public bool active = true;
    }

    public abstract class MapObjectSpecification
    {
        public abstract GameObject Prefab { get; }

        public abstract void Deserialize(MapObject data, GameObject target);

        public abstract MapObject Serialize(GameObject instance);
    }

    public abstract class MapObjectSpecification<T> : MapObjectSpecification
        where T : MapObject
    {
        public override void Deserialize(MapObject data, GameObject target)
        {
            this.OnDeserialize((T) data, target);
            var c = target.GetOrAddComponent<MapObjectInstance>();
            c.dataType = data.GetType();
            target.SetActive(data.active);
        }

        public override MapObject Serialize(GameObject instance)
        {
            var data = this.OnSerialize(instance);
            data.active = instance.activeSelf;
            return data;
        }

        protected abstract void OnDeserialize(T data, GameObject target);

        protected abstract T OnSerialize(GameObject instance);
    }

    public class MapObjectInstance : MonoBehaviour
    {
        public Type dataType;

        public void Start()
        {
            this.FixShadow();
        }

        private void FixShadow()
        {
            var collider = this.gameObject.GetComponent<Collider2D>();

            if (collider == null)
            {
                return;
            }

            var sf = this.gameObject.GetOrAddComponent<SFPolygon>();
            sf.opacity = 0.5f;

            if (collider is PolygonCollider2D || collider is BoxCollider2D)
            {
                sf.CopyFromCollider(collider);
            }

            if (collider is CircleCollider2D circleCollider)
            {
                int numVertices = 24;
                float anglePerVertex = 360f / numVertices;
                float radius = circleCollider.radius;

                var identity = new Vector3(0, radius, 0);
                var vertices = new List<Vector2>();

                for (int i = 0; i < numVertices; i++)
                {
                    var rotation = Quaternion.Euler(0, 0, i * anglePerVertex);
                    var point = rotation * identity;
                    vertices.Add(new Vector2(point.x, point.y));
                }

                sf.SetPath(0, vertices.ToArray());
            }
        }
    }
}
