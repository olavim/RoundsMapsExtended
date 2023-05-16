using UnityEngine;
using UnityEngine.EventSystems;

namespace MapsExt.Editor
{
	public class EditorBaseInput : BaseInput
	{
		public override Vector2 mousePosition => EditorInput.MousePosition;
		public override Vector2 mouseScrollDelta => EditorInput.MouseScrollDelta;

		public override bool GetMouseButton(int button) => EditorInput.GetMouseButton(button);
		public override bool GetMouseButtonDown(int button) => EditorInput.GetMouseButtonDown(button);
		public override bool GetMouseButtonUp(int button) => EditorInput.GetMouseButtonUp(button);
	}
}
