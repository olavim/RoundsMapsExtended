using HarmonyLib;
using MapsExt.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnboundLib.GameModes;
using UnityEngine;

namespace MapsExt.Compatibility
{
	[CompatibilityPatch]
	public sealed class MapEmbiggenerCompatibilityPatch : ICompatibilityPatch, IPredicateDisabler<string>
	{
		internal const string ModId = "pykess.rounds.plugins.mapembiggener";

		private delegate void SceneLoadEvent(string sceneName);

		private static event SceneLoadEvent OnSceneLoad;

		[HarmonyPatch(typeof(MapManager), "RPCA_LoadLevel")]
		[HarmonyPriority(Priority.Low)]
		private static class MapEmbiggenerPatch_MapManager
		{
			public static void Prefix(string sceneName)
			{
				OnSceneLoad?.Invoke(sceneName);
			}
		}

		[HarmonyPatch(typeof(MapManager), "OnLevelFinishedLoading")]
		[HarmonyAfter(ModId)]
		private static class MapEmbiggenerPatch_MapManager_OnLevelFinishedLoading
		{
			static void Postfix(MapManager __instance, bool ___callInNextMap)
			{
				if (!___callInNextMap && __instance.currentMap.Map.transform.position.x < 90f)
				{
					__instance.currentMap.Map.transform.position = new Vector3(90f, __instance.currentMap.Map.transform.position.y, __instance.currentMap.Map.transform.position.z);
				}
			}
		}

		private Assembly _assembly;
		private Type _outOfBoundsUtilsType;
		private Type _outOfBoundsParticlesType;
		private Type _controllerManagerType;
		private GameObject _outOfBoundsUtilsBorder;
		private bool _embiggenerEnabled = true;

		private readonly List<Predicate<string>> _disableCases = new();

		public void Apply()
		{
			if (!Harmony.HasAnyPatches(ModId))
			{
				return;
			}

			this._assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == "MapEmbiggener");
			this._outOfBoundsUtilsType = this._assembly?.GetType("MapEmbiggener.OutOfBoundsUtils");
			this._outOfBoundsParticlesType = this._assembly?.GetType("MapEmbiggener.UI.OutOfBoundsParticles");
			this._controllerManagerType = this._assembly?.GetType("MapEmbiggener.Controllers.ControllerManager");

			this.AddDisableCase(sceneName => this.CurrentMapHasNonDefaultSizes());
			this.AddDisableCase(sceneName => this.CurrentMapHasAnimations());

			OnSceneLoad += this.HandleSceneLoad;
			this.DisableMapEmbiggener();
		}

		private bool CurrentMapHasNonDefaultSizes()
		{
			var map = MapManager.instance.GetCurrentCustomMap();
			return map != null && (map.Settings.MapSize != CustomMapSettings.DefaultMapSize || map.Settings.ViewportHeight != CustomMapSettings.DefaultViewportHeight);
		}

		private bool CurrentMapHasAnimations()
		{
			var map = MapManager.instance.GetCurrentCustomMap();
			return map != null && map.MapObjects.Any(obj => obj.GetProperty<AnimationProperty>() != null && obj.GetProperty<AnimationProperty>().Keyframes.Any());
		}

		public void AddDisableCase(Predicate<string> predicate)
		{
			this._disableCases.Add(predicate);
		}

		private void HandleSceneLoad(string sceneName)
		{
			if (sceneName == "Main")
			{
				return;
			}

			if (this._disableCases.Any(p => p(sceneName)))
			{
				this.DisableMapEmbiggener();
			}
			else
			{
				this.EnableMapEmbiggener();
			}
		}

		private IEnumerator ExecuteAfterInit(Action action)
		{
			while (this._outOfBoundsUtilsBorder == null)
			{
				this._outOfBoundsUtilsBorder = (GameObject) this._outOfBoundsUtilsType?.GetProperty("border").GetValue(null);
				yield return null;
			}

			action();
		}

		private void DisableMapEmbiggener()
		{
			MapsExtended.Log.LogInfo("Disabling MapEmbiggener");
			CameraHandler.Mode = CameraHandler.CameraMode.FollowPlayer;

			if (this._embiggenerEnabled)
			{
				Harmony.UnpatchID(ModId);
				GameModeManager.RemoveHook(GameModeHooks.HookPickStart, this.DisableMapEmbiggenerVisuals);
				GameModeManager.RemoveHook(GameModeHooks.HookPickEnd, this.EnableMapEmbiggenerVisuals);
				GameModeManager.RemoveHook(GameModeHooks.HookPointStart, this.EnableMapEmbiggenerVisuals);
				GameModeManager.RemoveHook(GameModeHooks.HookRoundStart, this.EnableMapEmbiggenerVisuals);
				GameModeManager.RemoveHook(GameModeHooks.HookRoundEnd, this.DisableMapEmbiggenerVisuals);
			}

			this._embiggenerEnabled = false;

			MapsExtended.Instance.StartCoroutine(this.DisableMapEmbiggenerVisuals(null));
		}

		private void EnableMapEmbiggener()
		{
			MapsExtended.Log.LogInfo("Enabling MapEmbiggener");
			CameraHandler.Mode = CameraHandler.CameraMode.Disabled;

			if (!this._embiggenerEnabled)
			{
				new Harmony(ModId).PatchAll(this._assembly);
				GameModeManager.AddHook(GameModeHooks.HookPickStart, this.DisableMapEmbiggenerVisuals);
				GameModeManager.AddHook(GameModeHooks.HookPickEnd, this.EnableMapEmbiggenerVisuals);
				GameModeManager.AddHook(GameModeHooks.HookPointStart, this.EnableMapEmbiggenerVisuals);
				GameModeManager.AddHook(GameModeHooks.HookRoundStart, this.EnableMapEmbiggenerVisuals);
				GameModeManager.AddHook(GameModeHooks.HookRoundEnd, this.DisableMapEmbiggenerVisuals);
			}

			this._embiggenerEnabled = true;
		}

		private IEnumerator EnableMapEmbiggenerVisuals(IGameModeHandler gm)
		{
			yield return this.ExecuteAfterInit(() =>
			{
				this._outOfBoundsUtilsBorder?.SetActive(true);
				foreach (var component in Resources.FindObjectsOfTypeAll(this._outOfBoundsParticlesType))
				{
					((MonoBehaviour) component).gameObject.SetActive(true);
				}
			});
		}

		private IEnumerator DisableMapEmbiggenerVisuals(IGameModeHandler gm)
		{
			yield return this.ExecuteAfterInit(() =>
			{
				this._outOfBoundsUtilsBorder.SetActive(false);
				foreach (var component in Resources.FindObjectsOfTypeAll(this._outOfBoundsParticlesType))
				{
					((MonoBehaviour) component).gameObject.SetActive(false);
				}
			});
		}
	}
}
