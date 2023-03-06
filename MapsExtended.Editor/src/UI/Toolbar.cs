using System;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.UI
{
	public class Toolbar : MonoBehaviour
	{
		private static readonly float gridStep = 0.25f;

		public Menu fileMenu;
		public Menu editMenu;
		public Menu mapObjectMenu;
		public Menu windowMenu;
		public Button simulateButton;
		public Slider gridSizeSlider;
		public Text gridSizeValueLabel;

		public Action<bool> onToggleSimulation;

		private Menu activeMenu;
		private bool simulationEnabled;

		private void Start()
		{
			this.SetGridSize(gridStep);
			this.simulationEnabled = false;

			var menus = new Menu[] { this.fileMenu, this.editMenu, this.mapObjectMenu, this.windowMenu };

			foreach (var menu in menus)
			{
				menu.onOpen += () => this.activeMenu = menu;
				menu.onClose += () => this.activeMenu = this.activeMenu == menu ? null : this.activeMenu;
				menu.onHighlight += () =>
				{
					if (this.activeMenu != null)
					{
						if (this.activeMenu.state == Menu.MenuState.ACTIVE)
						{
							this.activeMenu.SetState(Menu.MenuState.INACTIVE);
							menu.SetState(Menu.MenuState.ACTIVE);
						}
						else
						{
							this.activeMenu = null;
						}
					}
				};
			}

			this.gridSizeSlider.onValueChanged.AddListener(val =>
			{
				this.SetGridSize(val);
			});

			this.simulateButton.onClick.AddListener(() =>
			{
				this.simulationEnabled = !this.simulationEnabled;
				this.onToggleSimulation?.Invoke(this.simulationEnabled);
			});
		}

		private void SetGridSize(float val)
		{
			this.gridSizeSlider.value = Mathf.Round(val / gridStep) * gridStep;
			this.gridSizeSlider.value = Math.Max(gridStep, this.gridSizeSlider.value);
			this.gridSizeValueLabel.text = this.gridSizeSlider.value.ToString("F2");
		}
	}

}
