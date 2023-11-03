using MapsExt.Editor;
using UnboundLib;
using UnityEngine;

namespace MapsExt.UI
{
	public class MapBorder : MonoBehaviour
	{
		private MapEditor _editor;
		private LineRenderer _lineRenderer;

		protected virtual void Start()
		{
			this._editor = this.GetComponentInParent<MapEditor>();
			this._lineRenderer = this.gameObject.GetOrAddComponent<LineRenderer>();

			this._lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
			this._lineRenderer.widthMultiplier = 0.2f;
			this._lineRenderer.positionCount = 4;
			this._lineRenderer.startColor = new Color(0.5f, 0.5f, 0.6f, 0.02f);
			this._lineRenderer.endColor = this._lineRenderer.startColor;
			this._lineRenderer.loop = true;
		}

		protected virtual void LateUpdate()
		{
			var mapSize = this._editor.MapSettings.MapSize;
			var mapSizeDiff = mapSize - CustomMapSettings.DefaultMapSize;
			var screenMin = -mapSizeDiff * 0.5f;
			var screenMax = mapSize - mapSizeDiff * 0.5f;
			var min = (MainCam.instance.cam.ScreenToWorldPoint(screenMin) - MainCam.instance.cam.transform.position) * (20f / MainCam.instance.cam.orthographicSize) - new Vector3(0.05f, 0.05f);
			var max = (MainCam.instance.cam.ScreenToWorldPoint(screenMax) - MainCam.instance.cam.transform.position) * (20f / MainCam.instance.cam.orthographicSize) + new Vector3(0.05f, 0.05f);

			var positions = new Vector3[]
				{
					new Vector3(min.x, min.y),
					new Vector3(max.x, min.y),
					new Vector3(max.x, max.y),
					new Vector3(min.x, max.y)
				};

			this._lineRenderer.SetPositions(positions);
		}
	}
}
