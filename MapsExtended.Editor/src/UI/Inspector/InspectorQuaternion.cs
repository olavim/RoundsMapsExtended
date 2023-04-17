using UnityEngine.UI;
using UnityEngine;

namespace MapsExt.Editor.UI
{
	public class InspectorQuaternion : MonoBehaviour
	{
		[SerializeField] private Text _label;
		[SerializeField] private TextSliderInput _input;

		public Text Label { get => this._label; set => this._label = value; }
		public TextSliderInput Input { get => this._input; set => this._input = value; }
	}
}
