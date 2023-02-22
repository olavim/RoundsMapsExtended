using UnityEngine;

namespace MapsExt.Editor
{
	public interface IInputSource
	{
		bool GetMouseButtonDown(int button);
		bool GetMouseButtonUp(int button);
		bool GetMouseButton(int button);
		bool GetKeyDown(KeyCode key);
		bool GetKeyUp(KeyCode key);
		bool GetKey(KeyCode key);

		Vector2 mouseScrollDelta { get; }
		Vector3 mousePosition { get; }
	}
}