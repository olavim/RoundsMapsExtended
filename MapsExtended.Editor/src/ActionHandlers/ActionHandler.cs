using MapsExt.MapObjects.Properties;
using System;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public abstract class ActionHandlerBase : MonoBehaviour, IActionHandler
	{
		public abstract Action OnChange { get; set; }
		public abstract IProperty GetValue();
		public abstract void SetValue(IProperty value);
		public abstract void OnSelect();
		public abstract void OnDeselect();
		public abstract void OnPointerDown();
		public abstract void OnPointerUp();
		public abstract void OnKeyDown(KeyCode key);
		public abstract void OnKeyUp(KeyCode key);
	}

	public abstract class ActionHandler : ActionHandlerBase
	{
		protected MapEditor Editor => this.GetComponentInParent<MapEditor>();
		public override Action OnChange { get; set; } = () => { };

		public sealed override IProperty GetValue() => this.GetValueInternal();

		protected virtual IProperty GetValueInternal() => null;
		public override void SetValue(IProperty value) { }

		public override void OnSelect() { }
		public override void OnDeselect() { }
		public override void OnPointerDown() { }
		public override void OnPointerUp() { }
		public override void OnKeyDown(KeyCode key) { }
		public override void OnKeyUp(KeyCode key) { }
	}

	public abstract class ActionHandler<T> : ActionHandler, IActionHandler<T> where T : IProperty
	{
		protected sealed override IProperty GetValueInternal() => this.GetValue();
		public sealed override void SetValue(IProperty value) => this.SetValue((T) value);

		public abstract new T GetValue();
		public abstract void SetValue(T value);
	}
}
