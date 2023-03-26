using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class GroupActionHandlerAttribute : Attribute
	{
		public readonly Type[] requiredHandlerTypes;

		public GroupActionHandlerAttribute(params Type[] requiredHandlers)
		{
			this.requiredHandlerTypes = requiredHandlers;
		}
	}

	public interface IGroupMapObjectActionHandler
	{
		void Initialize(IEnumerable<GameObject> gameObjects);
	}
}
