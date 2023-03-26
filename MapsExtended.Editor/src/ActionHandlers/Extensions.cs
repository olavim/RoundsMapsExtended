using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public static class ActionHandlerExtensions
	{
		public static T GetHandlerValue<T>(this GameObject go) where T : IMapObjectProperty
		{
			return go.GetComponent<IActionHandler<T>>().GetValue();
		}

		public static T GetHandlerValue<T>(this MonoBehaviour component) where T : IMapObjectProperty
		{
			return component.GetComponent<IActionHandler<T>>().GetValue();
		}

		public static void SetHandlerValue<T>(this GameObject go, T value) where T : IMapObjectProperty
		{
			go.GetComponent<IActionHandler<T>>().SetValue(value);
		}

		public static void SetHandlerValue<T>(this MonoBehaviour component, T value) where T : IMapObjectProperty
		{
			component.GetComponent<IActionHandler<T>>().SetValue(value);
		}
	}
}
