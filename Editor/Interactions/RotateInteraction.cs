using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MapsExt.Editor.Interactions
{
	public class RotateInteraction : MonoBehaviour, IEditorInteraction
	{
		private MapEditor editor;
		private bool isRotatingMapObjects;
		private Vector3 prevMouse;
		private GameObject content;

		private void Start()
		{
			this.editor = this.GetComponentInParent<MapEditor>();
			this.editor.selectedObjects.CollectionChanged += this.SelectedMapObjectsChanged;

			this.content = new GameObject("Rotate Interaction Content");
			this.content.transform.SetParent(this.transform);

			this.content.AddComponent<GraphicRaycaster>();
			var canvas = this.content.GetComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		}

		private void Update()
		{
			if (this.isRotatingMapObjects)
			{
				this.RotateMapObjects();
			}

			if (EditorInput.GetMouseButtonUp(0) && this.isRotatingMapObjects)
			{
				this.OnRotateEnd();
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
				this.AddRotationHandle(this.editor.selectedObjects[0]);
			}
		}

		private void OnRotateStart()
		{
			var mousePos = EditorInput.mousePosition;
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(mousePos.x, mousePos.y));

			this.isRotatingMapObjects = true;
			this.prevMouse = mouseWorldPos;
		}

		private void OnRotateEnd()
		{
			this.isRotatingMapObjects = false;
			this.editor.UpdateRopeAttachments();
			this.editor.TakeSnaphot();
		}

		private void RotateMapObjects()
		{
			var mouseWorldPos = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(EditorInput.mousePosition.x, EditorInput.mousePosition.y));
			var selectedObj = this.editor.selectedObjects[0];

			var mousePos = mouseWorldPos;
			var objectPos = selectedObj.transform.position;
			mousePos.x -= objectPos.x;
			mousePos.y -= objectPos.y;

			float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg - 90;
			angle = EditorUtils.Snap(angle, this.editor.snapToGrid ? 15f : 2f);
			var toRotation = Quaternion.AngleAxis(angle, Vector3.forward);

			foreach (var handler in this.editor.selectedObjects.SelectMany(obj => obj.GetComponents<ActionHandlers.RotationHandler>()))
			{
				handler.SetRotation(toRotation);
				handler.OnChange();
			}
		}

		private void AddRotationHandle(GameObject mapObject)
		{
			if (!mapObject.GetComponent<ActionHandlers.RotationHandler>())
			{
				return;
			}

			var go = new GameObject("Rotation Handle");

			var aligner = go.AddComponent<UI.UIAligner>();
			aligner.referenceGameObject = mapObject;
			aligner.position = AnchorPosition.TopMiddle;
			aligner.padding = 48f;

			var image = go.AddComponent<ProceduralImage>();
			image.rectTransform.sizeDelta = new Vector2(12f, 12f);

			var modifier = go.AddComponent<UniformModifier>();
			modifier.Radius = 6;

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
				if (!this.isRotatingMapObjects)
				{
					this.OnRotateStart();
				}
			};

			go.transform.SetParent(this.content.transform);
		}
	}
}
