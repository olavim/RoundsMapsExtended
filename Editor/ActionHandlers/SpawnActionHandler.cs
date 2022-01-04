using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public class SpawnActionHandler : EditorActionHandler
	{
		public override void Move(Vector3 positionDelta)
		{
			this.transform.position += positionDelta;
		}
	}
}
