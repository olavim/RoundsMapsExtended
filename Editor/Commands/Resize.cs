using UnityEngine;

namespace MapsExt.Editor.Commands
{
	public class ResizeCommand : ICommand
	{
		public readonly Vector3 delta;
		public readonly int resizeDirection;

		public ResizeCommand(Vector3 from, Vector3 to) : this(to - from, 0) { }
		public ResizeCommand(Vector3 delta) : this(delta, 0) { }

		public ResizeCommand(Vector3 delta, int resizeDirection)
		{
			this.delta = delta;
			this.resizeDirection = resizeDirection;
		}
	}
}
