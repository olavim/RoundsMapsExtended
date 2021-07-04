using UnityEngine;

namespace MapsExtended.Editor
{
    public class SawActionHandler : MonoBehaviour, IEditorActionHandler
    {
        public bool CanRotate()
        {
            return false;
        }

        public bool CanResize(int resizeDirection)
        {
            var multi = TogglePosition.directionMultipliers[resizeDirection];
            return Mathf.Abs(multi.x) == Mathf.Abs(multi.y);
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
