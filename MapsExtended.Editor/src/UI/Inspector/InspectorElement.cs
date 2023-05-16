using UnityEngine;

namespace MapsExt.Editor.UI
{
	public abstract class InspectorElement : IInspectorElement
	{
		protected InspectorContext Context { get; private set; }

		public GameObject Instantiate(InspectorContext context)
		{
			this.Context = context;
			return this.GetInstance();
		}

		public virtual void OnUpdate() { }

		protected abstract GameObject GetInstance();
	}
}
