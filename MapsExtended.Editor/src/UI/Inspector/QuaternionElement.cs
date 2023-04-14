using UnityEngine;

namespace MapsExt.Editor.UI
{
	public abstract class QuaternionElement : InspectorElement
	{
		private readonly string _name;
		private TextSliderInput _input;

		public Quaternion Value
		{
			get => this.GetValue();
			set => this.OnChange(value, ChangeType.ChangeEnd);
		}

		protected QuaternionElement(string name)
		{
			this._name = name;
		}

		protected override GameObject GetInstance()
		{
			var instance = GameObject.Instantiate(Assets.InspectorQuaternionPrefab);
			var quaternionInput = instance.GetComponent<InspectorQuaternion>();
			quaternionInput.Label.text = this._name;
			this._input = quaternionInput.Input;
			this._input.OnChanged += this.OnChange;
			return instance;
		}

		public override void OnUpdate()
		{
			this._input.SetWithoutEvent(this.Value.eulerAngles.z);
		}

		private void OnChange(float angle, ChangeType changeType)
		{
			this.OnChange(Quaternion.Euler(0, 0, angle), changeType);
		}

		protected abstract Quaternion GetValue();
		protected abstract void OnChange(Quaternion rotation, ChangeType changeType);
	}
}
