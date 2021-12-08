using System;
using System.Collections.Generic;

namespace MapsExt.Editor.Commands
{
	public class CommandHandlerProvider
	{
		private Dictionary<Type, object> handlers;

		public CommandHandlerProvider()
		{
			this.handlers = new Dictionary<Type, object>();
		}

		public void RegisterHandler<T>(ICommandHandler<T> handler) where T : ICommand
		{
			this.handlers.Add(typeof(T), handler);
		}

		public bool TryGetHandler<T>(out ICommandHandler<T> handler) where T : ICommand
		{
			if (this.handlers.TryGetValue(typeof(T), out object value))
			{
				handler = (ICommandHandler<T>) value;
				return true;
			}

			handler = null;
			return false;
		}

		public ICommandHandler<T> GetHandler<T>() where T : ICommand
		{
			if (this.handlers.TryGetValue(typeof(T), out object value))
			{
				return (ICommandHandler<T>) value;
			}

			return null;
		}

		public ICommandHandler GetHandler(Type type)
		{
			if (this.handlers.TryGetValue(type, out object value))
			{
				return (ICommandHandler) value;
			}

			return null;
		}
	}
}
