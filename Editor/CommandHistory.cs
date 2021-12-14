using MapsExt.Editor.Commands;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MapsExt
{
	public class CommandHistory
	{
		private CommandHandlerProvider commandHandlerProvider;
		private List<Tuple<int, ICommand>> commands;
		private int commandIndex;
		private int commandMergeId;

		public CommandHistory(CommandHandlerProvider commandHandlerProvider)
		{
			this.commandHandlerProvider = commandHandlerProvider;
			this.commands = new List<Tuple<int, ICommand>>();
			this.commandIndex = -1;
			this.commandMergeId = 0;
		}

		public bool CanRedo()
		{
			return this.commandIndex < this.commands.Count - 1;
		}

		public bool CanUndo()
		{
			return this.commandIndex >= 0;
		}

		/// <summary>Adds a new command to the command history and executes it.</summary>
		/// <param name="cmd">Command to add and execute</param>
		/// <param name="merge">
		/// If true, merge the new command with the previous one when possible.
		/// If false or merging is not possible, just add the new command to the history.
		/// The command will be executed either way.
		/// </param>
		public void Add<T>(T cmd, bool merge = false) where T : ICommand
		{
			var handler = this.commandHandlerProvider.GetHandler<T>();

			if (handler == null)
			{
				throw new ArgumentException($"No handler registered for command type {cmd.GetType().Name}");
			}

			if (handler.IsRedundant(cmd))
			{
				return;
			}

			while (this.commandIndex < this.commands.Count - 1)
			{
				this.commands.RemoveAt(this.commandIndex + 1);
			}

			System.Tuple<int, ICommand> prevCmd = this.commands.Count > 0 ? this.commands[this.commandIndex] : null;

			// Merge new command with the previous one if it's possible and was requested
			if (merge && prevCmd?.Item1 == this.commandMergeId && prevCmd?.Item2.GetType() == typeof(T))
			{
				var mergedCmd = handler.Merge((T) prevCmd.Item2, cmd);
				this.commands[this.commandIndex] = new Tuple<int, ICommand>(this.commandMergeId, mergedCmd);

				if (handler.IsRedundant((T) mergedCmd))
				{
					this.commands.RemoveAt(this.commandIndex);
					this.commandIndex--;
				}
			}
			else
			{
				this.commands.Add(new Tuple<int, ICommand>(this.commandMergeId, cmd));
				this.commandIndex++;
			}

			// Execute only the new command, NOT the possible result of a merge
			handler.Execute(cmd);
		}

		public void PreventNextMerge()
		{
			this.commandMergeId++;
		}

		public void Execute()
		{
			this.commandIndex++;
			var cmd = this.commands[this.commandIndex];
			this.commandHandlerProvider.GetHandler(cmd.Item2.GetType()).Execute(cmd.Item2);
		}

		public void Undo()
		{
			var cmd = this.commands[this.commandIndex];
			this.commandHandlerProvider.GetHandler(cmd.Item2.GetType()).Undo(cmd.Item2);
			this.commandIndex--;
		}
	}
}
