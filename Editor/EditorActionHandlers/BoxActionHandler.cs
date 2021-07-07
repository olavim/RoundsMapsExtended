using UnityEngine;

namespace MapsExtended.Editor
{
    public class BoxActionHandler : MonoBehaviour, IEditorActionHandler
    {
        public bool CanRotate()
        {
            return true;
        }
        public bool CanResize(int resizeDirection)
        {
            return true;
        }

        public bool Resize(Vector3 mouseDelta, int resizeDirection)
        {
            float gridSize = this.gameObject.GetComponentInParent<Editor.MapEditor>().gridSize;
            bool snapToGrid = this.gameObject.GetComponentInParent<Editor.MapEditor>().SnapToGrid;

            var scaleMulti = Editor.TogglePosition.directionMultipliers[resizeDirection];

            var snappedRotation = this.transform.rotation;
            snappedRotation.eulerAngles = new Vector3(0, 0, Editor.EditorUtils.Snap(snappedRotation.eulerAngles.z, 180));

            var scaleDelta = Vector3.Scale(snappedRotation * mouseDelta, new Vector3(scaleMulti.x, scaleMulti.y, 0));
            var positionDelta = this.transform.rotation * Vector3.Scale(snappedRotation * mouseDelta, new Vector3(Mathf.Abs(scaleMulti.x), Mathf.Abs(scaleMulti.y), 0));

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
            this.transform.position += positionDelta * 0.5f;
            return true;
        }
    }
}
