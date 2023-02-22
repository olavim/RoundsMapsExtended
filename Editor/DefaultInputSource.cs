using UnityEngine;

namespace MapsExt.Editor
{
	public class DefaultInputSource : IInputSource
	{
		public Vector2 mouseScrollDelta => Input.mouseScrollDelta;
		public Vector3 mousePosition => Input.mousePosition;

		public bool GetMouseButtonDown(int button) => Input.GetMouseButtonDown(button);
		public bool GetMouseButtonUp(int button) => Input.GetMouseButtonUp(button);
		public bool GetMouseButton(int button) => Input.GetMouseButton(button);
		public bool GetKeyDown(KeyCode key) => Input.GetKeyDown(key);
		public bool GetKeyUp(KeyCode key) => Input.GetKeyUp(key);
		public bool GetKey(KeyCode key) => Input.GetKey(key);
	}
}
