using MapsExt.MapObjects.Properties;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(RotationHandler), typeof(PositionHandler))]
	public class GroupRotationHandler : RotationHandler, IGroupMapObjectActionHandler
	{
		private IEnumerable<GameObject> gameObjects;

		private readonly Dictionary<GameObject, Vector2> localPositions = new Dictionary<GameObject, Vector2>();
		private readonly Dictionary<GameObject, float> localAngles = new Dictionary<GameObject, float>();

		protected override void Awake()
		{
			base.Awake();

			foreach (var obj in this.gameObjects)
			{
				var posHandler = obj.GetComponent<PositionHandler>();
				var rotHandler = obj.GetComponent<RotationHandler>();

				this.localPositions[obj] = posHandler.GetValue() - (Vector2) this.transform.position;
				this.localAngles[obj] = rotHandler.GetValue().Value.eulerAngles.z;
			}
		}

		public override void SetValue(RotationProperty rotation)
		{
			foreach (var obj in this.gameObjects)
			{
				var posHandler = obj.GetComponent<PositionHandler>();
				var rotHandler = obj.GetComponent<RotationHandler>();

				posHandler.SetValue(this.transform.position + (rotation * localPositions[obj]).Round(4));
				rotHandler.SetValue(Quaternion.Euler(0, 0, localAngles[obj] + rotation.Value.eulerAngles.z));
			}
			this.transform.rotation = rotation;
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this.gameObjects = gameObjects;
		}
	}
}
