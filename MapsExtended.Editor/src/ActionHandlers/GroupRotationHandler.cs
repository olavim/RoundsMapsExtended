using MapsExt.Properties;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(PositionHandler))]
	public class GroupRotationHandler : RotationHandler, IGroupMapObjectActionHandler
	{
		private IEnumerable<GameObject> _gameObjects;

		private readonly Dictionary<GameObject, Quaternion> _localRotations = new();

		public override void SetValue(RotationProperty rotation)
		{
			this.transform.rotation = rotation;
			this.RefreshRotations();
			this.GetComponent<GroupPositionHandler>()?.RefreshPositions();
		}

		public virtual void RefreshRotations()
		{
			foreach (var obj in this._gameObjects)
			{
				float newAngle = (this.transform.rotation * this._localRotations[obj]).eulerAngles.z % 360;
				obj.TrySetHandlerValue<RotationProperty>(EditorUtils.Snap(newAngle, 1f));
			}
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this._gameObjects = gameObjects;

			foreach (var obj in this._gameObjects)
			{
				this._localRotations[obj] = obj.GetComponent<RotationHandler>()?.GetValue() ?? Quaternion.identity;
			}
		}
	}
}
