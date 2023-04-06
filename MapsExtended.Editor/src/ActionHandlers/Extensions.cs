using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public static class ActionHandlerExtensions
	{
		public static T GetHandlerValue<T>(this GameObject go) where T : IProperty
		{
			return go.GetComponent<IActionHandler<T>>().GetValue();
		}

		public static T GetHandlerValue<T>(this MonoBehaviour component) where T : IProperty
		{
			return component.GetComponent<IActionHandler<T>>().GetValue();
		}

		public static T GetHandlerValue<T>(this Transform transform) where T : IProperty
		{
			return transform.GetComponent<IActionHandler<T>>().GetValue();
		}

		public static void SetHandlerValue<T>(this GameObject go, T value) where T : IProperty
		{
			go.GetComponent<IActionHandler<T>>().SetValue(value);
		}

		public static void SetHandlerValue<T>(this MonoBehaviour component, T value) where T : IProperty
		{
			component.GetComponent<IActionHandler<T>>().SetValue(value);
		}

		public static void SetHandlerValue<T>(this Transform transform, T value) where T : IProperty
		{
			transform.GetComponent<IActionHandler<T>>().SetValue(value);
		}
	}
}
