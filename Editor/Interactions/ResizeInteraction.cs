using MapsExt.Editor.ActionHandlers;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.Interactions
{
	public class ResizeInteraction : MonoBehaviour, IEditorInteraction
	{
		private MapEditor editor;

		private bool isResizingMapObjects;
		private int resizeDirection;
		private Vector3 prevMouse;
		private Vector3Int prevCell;
		private GameObject content;

		private void Start()
		{
			this.editor = this.GetComponentInParent<MapEditor>();
			this.editor.selectedObjects.CollectionChanged += this.SelectedMapObjectsChanged;

			this.content = new GameObject("Resize Interaction Content");
			this.content.transform.SetParent(this.transform);

			this.content.AddComponent<GraphicRaycaster>();
			var canvas = this.content.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		}

		private void Update()
		{
			if (this.isResizingMapObjects)
			{
				this.ResizeMapObjects();
			}

			if (EditorInput.GetMouseButtonUp(0) && this.isResizingMapObjects)
			{
				this.OnResizeEnd();
			}
		}

		public void OnPointerDown() { }
		public void OnPointerUp() { }

		private void SelectedMapObjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			foreach (Transform child in this.content.transform)
			{
				GameObject.Destroy(child.gameObject);
			}

			if (this.editor.selectedObjects.Count == 1)
			{
				var obj = this.editor.selectedObjects[0];
				this.AddResizeHandle(obj, AnchorPosition.TopLeft);
				this.AddResizeHandle(obj, AnchorPosition.TopRight);
				this.AddResizeHandle(obj, AnchorPosition.BottomLeft);
				this.AddResizeHandle(obj, AnchorPosition.BottomRight);
				this.AddResizeHandle(obj, AnchorPosition.MiddleLeft);
				this.AddResizeHandle(obj, AnchorPosition.MiddleRight);
				this.AddResizeHandle(obj, AnchorPosition.BottomMiddle);
				this.AddResizeHandle(obj, AnchorPosition.TopMiddle);
			}
		}

		private void OnResizeStart(int resizeDirection)
		{
			var mousePos = EditorInput.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.editor.grid.transform.rotation = this.editor.selectedObjects[0].transform.rotation;

			this.isResizingMapObjects = true;
			this.resizeDirection = resizeDirection;
			this.prevMouse = mouseWorldPos;
			this.prevCell = this.editor.grid.WorldToCell(mouseWorldPos);
		}

		private void OnResizeEnd()
		{
			this.isResizingMapObjects = false;
			this.editor.UpdateRopeAttachments();
			this.editor.TakeSnaphot();
		}

		private void ResizeMapObjects()
		{
			var mousePos = EditorInput.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));
			var mouseCell = this.editor.grid.WorldToCell(mouseWorldPos);
			var mouseDelta = mouseWorldPos - this.prevMouse;
			Vector3 cellDelta = mouseCell - this.prevCell;

			var sizeDelta = this.editor.snapToGrid
				? cellDelta * this.editor.GridSize
				: Quaternion.Inverse(this.editor.grid.transform.rotation) * mouseDelta;

			if (sizeDelta != Vector3.zero)
			{
				foreach (var handler in this.editor.selectedObjects.SelectMany(obj => obj.GetComponents<SizeHandler>()))
				{
					handler.Resize(sizeDelta, this.resizeDirection);
					handler.OnChange();
				}

				this.prevMouse += mouseDelta;
				this.prevCell = mouseCell;
			}
		}

		private void AddResizeHandle(GameObject mapObject, int direction)
		{
			if (!mapObject.GetComponent<SizeHandler>())
			{
				return;
			}

			var go = new GameObject("Toggle");

			var aligner = go.AddComponent<UI.UIAligner>();
			aligner.referenceGameObject = mapObject;
			aligner.position = direction;

			var image = go.AddComponent<Image>();
			image.rectTransform.sizeDelta = new Vector2(10f, 10f);

			var button = go.AddComponent<Button>();
			button.colors = new ColorBlock()
			{
				colorMultiplier = 1,
				fadeDuration = 0.1f,
				normalColor = new Color(1, 1, 1),
				highlightedColor = new Color(0.8f, 0.8f, 0.8f),
				pressedColor = new Color(0.6f, 0.6f, 0.6f)
			};

			var events = go.AddComponent<PointerDownHandler>();

			events.pointerDown += hoveredObj =>
			{
				if (!this.isResizingMapObjects)
				{
					this.OnResizeStart(direction);
				}
			};

			go.transform.SetParent(this.content.transform);
		}
	}
}
