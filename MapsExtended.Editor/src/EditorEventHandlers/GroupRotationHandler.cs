using MapsExt.Properties;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.Events
{
	[GroupEventHandler(typeof(PositionHandler))]
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
			this.GetComponent<GroupPositionHandler>()?.RefreshPositions();
		}

		public virtual void RefreshRotations()
		{
			foreach (var obj in this._gameObjects)
			{
				float newAngle = (this.GetValue() + this._localRotations[obj]) % 360;
				obj.GetComponent<RotationHandler>()?.SetValue(EditorUtils.Snap(newAngle, 1f));
			}
		}

		protected override bool ShouldHandleEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects) => true;

		protected override void HandleAcceptedEditorEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects)
		{
			base.HandleAcceptedEditorEvent(evt, subjects);

			if (evt is SelectEvent)
			{
				this.OnSelect();

				this._gameObjects = this.Editor.SelectedObjects.ToList();

				foreach (var obj in this._gameObjects)
				{
					this._localRotations[obj] = obj.GetComponent<RotationHandler>()?.GetValue() ?? new();
				}
			}
		}
	}
}
