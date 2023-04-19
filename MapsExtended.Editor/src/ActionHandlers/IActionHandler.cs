using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public interface IActionHandler
	{
		void OnSelect(bool inGroup);
		void OnDeselect();
		void OnPointerDown();
		void OnPointerUp();
		void OnKeyDown(KeyCode key);
		void OnKeyUp(KeyCode key);
	}
}
