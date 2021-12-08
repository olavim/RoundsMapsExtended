using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class SpawnActionHandler : EditorActionHandler
	{
		public override bool CanMove() => false;
		public override bool CanResize() => false;
		public override bool CanRotate() => false;

		public override bool Move(Vector3 positionDelta)
		{
			this.transform.position += positionDelta;
			return true;
		}

		public override bool Resize(Vector3 sizeDelta, int resizeDirection)
		{
			return false;
		}

		public override bool Rotate(Quaternion rotationDelta)
		{
			return false;
		}
	}
}
