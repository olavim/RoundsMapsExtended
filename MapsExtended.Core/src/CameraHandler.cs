using MapsExt.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnboundLib.GameModes;
using UnityEngine;

namespace MapsExt
{
	public sealed class CameraHandler : MonoBehaviour
	{
		public enum CameraMode
		{
			Static,
			FollowPlayer,
			Disabled
		}

		public static CameraMode Mode { get; set; } = CameraMode.FollowPlayer;
		public static float StaticZoom { get; set; } = 20f;

		private CameraZoomHandler _zoomHandler;
		private Camera[] _cameras;
		private float _refUpdateDelay = 0f;
		private Vector2 _targetPosition;
		private float _targetSize;
		private bool _isPickPhase = false;
		private bool _playersHaveSpawned = false;

		private void Awake()
		{
			this._zoomHandler = this.GetComponent<CameraZoomHandler>();
		}

		private void OnEnable()
		{
			GameModeManager.AddHook(GameModeHooks.HookGameStart, this.OnGameStart);
			GameModeManager.AddHook(GameModeHooks.HookPickStart, this.OnPickStart);
			GameModeManager.AddHook(GameModeHooks.HookPickEnd, this.OnPickEnd);
			GameModeManager.AddHook(GameModeHooks.HookPointStart, this.OnPlayersActive);
			GameModeManager.AddHook(GameModeHooks.HookPointEnd, this.OnPlayersInactive);
			GameModeManager.AddHook(GameModeHooks.HookRoundStart, this.OnPlayersActive);
			GameModeManager.AddHook(GameModeHooks.HookRoundEnd, this.OnPlayersInactive);

			if (this._zoomHandler != null)
			{
				this._zoomHandler.enabled = false;
			}
		}

		private void OnDisable()
		{
			GameModeManager.RemoveHook(GameModeHooks.HookGameStart, this.OnGameStart);
			GameModeManager.RemoveHook(GameModeHooks.HookPickStart, this.OnPickStart);
			GameModeManager.RemoveHook(GameModeHooks.HookPickEnd, this.OnPickEnd);
			GameModeManager.RemoveHook(GameModeHooks.HookPointStart, this.OnPlayersActive);
			GameModeManager.RemoveHook(GameModeHooks.HookPointEnd, this.OnPlayersInactive);
			GameModeManager.RemoveHook(GameModeHooks.HookRoundStart, this.OnPlayersActive);
			GameModeManager.RemoveHook(GameModeHooks.HookRoundEnd, this.OnPlayersInactive);

			if (this._zoomHandler != null)
			{
				this._zoomHandler.enabled = true;
			}
		}

		private IEnumerator OnGameStart(IGameModeHandler gm)
		{
			this.UpdateTargets();
			this.ForceTargetPosition();
			this.ForceTargetSize();
			yield break;
		}

		private IEnumerator OnPickStart(IGameModeHandler gm)
		{
			this._isPickPhase = true;
			this.UpdateTargets();
			this.ForceTargetPosition();
			this.ForceTargetSize();
			yield break;
		}

		private IEnumerator OnPickEnd(IGameModeHandler gm)
		{
			this._isPickPhase = false;
			yield break;
		}

		private IEnumerator OnPlayersActive(IGameModeHandler gm)
		{
			this._playersHaveSpawned = true;
			yield break;
		}

		private IEnumerator OnPlayersInactive(IGameModeHandler gm)
		{
			this._playersHaveSpawned = false;
			yield break;
		}

		private void Update()
		{
			this._zoomHandler.enabled = Mode == CameraMode.Disabled && !this._isPickPhase;

			this._refUpdateDelay -= Time.deltaTime;
			if (this._refUpdateDelay <= 0f)
			{
				this._cameras = this.GetComponentsInChildren<Camera>();
				this._refUpdateDelay = 1f;
			}

			if (Mode == CameraMode.Disabled)
			{
				return;
			}

			this.UpdateTargets();
			this.LerpOrthographicSize(this._targetSize);
			if (Mode != CameraMode.Static)
			{
				this.LerpPosition(this._targetPosition);
			}
		}

