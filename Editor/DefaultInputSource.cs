using UnityEngine;

namespace MapsExt.Editor
{
	public class DefaultInputSource : IInputSource
	{
		public Vector2 mouseScrollDelta => Input.mouseScrollDelta;
		public Vector3 mousePosition => Input.mousePosition;

		public bool GetMouseButtonDown(int button)
		{
			return Input.GetMouseButtonDown(button);
		}

		public bool GetMouseButtonUp(int button)
		{
			return Input.GetMouseButtonUp(button);
		}

		public bool GetKeyDown(KeyCode key)
		{
			return Input.GetKeyDown(key);
		}

		public bool GetKeyUp(KeyCode key)
		{
			return Input.GetKeyUp(key);
		}

		public bool GetKey(KeyCode key)
		{
			return Input.GetKey(key);
		}
	}
}
