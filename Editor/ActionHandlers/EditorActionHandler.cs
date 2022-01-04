using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public abstract class EditorActionHandler : MonoBehaviour
	{
		public int frameIndex = 0;

		public virtual bool CanResize => false;
		public virtual bool CanRotate => false;

		public virtual void Move(Vector3 positionDelta)
		{

		}

		public virtual void Resize(Vector3 sizeDelta, int resizeDirection)
		{

		}

		public virtual void SetRotation(Quaternion rotation)
		{

		}
	}
}
