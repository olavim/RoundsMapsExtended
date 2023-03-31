using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public abstract class BooleanElement : InspectorElement
	{
		private readonly string _name;
		private Toggle _toggle;

		protected abstract bool Value { get; set; }

		protected BooleanElement(string name)
		{
			this._name = name;
		}

		protected override GameObject GetInstance()
		{
			var instance = GameObject.Instantiate(Assets.InspectorBooleanPrefab);
			var elem = instance.GetComponent<InspectorBoolean>();
			elem.Label.text = this._name;

			this._toggle = elem.Input;
			this._toggle.onValueChanged.AddListener(this.OnInputChange);

			return instance;
		}

		public override void OnUpdate()
		{
			this._toggle.onValueChanged.RemoveListener(this.OnInputChange);
			this._toggle.isOn = this.Value;
			this._toggle.onValueChanged.AddListener(this.OnInputChange);
		}

		protected virtual void OnInputChange(bool value)
		{
			this.Value = value;
			this.Context.Editor.TakeSnaphot();
		}
	}
}
