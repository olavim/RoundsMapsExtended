using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MapsExt.Editor.Commands
{
	public class CompositeCommand : ICommand
	{
		public readonly ICommand[] commands;

		public CompositeCommand(params ICommand[] commands)
		{
			this.commands = commands;
		}
	}

	public class CompositeCommandHandler : CommandHandler<CompositeCommand>
	{
		private CommandHandlerProvider provider;

		public CompositeCommandHandler(CommandHandlerProvider provider)
		{
			this.provider = provider;
		}

		public override IEnumerator Execute(CompositeCommand composite)
		{
			foreach (var cmd in composite.commands)
			{
				var handler = this.provider.GetHandler(cmd.GetType());
				yield return handler.Execute(cmd);
			}
		}

		public override IEnumerator Undo(CompositeCommand composite)
		{
			foreach (var cmd in composite.commands)
			{
				var handler = this.provider.GetHandler(cmd.GetType());
				yield return handler.Undo(cmd);
			}
		}

		public override CompositeCommand Merge(CompositeCommand c1, CompositeCommand c2)
		{
			var mergedCommands = new List<ICommand>();

			for (int i = 0; i < c1.commands.Length; i++)
			{
				var handler = this.provider.GetHandler(c1.commands[i].GetType());
				mergedCommands.Add(handler.Merge(c1.commands[i], c2.commands[i]));
			}

			return new CompositeCommand(mergedCommands.ToArray());
		}

		public override bool CanMerge(CompositeCommand c1, CompositeCommand c2)
		{
			if (c1.commands.Length != c2.commands.Length)
			{
				return false;
			}

			for (int i = 0; i < c1.commands.Length; i++)
			{
				var handler = this.provider.GetHandler(c1.commands[i].GetType());
				if (!handler.CanMerge(c1.commands[i], c2.commands[i]))
				{
					return false;
				}
			}

			return true;
		}

		public override bool IsRedundant(CompositeCommand composite)
		{
			foreach (var cmd in composite.commands)
			{
				var handler = this.provider.GetHandler(cmd.GetType());
				if (!handler.IsRedundant(cmd))
				{
					return false;
				}
			}

			return true;
		}
	}
}
