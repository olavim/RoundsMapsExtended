using UnityEngine.UI;
using UnityEngine;
using System;

namespace MapsExt.UI
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
				this.xInput.onValueChanged.RemoveListener(this.UpdateXValue);
				this.yInput.onValueChanged.RemoveListener(this.UpdateYValue);
				this.xInput.text = value.x.ToString();
				this.yInput.text = value.y.ToString();
				this.inputValue = value;
				this.xInput.onValueChanged.AddListener(this.UpdateXValue);
				this.yInput.onValueChanged.AddListener(this.UpdateYValue);
			}
		}

		public void Start()
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

			if (valueStr == "")
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
	}
}
