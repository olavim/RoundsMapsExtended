using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MapsExt
{
	public static class RoundsExtensions
	{
		private class ExtraRopeData
		{
			public Action<AnchoredJoint2D> ropeListener = _ => { };
		}

		internal class ExtraPlayerManagerData
		{
			public bool IsAnyPlayerBeingMoved => this.PlayersBeingMoved?.Any(p => p) ?? false;
			public bool[] PlayersBeingMoved { get; set; } = new bool[0];
		}

		private static readonly ConditionalWeakTable<MapObjet_Rope, ExtraRopeData> s_ropeData = new();
		private static readonly ConditionalWeakTable<PlayerManager, ExtraPlayerManagerData> s_pmData = new();

		internal static void OnJointAdded(this MapObjet_Rope instance, Action<AnchoredJoint2D> cb)
		{
			var data = s_ropeData.GetOrCreateValue(instance);
			data.ropeListener += cb;
		}

		internal static void JointAdded(this MapObjet_Rope instance, AnchoredJoint2D joint)
		{
			s_ropeData.GetOrCreateValue(instance).ropeListener(joint);
		}

		internal static ExtraPlayerManagerData GetExtraData(this PlayerManager instance)
		{
			return s_pmData.GetOrCreateValue(instance);
		}
	}
}
