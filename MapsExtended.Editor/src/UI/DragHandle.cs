using UnityEngine;
using UnityEngine.EventSystems;

namespace MapsExt.Editor.UI
{
	public class DragHandle : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
	{
		[SerializeField] private GameObject _target;
		private Vector2 _offset;

		public GameObject Target { get => this._target; set => this._target = value; }

		public void OnPointerDown(PointerEventData data)
		{
			data.useDragThreshold = false;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			this._offset = (Vector2) Target.transform.position - EditorInput.MousePosition;
		}

		public void OnDrag(PointerEventData data)
		{
			this.Target.transform.position = EditorInput.MousePosition + this._offset;
		}
	}
}
