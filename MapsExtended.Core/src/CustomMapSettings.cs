using UnityEngine;

namespace MapsExt
{
	public class CustomMapSettings
	{
		private static readonly Vector2 s_defaultMapSize = new(1920, 1080);
		private static readonly int s_defaultViewportSize = 1080;

		public static Vector2 DefaultMapSize => s_defaultMapSize;
		public static int DefaultViewportHeight => s_defaultViewportSize;

		[SerializeField] private Vector2 _mapSize;
		[SerializeField] private int _viewportHeight;

		public Vector2 MapSize { get => this._mapSize; set => this._mapSize = value; }
		public int ViewportHeight { get => this._viewportHeight; set => this._viewportHeight = value; }

		public CustomMapSettings() : this(DefaultMapSize, DefaultViewportHeight) { }

		public CustomMapSettings(Vector2 mapSize, int viewportHeight)
		{
			this._mapSize = mapSize;
			this._viewportHeight = viewportHeight;
		}
	}
}
