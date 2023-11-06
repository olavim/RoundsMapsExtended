using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public abstract class BooleanElement : InspectorElement
	{
		private readonly string _name;
		private Toggle _toggle;

		public bool Value
		{
			get => this.GetValue();
			set => this.OnChange(value);
		}

		protected BooleanElement(string name)
		{
			this._name = name;
		}

		protected override GameObject GetInstance()
		{
#pragma warning disable IDE0002
			var instance = GameObject.Instantiate(Assets.InspectorToggleInputPrefab);
#pragma warning restore IDE0002
			var inspectorInput = instance.GetComponent<InspectorToggleInput>();
			inspectorInput.Label.text = this._name;

			this._toggle = inspectorInput.Input;
			this._toggle.onValueChanged.AddListener(this.OnChange);

			return instance;
		}

		public override void OnUpdate()
		{
			this._toggle.onValueChanged.RemoveListener(this.OnChange);
			this._toggle.isOn = this.Value;
			this._toggle.onValueChanged.AddListener(this.OnChange);
		}

		protected abstract bool GetValue();
		protected abstract void OnChange(bool value);
	}
}
