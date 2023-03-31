using System;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public class Toolbar : MonoBehaviour
	{
		private const float GridStep = 0.25f;

		[SerializeField] private Menu _fileMenu;
		[SerializeField] private Menu _editMenu;
		[SerializeField] private Menu _mapObjectMenu;
		[SerializeField] private Menu _windowMenu;
		[SerializeField] private Button _simulateButton;
		[SerializeField] private Slider _gridSizeSlider;
		[SerializeField] private Text _gridSizeValueLabel;
		private Menu _activeMenu;
		private bool _simulationEnabled;

		public Menu FileMenu { get => this._fileMenu; set => this._fileMenu = value; }
		public Menu EditMenu { get => this._editMenu; set => this._editMenu = value; }
		public Menu MapObjectMenu { get => this._mapObjectMenu; set => this._mapObjectMenu = value; }
		public Menu WindowMenu { get => this._windowMenu; set => this._windowMenu = value; }
		public Button SimulateButton { get => this._simulateButton; set => this._simulateButton = value; }
		public Slider GridSizeSlider { get => this._gridSizeSlider; set => this._gridSizeSlider = value; }
		public Text GridSizeValueLabel { get => this._gridSizeValueLabel; set => this._gridSizeValueLabel = value; }

		public Action<bool> OnToggleSimulation { get; set; }

		protected virtual void Start()
		{
			this.SetGridSize(GridStep);
			this._simulationEnabled = false;

			var menus = new Menu[] { this.FileMenu, this.EditMenu, this.MapObjectMenu, this.WindowMenu };

			foreach (var menu in menus)
			{
				menu.OnOpen += () => this._activeMenu = menu;
				menu.OnClose += () => this._activeMenu = this._activeMenu == menu ? null : this._activeMenu;
				menu.OnHighlight += () =>
				{
					if (this._activeMenu != null)
					{
						if (this._activeMenu.State == Menu.MenuState.ACTIVE)
						{
							this._activeMenu.SetState(Menu.MenuState.INACTIVE);
							menu.SetState(Menu.MenuState.ACTIVE);
						}
						else
						{
							this._activeMenu = null;
						}
					}
				};
			}

			this.GridSizeSlider.onValueChanged.AddListener(val => this.SetGridSize(val));

			this.SimulateButton.onClick.AddListener(() =>
			{
				this._simulationEnabled = !this._simulationEnabled;
				this.OnToggleSimulation?.Invoke(this._simulationEnabled);
			});
		}

		private void SetGridSize(float val)
		{
			this.GridSizeSlider.value = Mathf.Round(val / GridStep) * GridStep;
			this.GridSizeSlider.value = Math.Max(GridStep, this.GridSizeSlider.value);
			this.GridSizeValueLabel.text = this.GridSizeSlider.value.ToString("F2");
		}
	}

}
