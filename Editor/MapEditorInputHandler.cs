using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapsExt.Editor
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

			if (EditorInput.GetMouseButtonDown(0))
			{
				this.HandleMouseDown();
			}

			if (EditorInput.GetMouseButtonUp(0))
			{
				this.HandleMouseUp();
			}

			if (EditorInput.GetKeyDown(KeyCode.LeftShift))
			{
				this.editor.OnToggleSnapToGrid(false);
			}

			if (EditorInput.GetKeyUp(KeyCode.LeftShift))
			{
				this.editor.OnToggleSnapToGrid(true);
			}

			if (EditorInput.GetKeyDown(KeyCode.Delete))
			{
				this.HandleDelete();
			}

			if (EditorInput.mouseScrollDelta.y > 0 || EditorInput.GetKeyDown(KeyCode.Plus) || EditorInput.GetKeyDown(KeyCode.KeypadPlus))
			{
				this.HandleZoom(1);
			}

			if (EditorInput.mouseScrollDelta.y < 0 || EditorInput.GetKeyDown(KeyCode.Minus) || EditorInput.GetKeyDown(KeyCode.KeypadMinus))
			{
				this.HandleZoom(-1);
			}
		}

		private void HandleZoom(int direction)
		{
			if (EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}

			if (direction > 0)
			{
				this.editor.OnZoomIn();
			}
			else
			{
				this.editor.OnZoomOut();
			}
		}

		private void HandleDelete()
		{
			if (EventSystem.current.currentSelectedGameObject != null)
			{
				return;
			}

			this.editor.OnDeleteSelectedMapObjects();
		}

		private void HandleNudge(Vector2 nudge)
		{
			if (EventSystem.current.currentSelectedGameObject != null)
			{
				return;
			}

			if (EditorInput.GetKey(KeyCode.LeftShift))
			{
				nudge *= 2f;
			}

			if (EditorInput.GetKey(KeyCode.LeftControl))
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
			this.mouseDownPosition = EditorInput.mousePosition;

			var list = EditorUtils.GetActionHandlersAt(this.mouseDownPosition).Select(h => h.gameObject).Distinct();

			if (list.Any(this.editor.IsSelected))
			{
				this.isDragging = true;
				this.editor.OnPointerDown();
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
			var newMousePosition = EditorInput.mousePosition;
			var mouseDelta = this.mouseDownPosition - newMousePosition;

			if (mouseDelta.magnitude <= this.clickPositionEpsilon && mouseUpTime - this.mouseDownSince <= this.clickTimeMsEpsilon)
			{
				this.editor.OnClickActionHandlers(EditorUtils.GetActionHandlersAt(newMousePosition));
			}

			if (this.isSelecting)
			{
				this.isSelecting = false;
				this.editor.OnSelectionEnd();
			}

			if (this.isDragging)
			{
				this.isDragging = false;
				this.editor.OnPointerUp();
			}
		}
	}
}
