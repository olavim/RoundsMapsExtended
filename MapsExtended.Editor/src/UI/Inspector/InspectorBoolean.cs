using UnityEngine.UI;
using UnityEngine;

namespace MapsExt.Editor.UI
{
	public class InspectorBoolean : MonoBehaviour
	{
		[SerializeField] private Text _label;
		[SerializeField] private Toggle _input;

		public Text Label { get => this._label; set => this._label = value; }
		public Toggle Input { get => this._input; set => this._input = value; }
	}
}
