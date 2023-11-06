using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MapsExt.Compatibility
{
	[CompatibilityPatch]
	public sealed class MapEmbiggenerCompatibilityPatch : ICompatibilityPatch, IPredicateDisabler<Scene>
	{
		private const string ModId = "pykess.rounds.plugins.mapembiggener";

		private bool _mapEmbiggenerEnabled = false;
		private Assembly _assembly;
		private Type _outOfBoundsUtilsType;
		private Type _outOfBoundsParticlesType;
		private GameObject _outOfBoundsUtilsBorder;

		private readonly List<Predicate<Scene>> _disableCases = new();

		public void Apply()
		{
			if (!Harmony.HasAnyPatches(ModId))
			{
				return;
			}

			this._mapEmbiggenerEnabled = true;
			this._assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => asm.GetName().Name == "MapEmbiggener");
			this._outOfBoundsUtilsType = this._assembly?.GetType("MapEmbiggener.OutOfBoundsUtils");
			this._outOfBoundsParticlesType = this._assembly?.GetType("MapEmbiggener.UI.OutOfBoundsParticles");

			this.AddDisableCase((scene) =>
			{
				var customMap = MapManager.instance.GetCurrentCustomMap();
				return customMap != null && !this.MapHasDefaultSizes(customMap);
			});

			SceneManager.sceneLoaded += this.OnSceneLoad;
		}

		public void AddDisableCase(Predicate<Scene> predicate)
		{
			this._disableCases.Add(predicate);
		}

		private void OnSceneLoad(Scene scene, LoadSceneMode mode)
		{
			if (scene.name == "Main")
			{
				return;
			}

			if (this._disableCases.Any(p => p(scene)))
			{
				this.DisableMapEmbiggener();
			}
			else
			{
				this.EnableMapEmbiggener();
			}
		}

		private bool MapHasDefaultSizes(CustomMap map)
		{
			return map.Settings.MapSize == CustomMapSettings.DefaultMapSize && map.Settings.ViewportHeight == CustomMapSettings.DefaultViewportHeight;
		}

		private void ExecuteAfterInit(Action action)
		{
			IEnumerator Execute()
			{
				while (this._outOfBoundsUtilsBorder == null)
				{
					this._outOfBoundsUtilsBorder = (GameObject) this._outOfBoundsUtilsType?.GetProperty("border").GetValue(null);
					yield return null;
				}

				action();
			}

			MapsExtended.Instance.StartCoroutine(Execute());
		}

		private void DisableMapEmbiggener()
		{
			if (!this._mapEmbiggenerEnabled)
			{
				return;
			}

			this.ExecuteAfterInit(() =>
			{
				Harmony.UnpatchID(ModId);
				this._outOfBoundsUtilsBorder.SetActive(false);
				foreach (var component in Resources.FindObjectsOfTypeAll(this._outOfBoundsParticlesType))
				{
					((MonoBehaviour) component).gameObject.SetActive(false);
				}

				this._mapEmbiggenerEnabled = false;
			});
		}

		private void EnableMapEmbiggener()
		{
			if (this._mapEmbiggenerEnabled)
			{
				return;
			}

			this.ExecuteAfterInit(() =>
			{
				new Harmony(ModId).PatchAll(this._assembly);
				this._outOfBoundsUtilsBorder?.SetActive(true);
				foreach (var component in Resources.FindObjectsOfTypeAll(this._outOfBoundsParticlesType))
				{
					((MonoBehaviour) component).gameObject.SetActive(true);
				}

				this._mapEmbiggenerEnabled = true;
			});
		}
	}
}
