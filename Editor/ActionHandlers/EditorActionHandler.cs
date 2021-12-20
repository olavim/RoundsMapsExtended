using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public abstract class EditorActionHandler : MonoBehaviour
	{
		public int frameIndex = 0;

		public abstract bool CanMove();
		public abstract bool CanResize();
		public abstract bool CanRotate();
		public abstract bool Move(Vector3 positionDelta);
		public abstract bool Resize(Vector3 sizeDelta, int resizeDirection);
		public abstract bool SetRotation(Quaternion rotation);
	}
}
