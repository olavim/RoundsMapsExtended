using System;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public class Toolbar : MonoBehaviour
	{
		private const float GridStep = 0.25f;

		public Menu fileMenu;
		public Menu editMenu;
		public Menu mapObjectMenu;
		public Menu windowMenu;
		public Button simulateButton;
		public Slider gridSizeSlider;
		public Text gridSizeValueLabel;

		public Action<bool> onToggleSimulation;

		private Menu _activeMenu;
		private bool _simulationEnabled;

		protected virtual void Start()
		{
			this.SetGridSize(GridStep);
			this._simulationEnabled = false;

			var menus = new Menu[] { this.fileMenu, this.editMenu, this.mapObjectMenu, this.windowMenu };

			foreach (var menu in menus)
			{
				menu.onOpen += () => this._activeMenu = menu;
				menu.onClose += () => this._activeMenu = this._activeMenu == menu ? null : this._activeMenu;
				menu.onHighlight += () =>
				{
					if (this._activeMenu != null)
					{
						if (this._activeMenu.state == Menu.MenuState.ACTIVE)
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

			this.gridSizeSlider.onValueChanged.AddListener(val => this.SetGridSize(val));

			this.simulateButton.onClick.AddListener(() =>
			{
				this._simulationEnabled = !this._simulationEnabled;
				this.onToggleSimulation?.Invoke(this._simulationEnabled);
			});
		}

		private void SetGridSize(float val)
		{
			this.gridSizeSlider.value = Mathf.Round(val / GridStep) * GridStep;
			this.gridSizeSlider.value = Math.Max(GridStep, this.gridSizeSlider.value);
			this.gridSizeValueLabel.text = this.gridSizeSlider.value.ToString("F2");
		}
	}

}
