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
            bool snapToGrid = this.gameObject.GetComponentInParent<MapEditor>().SnapToGrid;

            var scaleMulti = TogglePosition.directionMultipliers[resizeDirection];
            var scaleDelta = Vector3.Scale(mouseDelta, new Vector3(scaleMulti.x, scaleMulti.y, 0));            
            var newScale = this.transform.localScale + scaleDelta;

            if (snapToGrid && newScale.x != this.transform.localScale.x && newScale.x < gridSize)
            {
                newScale.x = gridSize;
            }

            if (snapToGrid && newScale.y != this.transform.localScale.y && newScale.y < gridSize)
            {
                newScale.y = gridSize;
            }

            if (newScale == this.transform.localScale)
            {
                return false;
            }

            this.transform.localScale = newScale;
            var positionDelta = Vector3.Scale(mouseDelta, new Vector3(Mathf.Abs(scaleMulti.x), Mathf.Abs(scaleMulti.y), 0));
            this.transform.position += positionDelta * 0.5f;

            return true;
        }
    }
}
