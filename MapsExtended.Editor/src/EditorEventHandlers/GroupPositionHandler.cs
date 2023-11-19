using MapsExt.Editor.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.Events
{
	[GroupEventHandler(typeof(PositionHandler))]
	public class GroupPositionHandler : PositionHandler
	{
		private bool _refreshPositionsNextFrame;
		private IEnumerable<GameObject> _gameObjects;
		private readonly Dictionary<GameObject, Vector2> _localPositions = new();

		override protected void Awake()
		{
			base.Awake();

			foreach (var handler in this.GetComponents<ITransformModifyingEditorEventHandler>())
			{
				// Also adds listener to itself
				TransformChangedEventHandler h = null;
				handler.OnTransformChanged += h = () =>
				{
					this.RefreshLocalPositions();
					handler.OnTransformChanged -= h;
					handler.OnTransformChanged += () => this._refreshPositionsNextFrame = true;
				};
			}
		}

		protected override void Update()
		{
			base.Update();

			if (this._refreshPositionsNextFrame)
			{
				this._refreshPositionsNextFrame = false;
				this.RefreshPositions();
			}
		}

		public void RefreshPositions()
		{
			foreach (var obj in this._gameObjects)
			{
				this.RefreshPosition(obj);
			}
		}

		public virtual void RefreshPosition(GameObject obj)
		{
			if (!this._localPositions.ContainsKey(obj))
			{
				throw new ArgumentException("Object is not part of the group", nameof(obj));
			}

			obj.GetComponent<PositionHandler>().SetValue(this.transform.TransformPoint(this._localPositions[obj]).Round(4));
		}

		public virtual Vector2 GetLocalPosition(GameObject obj)
		{
			if (!this._localPositions.ContainsKey(obj))
			{
				throw new ArgumentException("Object is not part of the group", nameof(obj));
			}

			return this._localPositions[obj];
		}

		public virtual void RefreshLocalPositions()
		{
			foreach (var obj in this._gameObjects)
			{
				this._localPositions[obj] = this.transform.InverseTransformPoint(obj.GetComponent<PositionHandler>().GetValue());
			}
		}

		protected override void HandleEvent(IEditorEvent evt)
		{
			base.HandleEvent(evt);

			if (evt is SelectEvent)
			{
				this._gameObjects = this.Editor.SelectedMapObjectParts.ToList();

				var boundsArr = this._gameObjects.Select(obj => obj.GetComponent<MapObjectPart>().Collider.bounds).ToArray();
				var bounds = boundsArr[0];
				for (var i = 1; i < boundsArr.Length; i++)
				{
					bounds.Encapsulate(boundsArr[i]);
				}

				this.SetValue(bounds.center);
			}
		}
	}
}
