using UnityEngine;
using System.Collections.Generic;

namespace MapEditor.Transformers
{
    public class SawTransformer : MonoBehaviour, IMapObjectTransformer
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

        public bool Resize(Vector3 mouseDelta, int resizeDirection)
        {
            float gridSize = this.gameObject.GetComponentInParent<MapEditor>().gridSize;

            var scaleMulti = TogglePosition.directionMultipliers[resizeDirection];
            var scaleDelta = Vector3.Scale(mouseDelta, new Vector3(scaleMulti.x, scaleMulti.y, 0));            
            var newScale = this.transform.localScale + scaleDelta;

            if (newScale.x != newScale.y || newScale.x < gridSize || newScale.y < gridSize)
            {
                return false;
            }

            var positionDelta = Vector3.Scale(mouseDelta, new Vector3(Mathf.Abs(scaleMulti.x), Mathf.Abs(scaleMulti.y), 0));

            this.transform.localScale = newScale;
            this.transform.position += positionDelta * 0.5f;
            return true;
        }
    }
}
