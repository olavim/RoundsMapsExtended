using UnityEngine;

namespace MapsExt.Editor.Commands
{
	public class RotateCommand : ICommand
	{
		public readonly Quaternion fromRotation;
		public readonly Quaternion toRotation;

		public RotateCommand(Quaternion fromRotation, Quaternion toRotation)
		{
			this.fromRotation = fromRotation;
			this.toRotation = toRotation;
		}
	}
}
