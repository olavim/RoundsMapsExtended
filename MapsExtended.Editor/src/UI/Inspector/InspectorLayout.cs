using UnityEngine.UI;
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
		public Func<T> getValue;
		public Action<T> OnChangeStart { get; set; }
		public Action<T> SetValue { get; set; }
		public Action<T> OnChanged { get; set; }

		public InspectorLayoutProperty() { }

		public InspectorLayoutProperty(string name, Action<T> onChangeStart, Action<T> onChanged, Action<T> onChangeEnd, Func<T> getValue)
		{
			this.name = name;
			this.getValue = getValue;
			this.OnChangeStart = onChangeStart;
			this.SetValue = onChanged;
			this.OnChanged = onChangeEnd;
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
