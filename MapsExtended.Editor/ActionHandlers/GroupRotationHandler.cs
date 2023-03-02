using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupMapObjectActionHandler(typeof(RotationHandler), typeof(PositionHandler))]
	public class GroupRotationHandler : RotationHandler, IGroupMapObjectActionHandler
	{
		public IEnumerable<GameObject> GameObjects { private get; set; }

		private readonly Dictionary<GameObject, Vector3> localPositions = new Dictionary<GameObject, Vector3>();
		private readonly Dictionary<GameObject, float> localAngles = new Dictionary<GameObject, float>();

		private void Start()
		{
			foreach (var obj in this.GameObjects)
			{
				var posHandler = obj.GetComponent<PositionHandler>();
				var rotHandler = obj.GetComponent<RotationHandler>();

				this.localPositions[obj] = (Vector2) (posHandler.GetPosition() - this.transform.position);
				this.localAngles[obj] = rotHandler.GetRotation().eulerAngles.z;
			}
		}

		public override void SetRotation(Quaternion rotation)
		{
			foreach (var obj in this.GameObjects)
			{
				var posHandler = obj.GetComponent<PositionHandler>();
				var rotHandler = obj.GetComponent<RotationHandler>();

				posHandler.SetPosition((this.transform.position + (rotation * localPositions[obj])).Round(4));
				rotHandler.SetRotation(Quaternion.Euler(0, 0, localAngles[obj] + rotation.eulerAngles.z));
			}
			this.transform.rotation = rotation;
		}
	}
}
