using UnityEngine;
using UnityEngine.EventSystems;

namespace MapsExt.Editor.UI
{
	public class DragHandle : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
	{
		public GameObject target;
		private Vector2 offset;

		public void OnPointerDown(PointerEventData data)
		{
			data.useDragThreshold = false;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			this.offset = (Vector2) target.transform.position - EditorInput.mousePosition;
		}

		public void OnDrag(PointerEventData data)
		{
			this.target.transform.position = EditorInput.mousePosition + this.offset;
		}
	}
}
