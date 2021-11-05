using System;
using System.Runtime.CompilerServices;
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

		private static ConditionalWeakTable<MapObjet_Rope, ExtraRopeData> ropeData = new ConditionalWeakTable<MapObjet_Rope, ExtraRopeData>();

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
	}
}
