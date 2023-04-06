using UnityEngine;

namespace MapsExt.Editor.UI
{
	public abstract class QuaternionElement : InspectorElement
	{
		private readonly string _name;
		private TextSliderInput _input;

		public abstract Quaternion Value { get; set; }

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
			this._input.OnChanged += this.HandleInputChange;
			return instance;
		}

		public override void OnUpdate()
		{
			this._input.SetWithoutEvent(this.Value.eulerAngles.z);
		}

		protected virtual void HandleInputChange(float angle, ChangeType changeType)
		{
			if (changeType == ChangeType.Change || changeType == ChangeType.ChangeEnd)
			{
				this.Value = Quaternion.Euler(0, 0, angle);
			}

			if (changeType == ChangeType.ChangeEnd)
			{
				this.Context.Editor.RefreshHandlers();
				this.Context.Editor.TakeSnaphot();
			}
		}
	}
}
