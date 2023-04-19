using System;

namespace MapsExt.Editor.ActionHandlers
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class GroupActionHandlerAttribute : Attribute
	{
		public Type[] RequiredHandlerTypes { get; }

		public GroupActionHandlerAttribute(params Type[] requiredHandlers)
		{
			this.RequiredHandlerTypes = requiredHandlers;
		}
	}
}
