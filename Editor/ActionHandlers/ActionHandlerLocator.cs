using MapsExt.MapObjects;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	// Finds a specific ActionHandler component from a GameObject hierarchy where the child GameObject instances might change
	public class ActionHandlerLocator
	{
		private GameObject container;
		private string mapObjectId;
		private int actionHandlerIndex;

		public ActionHandlerLocator(GameObject container, ActionHandler handler)
		{
			this.container = container;

			var mapObjectInstance = handler.GetComponentsInParent<MapObjectInstance>(true)[0];

			this.mapObjectId = mapObjectInstance.mapObjectId;

			// We assume GetComponentsInChildren always returns components in the same order within a given Unity version
			this.actionHandlerIndex = mapObjectInstance.GetComponentsInChildren<ActionHandler>(true).ToList().IndexOf(handler);
		}

		public ActionHandler Locate()
		{
			var instances = this.container.GetComponentsInChildren<MapObjectInstance>(true);
			var instance = instances.First(ins => ins.mapObjectId == this.mapObjectId);
			return instance.GetComponentsInChildren<ActionHandler>(true).ToArray()[this.actionHandlerIndex];
		}
	}
}
