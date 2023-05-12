using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapsExt.Editor
{
	public class MapEditorInputHandler : MonoBehaviour
	{
		private const float ClickTimeMsEpsilon = 500f;
		private const float ClickPositionEpsilon = 5f;

		private MapEditor _editor;
		private float _mouseDownSince;
		private Vector2 _mouseDownPosition;
		private bool _isSelecting;
		private bool _isMouseDown;

		protected virtual void Awake()
		{
			this._editor = this.gameObject.GetComponent<MapEditor>();
			this._mouseDownSince = 0;
			this._isSelecting = false;

			// The KeyMonitor component handles pressing and then holding keys in a more familiar way
			var monitor = this.gameObject.AddComponent<KeyMonitor>();

			var editorKeys = new KeyCode[] {
				KeyCode.LeftArrow,
				KeyCode.RightArrow,
				KeyCode.UpArrow,
				KeyCode.DownArrow
			};

			foreach (var key in editorKeys)
			{
				monitor.AddListener(key, () => this.HandleKeyDown(key));
			}
		}

		protected virtual void Update()
		{
			if (this._editor.IsSimulating)
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
				this._editor.ToggleSnapToGrid(false);
			}

			if (EditorInput.GetKeyUp(KeyCode.LeftShift))
			{
				this._editor.ToggleSnapToGrid(true);
			}

			if (EditorInput.GetKeyDown(KeyCode.Delete))
			{
				this.HandleDelete();
			}

			if (EditorInput.MouseScrollDelta.y > 0 || EditorInput.GetKeyDown(KeyCode.Plus) || EditorInput.GetKeyDown(KeyCode.KeypadPlus))
			{
				this.HandleZoom(1);
			}

			if (EditorInput.MouseScrollDelta.y < 0 || EditorInput.GetKeyDown(KeyCode.Minus) || EditorInput.GetKeyDown(KeyCode.KeypadMinus))
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
				this._editor.ZoomIn();
			}
			else
			{
				this._editor.ZoomOut();
			}
		}

		private void HandleDelete()
		{
			if (EventSystem.current.currentSelectedGameObject != null)
			{
				return;
			}

			this._editor.DeleteSelectedMapObjects();
		}

		private void HandleMouseDown()
		{
			if (EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}

			this._isMouseDown = true;
			this._mouseDownSince = Time.time * 1000;
			this._mouseDownPosition = EditorInput.MousePosition;

			var list = EditorUtils.GetEventHandlersAt(this._mouseDownPosition)
				.Select(h => h.gameObject)
				.Distinct();

			if (list.Any(this._editor.SelectedObjects.Contains))
			{
				this._editor.OnPointerDown();
			}
			else
			{
				this._isSelecting = true;
				this._editor.StartSelection();
			}
		}

		private void HandleMouseUp()
		{
			if (!this._isMouseDown)
			{
				return;
			}

			this._isMouseDown = false;

			this._editor.OnPointerUp();

			var mouseUpTime = Time.time * 1000;
			var newMousePosition = EditorInput.MousePosition;
			var mouseDelta = this._mouseDownPosition - newMousePosition;

			if (mouseDelta.magnitude <= ClickPositionEpsilon && mouseUpTime - this._mouseDownSince <= ClickTimeMsEpsilon)
			{
				this._editor.OnClickEventHandlers(EditorUtils.GetEventHandlersAt(newMousePosition));
			}

			if (this._isSelecting)
			{
				this._isSelecting = false;
				this._editor.EndSelection();
			}
		}

		private void HandleKeyDown(KeyCode key)
		{
			if (EventSystem.current.currentSelectedGameObject != null)
			{
				return;
			}

			this._editor.OnKeyDown(key);
		}
	}
}
