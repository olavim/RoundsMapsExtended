using MapsExt.Editor.Utils;
using MapsExt.Properties;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.Events
{
	[GroupEventHandler(typeof(MapObjectPartHandler), typeof(PositionHandler))]
	public class GroupRotationHandler : RotationHandler
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
		}

		public virtual void RefreshRotations()
		{
			foreach (var obj in this._gameObjects)
			{
				float newAngle = (this.GetValue() + this._localRotations[obj]) % 360;
				obj.GetComponent<RotationHandler>()?.SetValue(EditorUtils.Snap(newAngle, 1f));
			}
		}

		protected override void HandleEvent(IEditorEvent evt)
		{
			base.HandleEvent(evt);

			if (evt is SelectEvent)
			{
				this.OnSelect();

				this._gameObjects = this.Editor.SelectedMapObjectParts.ToList();

				foreach (var obj in this._gameObjects)
				{
					this._localRotations[obj] = obj.GetComponent<RotationHandler>()?.GetValue() ?? new();
				}
			}
		}
	}
}
