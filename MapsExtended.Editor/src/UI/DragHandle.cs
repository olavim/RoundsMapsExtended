using UnityEngine;
using UnityEngine.EventSystems;

namespace MapsExt.Editor.UI
{
	public class DragHandle : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
	{
		public GameObject target;

		private Vector2 _offset;

		public void OnPointerDown(PointerEventData data)
		{
			data.useDragThreshold = false;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			this._offset = (Vector2) target.transform.position - EditorInput.MousePosition;
		}

		public void OnDrag(PointerEventData data)
		{
			this.target.transform.position = EditorInput.MousePosition + this._offset;
		}
	}
}
