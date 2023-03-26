using UnityEngine;

namespace MapsExt.Editor
{
	public static class EditorInput
	{
		public static readonly DefaultInputSource defaultInputSource = new DefaultInputSource();

		public static Vector2 mouseScrollDelta => EditorInput.inputSource.mouseScrollDelta;
		public static Vector2 mousePosition => EditorInput.inputSource.mousePosition;

		private static IInputSource inputSource = defaultInputSource;

		public static bool GetMouseButton(int button)
		{
			return EditorInput.inputSource.GetMouseButton(button);
		}

		public static bool GetMouseButtonDown(int button)
		{
			return EditorInput.inputSource.GetMouseButtonDown(button);
		}

		public static bool GetMouseButtonUp(int button)
		{
			return EditorInput.inputSource.GetMouseButtonUp(button);
		}

		public static bool GetKeyDown(KeyCode key)
		{
			return EditorInput.inputSource.GetKeyDown(key);
		}

		public static bool GetKeyUp(KeyCode key)
		{
			return EditorInput.inputSource.GetKeyUp(key);
		}

		public static bool GetKey(KeyCode key)
		{
			return EditorInput.inputSource.GetKey(key);
		}

		public static void SetInputSource(IInputSource source)
		{
			EditorInput.inputSource = source;
		}
	}
}
