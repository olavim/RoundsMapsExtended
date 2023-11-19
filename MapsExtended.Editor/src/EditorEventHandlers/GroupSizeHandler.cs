using MapsExt.Editor.MapObjects;
using MapsExt.Properties;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.Events
{
	[GroupEventHandler(typeof(MapObjectPartHandler))]
	public class GroupSizeHandler : SizeHandler
	{
		private IEnumerable<GameObject> _gameObjects;
		private float _aspect;
		private readonly Dictionary<GameObject, Vector2> _localScales = new();

		protected override bool ShouldHandleEvent(IEditorEvent evt)
		{
			return this.Editor.ActiveMapObjectPart == this.gameObject;
		}

		protected override void HandleEvent(IEditorEvent evt)
		{
			if (evt is SelectEvent)
			{
				this._gameObjects = this.Editor.SelectedMapObjectParts.ToList();

				var boundsArr = this._gameObjects.Select(obj => obj.GetComponent<MapObjectPart>().Collider.bounds).ToArray();
				var bounds = boundsArr[0];
				for (var i = 1; i < boundsArr.Length; i++)
				{
					bounds.Encapsulate(boundsArr[i]);
				}

				this._aspect = bounds.size.x / bounds.size.y;

				base.SetValue(bounds.size, Direction2D.Middle);

				foreach (var obj in this._gameObjects)
				{
					var scale = obj.GetComponent<SizeHandler>()?.GetValue().Value ?? new();
					this._localScales[obj] = new Vector2(scale.x / bounds.size.x, scale.y / bounds.size.y);
				}
			}

			base.HandleEvent(evt);
		}

		protected override void OnSelect()
		{
			this.AddResizeHandle(Direction2D.NorthEast);
			this.AddResizeHandle(Direction2D.NorthWest);
			this.AddResizeHandle(Direction2D.SouthEast);
			this.AddResizeHandle(Direction2D.SouthWest);
		}

		public override void SetValue(ScaleProperty size, Direction2D resizeDirection)
		{
			float sizeAspect = size.Value.x / size.Value.y;

			var newScale = sizeAspect >= this._aspect
				? new Vector2(size.Value.x, size.Value.x / this._aspect)
				: new Vector2(size.Value.y * this._aspect, size.Value.y);

			foreach (var obj in this._gameObjects)
			{
				obj.GetComponent<SizeHandler>()?.SetValue(Vector2.Scale(this._localScales[obj], this.GetValue().Value));
			}

			base.SetValue(newScale, resizeDirection);
		}
	}
}
