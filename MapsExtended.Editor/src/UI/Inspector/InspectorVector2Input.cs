using UnityEngine.UI;
using UnityEngine;

namespace MapsExt.Editor.UI
{
	public class InspectorVector2Input : MonoBehaviour
	{
		[SerializeField] private Text _label;
		[SerializeField] private Vector2Input _input;

		public Text Label { get => this._label; set => this._label = value; }
		public Vector2Input Input { get => this._input; set => this._input = value; }
	}
}