		internal void UpdateTargets()
		{
			if (Mode == CameraMode.Static)
			{
				this._targetSize = StaticZoom;
				return;
			}

			if (MapManager.instance.currentMap == null || this._isPickPhase)
			{
				this._targetSize = 20f;
				this._targetPosition = Vector2.zero;
				return;
			}

			var customMap = MapManager.instance.GetCurrentCustomMap();

			if (customMap == null)
			{
				// Not in a MapsExtended map; use the current map's size
				this._targetSize = MapManager.instance.currentMap.Map.size;
				this._targetPosition = Vector2.zero;
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

			if (this._playersHaveSpawned)
			{
				foreach (var player in PlayerManager.instance.players.Where(p => p?.data?.view?.IsMine == true))
				{
					float posX = viewportSize.x >= mapSize.x ? 0 : Mathf.Clamp(player.transform.position.x, viewportMinCenter.x, viewportMaxCenter.x);
					float posY = viewportSize.y >= mapSize.y ? 0 : Mathf.Clamp(player.transform.position.y, viewportMinCenter.y, viewportMaxCenter.y);
					var bounds = new Bounds(new Vector2(posX, posY), viewportSizeWorld);
					viewportBoundsArr.Add(bounds);
				}
			}

			float maxViewportHeight = Mathf.Max(mapBounds.size.y, mapBounds.size.x / aspect);
			var maxViewportSize = new Vector2(maxViewportHeight * aspect, maxViewportHeight);
			var viewportBounds = viewportBoundsArr.Count > 0 ? viewportBoundsArr[0] : new Bounds(Vector2.zero, maxViewportSize);
			for (int i = 1; i < viewportBoundsArr.Count; i++)
			{
				viewportBounds.Encapsulate(viewportBoundsArr[i]);
			}

			float targetViewportHeight = Mathf.Max(viewportBounds.size.y, viewportBounds.size.x / aspect);
			var targetViewportSize = new Vector2(targetViewportHeight * aspect, targetViewportHeight);
			this._targetSize = targetViewportSize.y * 0.5f;

			var targetViewportMinCenter = (Vector2) mapBounds.min + targetViewportSize * 0.5f;
			var targetViewportMaxCenter = (Vector2) mapBounds.max - targetViewportSize * 0.5f;
			float viewPosX = targetViewportSize.x >= mapSizeWorld.x ? 0 : Mathf.Clamp(viewportBounds.center.x, targetViewportMinCenter.x, targetViewportMaxCenter.x);
			float viewPosY = targetViewportSize.y >= mapSizeWorld.y ? 0 : Mathf.Clamp(viewportBounds.center.y, targetViewportMinCenter.y, targetViewportMaxCenter.y);
			this._targetPosition = new Vector2(viewPosX, viewPosY);
		}

		private void LerpOrthographicSize(float size)
		{
			foreach (var cam in this._cameras.Where(c => c.gameObject.activeInHierarchy))
			{
				cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, size, Time.unscaledDeltaTime * 5f);
			}
		}

		private void LerpPosition(Vector2 position)
		{
			foreach (var cam in this._cameras.Where(c => c.gameObject.activeInHierarchy))
			{
				var targetPos = new Vector3(position.x, position.y, cam.transform.position.z);
				cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.unscaledDeltaTime * 5f);
			}
		}

		internal void ForceTargetPosition()
		{
			foreach (var cam in this._cameras.Where(c => c.gameObject.activeInHierarchy))
			{
				cam.transform.position = new Vector3(this._targetPosition.x, this._targetPosition.y, cam.transform.position.z);
			}
		}

		internal void ForceTargetSize()
		{
			foreach (var cam in this._cameras.Where(c => c.gameObject.activeInHierarchy))
			{
				cam.orthographicSize = this._targetSize;
			}
		}
	}
}
