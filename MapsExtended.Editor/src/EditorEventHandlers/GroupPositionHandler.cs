using MapsExt.Properties;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.Events
{
	[GroupEventHandler(typeof(PositionHandler), typeof(SelectionHandler))]
	public class GroupPositionHandler : PositionHandler
	{
		private IEnumerable<GameObject> _gameObjects;
		private readonly Dictionary<GameObject, Vector2> _localPositions = new();

		public override void SetValue(PositionProperty position)
		{
			base.SetValue(position);
			this.RefreshPositions();
		}

		public virtual void RefreshPositions()
		{
			foreach (var obj in this._gameObjects)
			{
				var newLocalPos = this.transform.rotation * this._localPositions[obj];
				obj.GetComponent<PositionHandler>().SetValue((this.transform.position + newLocalPos).Round(4));
			}
		}

		protected override bool ShouldHandleEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects) => true;

		protected override void HandleAcceptedEditorEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects)
		{
			UnityEngine.Debug.Log($"GroupPositionHandler: {evt.GetType().Name}");
			base.HandleAcceptedEditorEvent(evt, subjects);

			if (evt is SelectEvent)
			{
				this._gameObjects = this.Editor.SelectedObjects.ToList();

				var boundsArr = this._gameObjects.Select(obj => obj.GetComponent<SelectionHandler>().GetBounds()).ToArray();
				var bounds = boundsArr[0];
				for (var i = 1; i < boundsArr.Length; i++)
				{
					bounds.Encapsulate(boundsArr[i]);
				}

				this.transform.position = bounds.center;

				foreach (var obj in this._gameObjects)
				{
					this._localPositions[obj] = obj.GetComponent<PositionHandler>().GetValue() - (PositionProperty) bounds.center;
				}
			}
		}
	}
}
