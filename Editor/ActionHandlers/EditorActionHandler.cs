using MapsExt.Editor.Commands;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public interface IActionHandler
	{
		void Handle(ICommand cmd);
	}

	public interface IActionHandler<T> : IActionHandler where T : ICommand
	{
		void Handle(T cmd);
	}

	public abstract class ActionHandler : MonoBehaviour, IActionHandler
	{
		public abstract void Handle(ICommand cmd);
	}

	public abstract class ActionHandler<T> : ActionHandler, IActionHandler<T> where T : ICommand
	{
		public override void Handle(ICommand cmd) => this.Handle((T) cmd);
		public abstract void Handle(T cmd);
	}
}
