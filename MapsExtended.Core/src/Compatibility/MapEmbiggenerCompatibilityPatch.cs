using HarmonyLib;
using MapsExt.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnboundLib;
using UnboundLib.GameModes;
using UnityEngine;
using UnityEngine.SceneManagement;

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
		private static class MapEmbiggenerPatch_MapManager_OnLevelFinishedLoading
		{
			private static bool s_callInNextMap;

			static void Prefix(bool ___callInNextMap)
			{
				s_callInNextMap = ___callInNextMap;
			}

			[HarmonyBefore(ModId)]
			[HarmonyPostfix]
			static void PostfixBefore(ref bool ___callInNextMap)
			{
				___callInNextMap = s_callInNextMap;
			}

			[HarmonyAfter(ModId)]
			[HarmonyPostfix]
			static void PostfixAfter(MapManager __instance, ref bool ___callInNextMap)
			{
				if (!___callInNextMap && __instance.currentMap.Map.transform.position.x < 90f)
				{
					__instance.currentMap.Map.transform.position = new Vector3(90f, __instance.currentMap.Map.transform.position.y, __instance.currentMap.Map.transform.position.z);
				}

				___callInNextMap = false;
			}
		}

		private static Assembly s_assembly;
		private static Type s_outOfBoundsUtilsType;
		private static Type s_outOfBoundsParticlesType;
		private static Type s_controllerManagerType;
		private static GameObject s_outOfBoundsUtilsBorder;
		private static bool s_isMapEmbiggenerEnabled = true;

		private readonly List<Predicate<string>> _disableCases = new();

		public void Apply()
		{
			if (!Harmony.HasAnyPatches(ModId))
			{
				return;
			}

			s_assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == "MapEmbiggener");
			s_outOfBoundsUtilsType = s_assembly?.GetType("MapEmbiggener.OutOfBoundsUtils");
			s_outOfBoundsParticlesType = s_assembly?.GetType("MapEmbiggener.UI.OutOfBoundsParticles");
			s_controllerManagerType = s_assembly?.GetType("MapEmbiggener.Controllers.ControllerManager");

			this.AddDisableCase(sceneName => sceneName == "Main");
			this.AddDisableCase(sceneName => this.CurrentMapHasNonDefaultSizes());
			this.AddDisableCase(sceneName => this.CurrentMapHasAnimations());

			SceneManager.sceneLoaded += (scene, mode) =>
			{
				if (mode == LoadSceneMode.Single)
				{
					MapsExtended.Instance.ExecuteAfterFrames(1, () => OnSceneLoad?.Invoke(scene.name));
				}
			};

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
			while (s_outOfBoundsUtilsBorder == null)
			{
				s_outOfBoundsUtilsBorder = (GameObject) s_outOfBoundsUtilsType?.GetProperty("border").GetValue(null);
				yield return null;
			}

			action();
		}

		private void DisableMapEmbiggener()
		{
			MapsExtended.Log.LogInfo("Disabling MapEmbiggener");
			CameraHandler.Mode = CameraHandler.CameraMode.FollowPlayer;

			if (s_isMapEmbiggenerEnabled)
			{
				Harmony.UnpatchID(ModId);
				GameModeManager.RemoveHook(GameModeHooks.HookPickStart, this.DisableMapEmbiggenerVisuals);
				GameModeManager.RemoveHook(GameModeHooks.HookPickEnd, this.EnableMapEmbiggenerVisuals);
				GameModeManager.RemoveHook(GameModeHooks.HookPointStart, this.EnableMapEmbiggenerVisuals);
				GameModeManager.RemoveHook(GameModeHooks.HookRoundStart, this.EnableMapEmbiggenerVisuals);
				GameModeManager.RemoveHook(GameModeHooks.HookRoundEnd, this.DisableMapEmbiggenerVisuals);
			}

			s_isMapEmbiggenerEnabled = false;
			MapsExtended.Instance.StartCoroutine(this.DisableMapEmbiggenerVisuals());
		}

		private void EnableMapEmbiggener()
		{
			MapsExtended.Log.LogInfo("Enabling MapEmbiggener");
			CameraHandler.Mode = CameraHandler.CameraMode.Disabled;

			if (!s_isMapEmbiggenerEnabled)
			{
				new Harmony(ModId).PatchAll(s_assembly);
				GameModeManager.AddHook(GameModeHooks.HookPickStart, this.DisableMapEmbiggenerVisuals);
				GameModeManager.AddHook(GameModeHooks.HookPickEnd, this.EnableMapEmbiggenerVisuals);
				GameModeManager.AddHook(GameModeHooks.HookPointStart, this.EnableMapEmbiggenerVisuals);
				GameModeManager.AddHook(GameModeHooks.HookRoundStart, this.EnableMapEmbiggenerVisuals);
				GameModeManager.AddHook(GameModeHooks.HookRoundEnd, this.DisableMapEmbiggenerVisuals);
			}

			s_isMapEmbiggenerEnabled = true;

			if (GameModeManager.CurrentHandler?.Name == "Sandbox")
			{
				this.InvokeGameStartInSandbox();
			}
		}

		private IEnumerator EnableMapEmbiggenerVisuals(IGameModeHandler gm = null)
		{
			yield return this.ExecuteAfterInit(() =>
			{
				s_outOfBoundsUtilsBorder?.SetActive(true);
				foreach (var component in Resources.FindObjectsOfTypeAll(s_outOfBoundsParticlesType))
				{
					((MonoBehaviour) component).gameObject.SetActive(true);
				}
			});
		}

		private IEnumerator DisableMapEmbiggenerVisuals(IGameModeHandler gm = null)
		{
			yield return this.ExecuteAfterInit(() =>
			{
				s_outOfBoundsUtilsBorder.SetActive(false);
				foreach (var component in Resources.FindObjectsOfTypeAll(s_outOfBoundsParticlesType))
				{
					((MonoBehaviour) component).gameObject.SetActive(false);
				}
			});
		}

		private void InvokeGameStartInSandbox()
		{
			GameManager.instance.isPlaying = true;

			var currentController = AccessTools.Property(s_controllerManagerType, "CurrentMapController").GetValue(null);
			if (currentController != null)
			{
				var gameStartMethod = AccessTools.Method(currentController.GetType(), "OnGameStart");
				if (gameStartMethod != null)
				{
					var enumerator = (IEnumerator) gameStartMethod.Invoke(currentController, new object[] { GameModeManager.CurrentHandler });
					this.RunEnumerator(enumerator);
				}
			}

			var controllerManagerInstance = AccessTools.Field(s_controllerManagerType, "instance").GetValue(null);
			AccessTools.Method(s_controllerManagerType, "Update").Invoke(controllerManagerInstance, null);
		}

		private void RunEnumerator(IEnumerator enumerator)
		{
			var enumeratorStack = new Stack<IEnumerator>();
			enumeratorStack.Push(enumerator);

			while (enumeratorStack.Count > 0)
			{
				if (!enumeratorStack.Peek().MoveNext())
				{
					enumeratorStack.Pop();
					continue;
				}

				if (enumeratorStack.Peek().Current is IEnumerator enumerator2)
				{
					enumeratorStack.Push(enumerator2);
				}
			}
		}
	}
}
