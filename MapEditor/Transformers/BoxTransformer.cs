using UnityEngine;

namespace MapEditor.Transformers
{
    public class BoxTransformer : MonoBehaviour, IMapObjectTransformer
    {
        public bool Resize(Vector3 mouseDelta, int resizeDirection)
        {
            float gridSize = this.gameObject.GetComponentInParent<MapEditor>().gridSize;

            var scaleMulti = TogglePosition.directionMultipliers[resizeDirection];
            var scaleDelta = Vector3.Scale(mouseDelta, new Vector3(scaleMulti.x, scaleMulti.y, 0));
            var positionDelta = Vector3.Scale(mouseDelta, new Vector3(Mathf.Abs(scaleMulti.x), Mathf.Abs(scaleMulti.y), 0));

            var newScale = this.transform.localScale + scaleDelta;
            if (newScale.x < gridSize || newScale.y < gridSize)
            {
                return false;
            }

            this.transform.localScale = newScale;
            this.transform.position += positionDelta * 0.5f;
            return true;
        }
    }
}
