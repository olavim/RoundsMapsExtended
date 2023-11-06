using MapsExt.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt
{
	public sealed class CameraHandler : MonoBehaviour
	{
		private CameraZoomHandler _zoomHandler;

		private void Awake()
		{
			this._zoomHandler = this.GetComponent<CameraZoomHandler>();
		}

		private void OnEnable()
		{
			if (this._zoomHandler != null)
			{
				this._zoomHandler.enabled = false;
			}
		}

		private void OnDisable()
		{
			if (this._zoomHandler != null)
			{
				this._zoomHandler.enabled = true;
			}
		}

		private void Update()
		{
			if (MapManager.instance.currentMap == null)
			{
				// Not in a map; use default zoom
				this.LerpOrthographicSize(20);
				this.LerpPosition(Vector2.zero);
				return;
			}

			var customMap = MapManager.instance.GetCurrentCustomMap();

			if (customMap == null)
			{
				// Not in a MapsExtended map; use the current map's size
				this.LerpOrthographicSize(MapManager.instance.currentMap.Map.size);
				this.LerpPosition(Vector2.zero);
				return;
			}

			// Calculate the size and position of the camera based on map settings and player positions
			float aspect = MainCam.instance.cam.aspect;

			var mapSize = customMap.Settings.MapSize;
			var mapSizeWorld = ConversionUtils.ScreenToWorldUnits(mapSize);
			var mapBounds = new Bounds(Vector2.zero, mapSizeWorld);

			var viewportSize = new Vector2(customMap.Settings.ViewportHeight * aspect, customMap.Settings.ViewportHeight);
			var viewportSizeWorld = ConversionUtils.ScreenToWorldUnits(viewportSize);

			var viewportMinCenter = (Vector2) mapBounds.min + viewportSizeWorld * 0.5f;
			var viewportMaxCenter = (Vector2) mapBounds.max - viewportSizeWorld * 0.5f;

			/* The camera should follow the player and have a constant size. We make an exception when there are multiple local players,
			 * in which case we want the camera to zoom out enough to fit all local players in the viewport at once.
			 */
			var viewportBoundsArr = new List<Bounds>();
			foreach (var player in PlayerManager.instance.players)
			{
				if (player.data.view.IsMine)
				{
					float posX = viewportSize.x >= mapSize.x ? 0 : Mathf.Clamp(player.transform.position.x, viewportMinCenter.x, viewportMaxCenter.x);
					float posY = viewportSize.y >= mapSize.y ? 0 : Mathf.Clamp(player.transform.position.y, viewportMinCenter.y, viewportMaxCenter.y);
					var bounds = new Bounds(new Vector2(posX, posY), viewportSizeWorld);
					viewportBoundsArr.Add(bounds);
				}
			}

			var viewportBounds = viewportBoundsArr.Count > 0 ? viewportBoundsArr[0] : new Bounds(Vector2.zero, viewportSizeWorld);
			for (int i = 1; i < viewportBoundsArr.Count; i++)
			{
				viewportBounds.Encapsulate(viewportBoundsArr[i]);
			}

			float targetViewportHeight = Mathf.Max(viewportBounds.size.y, viewportBounds.size.x / aspect);
			var targetViewportSize = new Vector2(targetViewportHeight * aspect, targetViewportHeight);

			this.LerpOrthographicSize(targetViewportSize.y * 0.5f);

			var targetViewportMinCenter = (Vector2) mapBounds.min + targetViewportSize * 0.5f;
			var targetViewportMaxCenter = (Vector2) mapBounds.max - targetViewportSize * 0.5f;
			float viewPosX = targetViewportSize.x >= mapSizeWorld.x ? 0 : Mathf.Clamp(viewportBounds.center.x, targetViewportMinCenter.x, targetViewportMaxCenter.x);
			float viewPosY = targetViewportSize.y >= mapSizeWorld.y ? 0 : Mathf.Clamp(viewportBounds.center.y, targetViewportMinCenter.y, targetViewportMaxCenter.y);
			this.LerpPosition(new Vector2(viewPosX, viewPosY));
		}

		private void LerpOrthographicSize(float size)
		{
			foreach (var cam in this.GetComponentsInChildren<Camera>())
			{
				cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, size, Time.unscaledDeltaTime * 5f);
			}
		}

		private void LerpPosition(Vector2 position)
		{
			foreach (var cam in this.GetComponentsInChildren<Camera>())
			{
				var targetPos = new Vector3(position.x, position.y, cam.transform.position.z);
				cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.unscaledDeltaTime * 5f);
			}
		}
	}
}
