using UnityEngine;

namespace MapsExt.Editor.Commands
{
	public class MoveCommand : ICommand
	{
		public readonly Vector3 delta;

		public MoveCommand(float x, float y) : this(x, y, 0) { }
		public MoveCommand(float x, float y, float z) : this(new Vector3(x, y, z)) { }
		public MoveCommand(Vector3 from, Vector3 to) : this(to - from) { }

		public MoveCommand(Vector3 delta)
		{
			this.delta = delta;
		}
	}
}
