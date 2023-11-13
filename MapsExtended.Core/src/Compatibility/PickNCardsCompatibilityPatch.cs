using HarmonyLib;
using System.Collections;
using UnboundLib.GameModes;

namespace MapsExt.Compatibility
{
	[CompatibilityPatch]
	public sealed class PickNCardsCompatibilityPatch : ICompatibilityPatch
	{
		private const string ModId = "pykess.rounds.plugins.pickncards";

		private CameraHandler.CameraMode _previousMode;

		public void Apply()
		{
			if (!Harmony.HasAnyPatches(ModId))
			{
				return;
			}

			GameModeManager.AddHook(GameModeHooks.HookPickStart, gm => this.PickStart());
			GameModeManager.AddHook(GameModeHooks.HookPickEnd, gm => this.PickEnd());
		}

		private IEnumerator PickStart()
		{
			this._previousMode = CameraHandler.Mode;
			CameraHandler.Mode = CameraHandler.CameraMode.Default;

			var handler = MainCam.instance.cam.GetComponentInParent<CameraHandler>();
			handler.UpdateTargets();
			handler.ForceTargetPosition();
			handler.ForceTargetSize();
			yield break;
		}

		private IEnumerator PickEnd()
		{
			CameraHandler.Mode = this._previousMode;
			yield break;
		}
	}
}
