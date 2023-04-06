using MapsExt.Properties;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(RotationHandler), typeof(PositionHandler))]
	public class GroupRotationHandler : RotationHandler, IGroupMapObjectActionHandler
	{
		private IEnumerable<GameObject> _gameObjects;

		private readonly Dictionary<GameObject, Vector2> _localPositions = new();
		private readonly Dictionary<GameObject, float> _localAngles = new();

		protected override void Awake()
		{
			base.Awake();
			this.RefreshLocalPositions();
			this.RefreshLocalAngles();
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

			this.GetComponent<GroupPositionHandler>()?.RefreshLocalPositions();
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this._gameObjects = gameObjects;
		}

		public void RefreshLocalPositions()
		{
			foreach (var obj in this._gameObjects)
			{
				this._localPositions[obj] = obj.GetHandlerValue<PositionProperty>() - (PositionProperty) this.transform.position;
			}
		}

		public void RefreshLocalAngles()
		{
			foreach (var obj in this._gameObjects)
			{
				this._localAngles[obj] = obj.GetHandlerValue<RotationProperty>().Value.eulerAngles.z;
			}
		}
	}
}
