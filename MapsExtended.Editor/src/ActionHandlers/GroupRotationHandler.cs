using MapsExt.Properties;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(PositionHandler))]
	public class GroupRotationHandler : RotationHandler, IGroupMapObjectActionHandler
	{
		private IEnumerable<GameObject> _gameObjects;

		private readonly Dictionary<GameObject, RotationProperty> _localRotations = new();

		protected override void Awake()
		{
			base.Awake();
			this.gameObject.AddComponent<RotationPropertyInstance>();
		}

		public override void SetValue(RotationProperty rotation)
		{
			base.SetValue(rotation);
			this.RefreshRotations();
			this.GetComponent<GroupPositionHandler>()?.RefreshPositions();
		}

		public virtual void RefreshRotations()
		{
			foreach (var obj in this._gameObjects)
			{
				float newAngle = (this.GetValue() + this._localRotations[obj]) % 360;
				obj.TrySetHandlerValue<RotationProperty>(EditorUtils.Snap(newAngle, 1f));
			}
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this._gameObjects = gameObjects;

			foreach (var obj in this._gameObjects)
			{
				this._localRotations[obj] = obj.GetComponent<RotationHandler>()?.GetValue() ?? new();
			}
		}
	}
}
