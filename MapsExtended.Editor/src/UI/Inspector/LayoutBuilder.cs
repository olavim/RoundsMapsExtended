using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;

namespace MapsExt.Editor.UI
{
	public interface ILayoutElementBuilder
	{
		ILayoutElement element { get; }
	}

	public class InspectorLayoutBuilder
	{
		public InspectorLayout layout
		{
			get
			{
				var l = new InspectorLayout();
				l.elements = this.propertyBuilders.Select(b => b.element).ToList();
				return l;
			}
		}

		public List<ILayoutElementBuilder> propertyBuilders = new List<ILayoutElementBuilder>();

		public InspectorPropertyBuilder<T> Property<T>(string name)
		{
			var builder = new InspectorPropertyBuilder<T>();
			this.propertyBuilders.Add(builder.Name(name));
			return builder;
		}

		public InspectorButtonBuilder Button()
		{
			var builder = new InspectorButtonBuilder();
			this.propertyBuilders.Add(builder);
			return builder;
		}

		public InspectorDividerBuilder Divider()
		{
			var builder = new InspectorDividerBuilder();
			this.propertyBuilders.Add(builder);
			return builder;
		}
	}

	public class InspectorPropertyBuilder<T> : ILayoutElementBuilder
	{
		public ILayoutElement element { get; private set; }

		public InspectorPropertyBuilder()
		{
			this.element = new InspectorLayoutProperty<T>();
		}

		public InspectorPropertyBuilder<T> Name(string name)
		{
			(this.element as InspectorLayoutProperty<T>).name = name;
			return this;
		}

		public InspectorPropertyBuilder<T> ValueSetter(Action<T> setValue)
		{
			(this.element as InspectorLayoutProperty<T>).SetValue = setValue;
			return this;
		}

		public InspectorPropertyBuilder<T> OnChange(Action<T> onChanged)
		{
			(this.element as InspectorLayoutProperty<T>).OnChanged = onChanged;
			return this;
		}

		public InspectorPropertyBuilder<T> ValueGetter(Func<T> getValue)
		{
			(this.element as InspectorLayoutProperty<T>).getValue = getValue;
			return this;
		}
	}

	public class InspectorButtonBuilder : ILayoutElementBuilder
	{
		public ILayoutElement element { get; private set; }

		public InspectorButtonBuilder()
		{
			this.element = new InspectorLayoutButton();
		}

		public InspectorButtonBuilder ClickEvent(Action onClick)
		{
			(this.element as InspectorLayoutButton).onClick = onClick;
			return this;
		}

		public InspectorButtonBuilder UpdateEvent(Action<Button> onUpdate)
		{
			(this.element as InspectorLayoutButton).onUpdate = onUpdate;
			return this;
		}
	}

	public class InspectorDividerBuilder : ILayoutElementBuilder
	{
		public ILayoutElement element { get; private set; }

		public InspectorDividerBuilder()
		{
			this.element = new InspectorDivider();
		}
	}
}
