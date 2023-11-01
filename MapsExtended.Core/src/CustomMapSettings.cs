using UnityEngine;

namespace MapsExt
{
	public class CustomMapSettings
	{
		public static readonly Vector2 DefaultMapSize = new(1920, 1080);
		public static readonly Vector2 DefaultViewportSize = new(1920, 1080);

		[SerializeField] private Vector2 _mapSize;
		[SerializeField] private Vector2 _viewportSize;

		public Vector2 MapSize { get => this._mapSize; set => this._mapSize = value; }
		public Vector2 ViewportSize { get => this._viewportSize; set => this._viewportSize = value; }

		public CustomMapSettings() : this(DefaultMapSize, DefaultViewportSize) { }

		public CustomMapSettings(Vector2 mapSize, Vector2 viewportSize)
		{
			this._mapSize = mapSize;
			this._viewportSize = viewportSize;
		}
	}
}
