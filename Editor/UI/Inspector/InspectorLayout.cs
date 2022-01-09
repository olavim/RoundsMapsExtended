using UnityEngine.UI;
using MapsExt.Editor.Commands;
using System;
using System.Collections.Generic;
using MapsExt.Editor.ActionHandlers;

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
		public Func<T> getValue;
		public Action<T> onChangeStart { get; set; }
		public Action<T> setValue { get; set; }
		public Action<T> onChanged { get; set; }

		public InspectorLayoutProperty() { }

		public InspectorLayoutProperty(string name, Action<T> onChangeStart, Action<T> onChanged, Action<T> onChangeEnd, Func<T> getValue)
		{
			this.name = name;
			this.getValue = getValue;
			this.onChangeStart = onChangeStart;
			this.setValue = onChanged;
			this.onChanged = onChangeEnd;
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
