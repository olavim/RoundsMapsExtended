using UnityEngine.UI;
using MapsExt.Editor.Commands;
using System;
using System.Collections.Generic;

namespace MapsExt.Editor.UI
{
	public interface ILayoutElement { }

	public class InspectorLayout
	{
		public List<ILayoutElement> elements = new List<ILayoutElement>();
	}

	public class InspectorLayoutProperty<T> : ILayoutElement
	{
		public string name;
		public Func<T, ICommand> getCommand;
		public Func<T> getValue;
		public Action onChanged { get; set; }

		public InspectorLayoutProperty() { }

		public InspectorLayoutProperty(string name, Func<T, ICommand> getCommand, Func<T> getValue, Action onChanged = null)
		{
			this.name = name;
			this.getCommand = getCommand;
			this.getValue = getValue;
			this.onChanged = onChanged;
		}
	}

	public class InspectorLayoutButton : ILayoutElement
	{
		public Action onClick;
		public Action<Button> onUpdate;

		public InspectorLayoutButton() { }

		public InspectorLayoutButton(Action onClick, Action<Button> onUpdate)
		{
			this.onClick = onClick;
			this.onUpdate = onUpdate;
		}
	}

	public class InspectorDivider : ILayoutElement { }
}
