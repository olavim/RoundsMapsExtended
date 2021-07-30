using UnityEngine;
using UnityEngine.EventSystems;

namespace MapsExtended.Editor
{
    public class MapEditorInputHandler : MonoBehaviour
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

            // The KeyMonitor component handles pressing and then holding keys in a more familiar way
            var monitor = this.gameObject.AddComponent<KeyMonitor>();
            monitor.AddListener(KeyCode.LeftArrow, () => this.HandleNudge(Vector2.left));
            monitor.AddListener(KeyCode.RightArrow, () => this.HandleNudge(Vector2.right));
            monitor.AddListener(KeyCode.UpArrow, () => this.HandleNudge(Vector2.up));
            monitor.AddListener(KeyCode.DownArrow, () => this.HandleNudge(Vector2.down));
        }

        public void Update()
        {
            if (this.editor.isSimulating)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                this.HandleMouseDown();
            }

            if (Input.GetMouseButtonUp(0))
            {
                this.HandleMouseUp();
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                this.editor.OnToggleSnapToGrid(false);
            }

            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                this.editor.OnToggleSnapToGrid(true);
            }

            if (Input.GetKeyDown(KeyCode.Delete))
            {
                this.editor.OnDeleteSelectedMapObjects();
            }
        }

        private void HandleNudge(Vector2 nudge)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                nudge *= 2f;
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                nudge /= 2f;
            }

            if (this.editor.GridSize > 0)
            {
                nudge *= this.editor.GridSize;
            }

            if (nudge.magnitude > 0)
            {
                this.editor.OnNudgeSelectedMapObjects(nudge);
            }
        }

        private void HandleMouseDown()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

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
