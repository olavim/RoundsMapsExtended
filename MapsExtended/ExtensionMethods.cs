using System;
using System.Runtime.CompilerServices;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace MapsExt
{
	public static class ExtensionMethods
	{
		private class ExtraRopeData
		{
			public Action<AnchoredJoint2D> ropeListener = _ => { };
		}

		internal class ExtraPlayerManagerData
		{
			public bool MovingPlayers => this.movingPlayer?.Any(p => p) == true;
			public bool[] movingPlayer;
		}

		private static readonly ConditionalWeakTable<MapObjet_Rope, ExtraRopeData> ropeData = new ConditionalWeakTable<MapObjet_Rope, ExtraRopeData>();
		private static readonly ConditionalWeakTable<PlayerManager, ExtraPlayerManagerData> pmData = new ConditionalWeakTable<PlayerManager, ExtraPlayerManagerData>();

		internal static void OnJointAdded(this MapObjet_Rope instance, Action<AnchoredJoint2D> cb)
		{
			var data = ExtensionMethods.ropeData.GetOrCreateValue(instance);
			data.ropeListener += cb;
		}

		internal static void JointAdded(this MapObjet_Rope instance, AnchoredJoint2D joint)
		{
			ExtensionMethods.ropeData.GetOrCreateValue(instance).ropeListener(joint);
		}

		internal static ExtraPlayerManagerData GetExtraData(this PlayerManager instance)
		{
			return ExtensionMethods.pmData.GetOrCreateValue(instance);
		}

		public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
		{
			return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
		}

		public static Vector3 Round(this Vector3 vector, int decimalPlaces)
		{
			return new Vector3(
				(float) System.Math.Round(vector.x, decimalPlaces),
				(float) System.Math.Round(vector.y, decimalPlaces),
				(float) System.Math.Round(vector.z, decimalPlaces)
			);
		}

		public static Vector2 Round(this Vector2 vector, int decimalPlaces)
		{
			return new Vector2(
				(float) System.Math.Round(vector.x, decimalPlaces),
				(float) System.Math.Round(vector.y, decimalPlaces)
			);
		}

		public static object GetFieldOrPropertyValue(this MemberInfo info, object instance)
		{
			return
				info is FieldInfo field ? field.GetValue(instance) :
				info is PropertyInfo property ? property.GetValue(instance) :
				throw new ArgumentException("MemberInfo must be of type FieldInfo or PropertyInfo");
		}
	}
}
