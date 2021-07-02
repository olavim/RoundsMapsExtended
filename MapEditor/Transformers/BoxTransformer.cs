using UnityEngine;

namespace MapEditor.Transformers
{
    public class BoxTransformer : MonoBehaviour, IMapObjectTransformer
    {
        public bool Resize(Vector3 mouseDelta, int resizeDirection)
        {
            float gridSize = this.gameObject.GetComponentInParent<MapEditor>().gridSize;

            var scaleMulti = TogglePosition.directionMultipliers[resizeDirection];

            var snappedRotation = this.transform.rotation;
            snappedRotation.eulerAngles = new Vector3(0, 0, EditorUtils.Snap(snappedRotation.eulerAngles.z, 90));

            var scaleDelta = Vector3.Scale(snappedRotation * mouseDelta, new Vector3(scaleMulti.x, scaleMulti.y, 0));
            var positionDelta = this.transform.rotation * Vector3.Scale(snappedRotation * mouseDelta, new Vector3(Mathf.Abs(scaleMulti.x), Mathf.Abs(scaleMulti.y), 0));

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
