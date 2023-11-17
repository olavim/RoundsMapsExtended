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

				this.transform.localScale = bounds.size;

				foreach (var obj in this._gameObjects)
				{
					var scale = obj.GetComponent<SizeHandler>()?.GetValue().Value ?? new();
					this._localScales[obj] = new Vector2(scale.x / bounds.size.x, scale.y / bounds.size.y);
				}
			}

			base.HandleEvent(evt);
		}

		public override void SetValue(ScaleProperty size, Direction2D resizeDirection)
		{
			base.SetValue(size, resizeDirection);

			foreach (var obj in this._gameObjects)
			{
				UnityEngine.Debug.Log($"{this._localScales[obj]} * {this.GetValue().Value} = " + Vector2.Scale(this._localScales[obj], this.GetValue().Value));
				obj.GetComponent<SizeHandler>()?.SetValue(Vector2.Scale(this._localScales[obj], this.GetValue().Value));
			}
		}
	}
}
