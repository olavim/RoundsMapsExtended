using System;

namespace MapsExt.Editor.Events
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class GroupEventHandlerAttribute : Attribute
	{
		public Type[] RequiredHandlerTypes { get; }

		public GroupEventHandlerAttribute(params Type[] requiredHandlers)
		{
			this.RequiredHandlerTypes = requiredHandlers;
		}
	}
}
