using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupMapObjectActionHandler(typeof(RotationHandler), typeof(PositionHandler))]
	public class GroupRotationHandler : RotationHandler, IGroupMapObjectActionHandler
	{
		private GameObject[] gameObjects;
		private Dictionary<GameObject, Vector3> localPositions;
		private Dictionary<GameObject, float> localAngles;

		public override void SetRotation(Quaternion rotation)
		{
			foreach (var obj in this.gameObjects)
			{
				var posHandler = obj.GetComponent<PositionHandler>();
				var rotHandler = obj.GetComponent<RotationHandler>();

				posHandler.SetPosition(this.transform.position + rotation * localPositions[obj]);
				rotHandler.SetRotation(Quaternion.Euler(0, 0, localAngles[obj] + rotation.eulerAngles.z));
			}
			this.transform.rotation = rotation;
		}

		public void SetHandlers(IEnumerable<GameObject> gameObjects)
		{
			this.gameObjects = gameObjects.ToArray();

			this.localPositions = new Dictionary<GameObject, Vector3>();
			this.localAngles = new Dictionary<GameObject, float>();
			foreach (var obj in this.gameObjects)
			{
				var posHandler = obj.GetComponent<PositionHandler>();
				var rotHandler = obj.GetComponent<RotationHandler>();

				this.localPositions[obj] = (Vector2) (posHandler.GetPosition() - this.transform.position);
				this.localAngles[obj] = rotHandler.GetRotation().eulerAngles.z;
			}
		}
	}
}
