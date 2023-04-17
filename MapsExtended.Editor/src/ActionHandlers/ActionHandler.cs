using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public abstract class ActionHandler : MonoBehaviour, IActionHandler
	{
		protected MapEditor Editor => this.GetComponentInParent<MapEditor>();

		public virtual void OnSelect() { }
		public virtual void OnDeselect() { }
		public virtual void OnRefresh() { }
		public virtual void OnPaste() { }
		public virtual void OnPointerDown() { }
		public virtual void OnPointerUp() { }
		public virtual void OnKeyDown(KeyCode key) { }
		public virtual void OnKeyUp(KeyCode key) { }
	}
}
