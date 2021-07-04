using UnityEngine;
using System.Collections.Generic;

namespace MapsExtended.Transformers
{
    public class SawTransformer : MonoBehaviour
    {
        public void Start()
        {
            var collider = this.GetComponent<CircleCollider2D>();
            float oldRadius = collider.radius;
            collider.radius = 0.5f;

            float ratio = (collider.radius / oldRadius);
            this.transform.GetChild(0).localScale *= ratio;

            var shadow = this.GetComponent<SFPolygon>();
            var path = shadow.GetPath(0);
            var newPath = new List<Vector2>();

            foreach (var point in path)
            {
                newPath.Add(point * ratio);
            }

            shadow.SetPath(0, newPath.ToArray());
        }
    }
}
