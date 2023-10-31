using UnityEngine;

namespace MapsExt
{
	public class CustomMapSettings
	{
		[SerializeField] private readonly Vector2 _mapSize;
		[SerializeField] private readonly Vector2 _viewportSize;

		public Vector2 MapSize => this._mapSize;
		public Vector2 ViewportSize => this._viewportSize;

		public CustomMapSettings() : this(new Vector2(1920, 1080), new Vector2(1920, 1080)) { }

		public CustomMapSettings(Vector2 mapSize, Vector2 viewportSize)
		{
			this._mapSize = mapSize;
			this._viewportSize = viewportSize;
		}
	}
}
