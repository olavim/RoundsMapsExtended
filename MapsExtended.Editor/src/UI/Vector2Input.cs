using UnityEngine.UI;
using UnityEngine;
using System;

namespace MapsExt.Editor.UI
{
	public class Vector2Input : MonoBehaviour
	{
		[SerializeField] private InputField _xInput;
		[SerializeField] private InputField _yInput;
		private Vector2 _inputValue;

		public InputField XInput { get => this._xInput; set => this._xInput = value; }
		public InputField YInput { get => this._yInput; set => this._yInput = value; }

		public Action<Vector2> OnChanged { get; set; }

		public Vector2 Value
		{
			get => this._inputValue;
			set
			{
				this.SetWithoutEvent(value);
				this.OnChanged?.Invoke(value);
			}
		}

		protected virtual void Start()
		{
			this.XInput.onValueChanged.AddListener(this.UpdateXValue);
			this.YInput.onValueChanged.AddListener(this.UpdateYValue);
		}

		private void UpdateXValue(string valueStr)
		{
			if (valueStr.EndsWith("."))
			{
				return;
			}

			if (valueStr?.Length == 0)
			{
				this._inputValue = new Vector2(0, this._inputValue.y);
			}
			else if (float.TryParse(valueStr, out float newX))
			{
				this._inputValue = new Vector2(newX, this._inputValue.y);
			}

			this.OnChanged?.Invoke(this.Value);
		}

		private void UpdateYValue(string valueStr)
		{
			if (valueStr.EndsWith("."))
			{
				return;
			}

			if (valueStr == "")
			{
				this._inputValue = new Vector2(this._inputValue.x, 0);
			}
			else if (float.TryParse(valueStr, out float newY))
			{
				this._inputValue = new Vector2(this._inputValue.x, newY);
			}

			this.OnChanged?.Invoke(this.Value);
		}

		public void SetWithoutEvent(Vector2 value)
		{
			this.XInput.onValueChanged.RemoveListener(this.UpdateXValue);
			this.YInput.onValueChanged.RemoveListener(this.UpdateYValue);
			this.XInput.text = value.x.ToString();
			this.YInput.text = value.y.ToString();
			this._inputValue = value;
			this.XInput.onValueChanged.AddListener(this.UpdateXValue);
			this.YInput.onValueChanged.AddListener(this.UpdateYValue);
		}

		public void SetEnabled(bool enabled)
		{
			this.XInput.interactable = enabled;
			this.YInput.interactable = enabled;

			var col = enabled ? Color.white : new Color(0.78f, 0.78f, 0.78f, 0.4f);
			foreach (var text in this.gameObject.GetComponentsInChildren<Text>())
			{
				text.color = col;
			}
		}
	}
}
