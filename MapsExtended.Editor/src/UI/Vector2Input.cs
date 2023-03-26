using UnityEngine.UI;
using UnityEngine;
using System;

namespace MapsExt.Editor.UI
{
	public class Vector2Input : MonoBehaviour
	{
		public InputField xInput;
		public InputField yInput;
		public Action<Vector2> onChanged;

		private Vector2 inputValue;

		public Vector2 Value
		{
			get => this.inputValue;
			set
			{
				this.SetWithoutEvent(value);
				this.onChanged?.Invoke(value);
			}
		}

		protected virtual void Start()
		{
			this.xInput.onValueChanged.AddListener(this.UpdateXValue);
			this.yInput.onValueChanged.AddListener(this.UpdateYValue);
		}

		private void UpdateXValue(string valueStr)
		{
			if (valueStr.EndsWith("."))
			{
				return;
			}

			if (valueStr?.Length == 0)
			{
				this.inputValue = new Vector2(0, this.inputValue.y);
			}
			else if (float.TryParse(valueStr, out float newX))
			{
				this.inputValue = new Vector2(newX, this.inputValue.y);
			}

			this.onChanged?.Invoke(this.Value);
		}

		private void UpdateYValue(string valueStr)
		{
			if (valueStr.EndsWith("."))
			{
				return;
			}

			if (valueStr == "")
			{
				this.inputValue = new Vector2(this.inputValue.x, 0);
			}
			else if (float.TryParse(valueStr, out float newY))
			{
				this.inputValue = new Vector2(this.inputValue.x, newY);
			}

			this.onChanged?.Invoke(this.Value);
		}

		public void SetWithoutEvent(Vector2 value)
		{
			this.xInput.onValueChanged.RemoveListener(this.UpdateXValue);
			this.yInput.onValueChanged.RemoveListener(this.UpdateYValue);
			this.xInput.text = value.x.ToString();
			this.yInput.text = value.y.ToString();
			this.inputValue = value;
			this.xInput.onValueChanged.AddListener(this.UpdateXValue);
			this.yInput.onValueChanged.AddListener(this.UpdateYValue);
		}

		public void SetEnabled(bool enabled)
		{
			this.xInput.interactable = enabled;
			this.yInput.interactable = enabled;

			var col = enabled ? Color.white : new Color(0.78f, 0.78f, 0.78f, 0.4f);
			foreach (var text in this.gameObject.GetComponentsInChildren<Text>())
			{
				text.color = col;
			}
		}
	}
}
