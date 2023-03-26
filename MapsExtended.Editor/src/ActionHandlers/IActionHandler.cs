using MapsExt.MapObjects.Properties;
using System;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public interface IActionHandler
	{
		Action OnChange { get; set; }

		IMapObjectProperty GetValue();
		void SetValue(IMapObjectProperty value);
		void OnSelect();
		void OnDeselect();
		void OnPointerDown();
		void OnPointerUp();
		void OnKeyDown(KeyCode key);
		void OnKeyUp(KeyCode key);
	}

	public interface IActionHandler<T> : IActionHandler
	{
		new T GetValue();
		void SetValue(T value);
	}
}
