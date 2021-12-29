using System;
using System.Runtime.CompilerServices;
using System.Linq;
using UnityEngine;
using ExceptionDispatchInfo = System.Runtime.ExceptionServices.ExceptionDispatchInfo;

namespace MapsExt
{
	public static class ExtensionMethods
	{
		private class ExtraRopeData
		{
			public Action<AnchoredJoint2D> ropeListener = joint => { };
		}

		public class ExtraPlayerManagerData
		{
			public bool movingPlayers => this.movingPlayer != null && this.movingPlayer.Any(p => p);
			public bool[] movingPlayer;
		}

		private static ConditionalWeakTable<MapObjet_Rope, ExtraRopeData> ropeData = new ConditionalWeakTable<MapObjet_Rope, ExtraRopeData>();
		private static ConditionalWeakTable<PlayerManager, ExtraPlayerManagerData> pmData = new ConditionalWeakTable<PlayerManager, ExtraPlayerManagerData>();

		public static void Rethrow(this Exception ex)
		{
			ExceptionDispatchInfo.Capture(ex).Throw();
		}

		public static void OnJointAdded(this MapObjet_Rope instance, Action<AnchoredJoint2D> cb)
		{
			var data = ExtensionMethods.ropeData.GetOrCreateValue(instance);
			data.ropeListener += cb;
		}

		public static void JointAdded(this MapObjet_Rope instance, AnchoredJoint2D joint)
		{
			ExtensionMethods.ropeData.GetOrCreateValue(instance).ropeListener(joint);
		}

		public static ExtraPlayerManagerData GetExtraData(this PlayerManager instance)
		{
			return ExtensionMethods.pmData.GetOrCreateValue(instance);
		}
	}
}
