using UnityEngine;

namespace MapEditor
{
    public class MapEditorEventHandler : MonoBehaviour
    {
        private readonly float clickTimeMsEpsilon = 500f;
        private readonly float clickPositionEpsilon = 5f;

        private MapEditor editor;
        private float mouseDownSince;
        private Vector3 mouseDownPosition;
        private bool isSelecting;
        private bool isDragging;

        public void Awake()
        {
            this.editor = this.gameObject.GetComponent<MapEditor>();
            this.mouseDownSince = 0;
            this.isSelecting = false;
            this.isDragging = false;
        }

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.HandleMouseDown();
            }

            if (Input.GetMouseButtonUp(0))
            {
                this.HandleMouseUp();
            }
        }

        private void HandleMouseDown()
        {
            this.mouseDownSince = Time.time * 1000;
            this.mouseDownPosition = Input.mousePosition;

            var list = EditorUtils.GetHoveredMapObjects();

            if (list.Exists(this.editor.IsMapObjectSelected))
            {
                this.isDragging = true;
                this.editor.OnDragStart();
            }
            else
            {
                this.isSelecting = true;
                this.editor.OnSelectionStart();
            }
        }

        private void HandleMouseUp()
        {
            var mouseUpTime = Time.time * 1000;
            var mouseDelta = this.mouseDownPosition - Input.mousePosition;

            if (mouseDelta.magnitude <= this.clickPositionEpsilon && mouseUpTime - this.mouseDownSince <= this.clickTimeMsEpsilon)
            {
                this.editor.OnClickMapObjects(EditorUtils.GetHoveredMapObjects());
            }

            if (this.isSelecting)
            {
                this.isSelecting = false;
                this.editor.OnSelectionEnd();
            }

            if (this.isDragging)
            {
                this.isDragging = false;
                this.editor.OnDragEnd();
            }
        }
    }
}
