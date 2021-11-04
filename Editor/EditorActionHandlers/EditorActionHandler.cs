using UnityEngine;
using System;

namespace MapsExt.Editor
{
	public abstract class EditorActionHandler : MonoBehaviour
	{
		public abstract bool CanRotate();

		public abstract bool CanResize(int resizeDirection);

		public abstract bool Resize(Vector3 sizeDelta, int resizeDirection);

		public Action onMove;
		public Action onResize;
		public Action onRotate;
	}
}
