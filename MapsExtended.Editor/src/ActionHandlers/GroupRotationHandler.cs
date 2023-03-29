using MapsExt.MapObjects.Properties;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(RotationHandler), typeof(PositionHandler))]
	public class GroupRotationHandler : RotationHandler, IGroupMapObjectActionHandler
	{
		private IEnumerable<GameObject> _gameObjects;

		private readonly Dictionary<GameObject, Vector2> _localPositions = new Dictionary<GameObject, Vector2>();
		private readonly Dictionary<GameObject, float> _localAngles = new Dictionary<GameObject, float>();

		protected override void Awake()
		{
			base.Awake();

			foreach (var obj in this._gameObjects)
			{
				var posHandler = obj.GetComponent<PositionHandler>();
				var rotHandler = obj.GetComponent<RotationHandler>();

				this._localPositions[obj] = posHandler.GetValue() - (Vector2) this.transform.position;
				this._localAngles[obj] = rotHandler.GetValue().Value.eulerAngles.z;
			}
		}

		public override void SetValue(RotationProperty rotation)
		{
			foreach (var obj in this._gameObjects)
			{
				var posHandler = obj.GetComponent<PositionHandler>();
				var rotHandler = obj.GetComponent<RotationHandler>();

				posHandler.SetValue(this.transform.position + (rotation * _localPositions[obj]).Round(4));
				rotHandler.SetValue(Quaternion.Euler(0, 0, _localAngles[obj] + rotation.Value.eulerAngles.z));
			}
			this.transform.rotation = rotation;
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this._gameObjects = gameObjects;
		}
	}
}
