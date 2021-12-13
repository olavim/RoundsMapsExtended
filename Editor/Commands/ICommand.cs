using MapsExt.Editor.ActionHandlers;
using MapsExt.MapObjects;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace MapsExt.Editor.Commands
{
	public interface ICommand { }

	public interface ICommandHandler
	{
		void Execute(ICommand cmd);
		void Redo(ICommand cmd);
		void Undo(ICommand cmd);
		ICommand Merge(ICommand cmd1, ICommand cmd2);
		bool IsRedundant(ICommand cmd);
	}

	public interface ICommandHandler<T> where T : ICommand
	{
		void Execute(T cmd);
		void Redo(T cmd);
		void Undo(T cmd);
		T Merge(T cmd1, T cmd2);
		bool IsRedundant(T cmd);
	}

	// Finds a specific EditorActionHandler component from a GameObject hierarchy where the GameObject instances might change
	public class ActionHandlerLocator
	{
		public static ActionHandlerLocator[] FromActionHandlers(IEnumerable<EditorActionHandler> handlers)
		{
			var list = new List<ActionHandlerLocator>();
			foreach (var handler in handlers)
			{
				var mapObjectInstance = handler.GetComponentsInParent<MapObjectInstance>(true)[0];

				// We assume GetComponentsInChildren always returns components in the same order within a given Unity version
				int handlerIndex = mapObjectInstance.GetComponentsInChildren<EditorActionHandler>(true).ToList().IndexOf(handler);
				list.Add(new ActionHandlerLocator(mapObjectInstance.mapObjectId, handlerIndex));
			}
			return list.ToArray();
		}

		public string mapObjectId;
		public int actionHandlerIndex;

		public ActionHandlerLocator(string mapObjectId, int actionHandlerIndex)
		{
			this.mapObjectId = mapObjectId.Split(':')[0];
			this.actionHandlerIndex = actionHandlerIndex;
		}

		public EditorActionHandler FindActionHandler(GameObject go)
		{
			var instances = go.GetComponentsInChildren<MapObjectInstance>(true);
			var instance = instances.First(ins => ins.mapObjectId == this.mapObjectId);
			return instance.GetComponentsInChildren<EditorActionHandler>(true).ToArray()[this.actionHandlerIndex];
		}
	}
}
