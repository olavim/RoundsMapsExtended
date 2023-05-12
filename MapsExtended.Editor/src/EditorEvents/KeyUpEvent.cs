using UnityEngine;

namespace MapsExt.Editor.Events
{
	public class KeyUpEvent : IEditorEvent
	{
		public KeyCode Key { get; }

		public KeyUpEvent(KeyCode key)
		{
			this.Key = key;
		}
	}
}
