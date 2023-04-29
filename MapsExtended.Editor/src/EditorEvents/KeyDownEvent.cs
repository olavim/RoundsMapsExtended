using UnityEngine;

namespace MapsExt.Editor.Events
{
	public class KeyDownEvent : IEditorEvent
	{
		public KeyCode Key { get; }

		public KeyDownEvent(KeyCode key)
		{
			this.Key = key;
		}
	}
}
