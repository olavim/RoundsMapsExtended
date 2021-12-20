using MapsExt.Editor;
using MapsExt.Editor.Commands;
using System.Collections.Generic;
using System;
using System.Collections;

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
			if (this.AddCommand(cmd, merge))
			{
				MapsExtendedEditor.instance.StartCoroutine(this.ExecuteCommandCoroutine(cmd));
			}
		}

		public IEnumerator AddAsync<T>(T cmd, bool merge = false) where T : ICommand
		{
			if (this.AddCommand(cmd, merge))
			{
				yield return this.ExecuteCommandCoroutine(cmd);
			}
		}

		private IEnumerator ExecuteCommandCoroutine<T>(T cmd) where T : ICommand
		{
			var handler = this.commandHandlerProvider.GetHandler(cmd.GetType());
			yield return handler.Execute(cmd);
		}

		private IEnumerator UndoCommandCoroutine<T>(T cmd) where T : ICommand
		{
			var handler = this.commandHandlerProvider.GetHandler(cmd.GetType());
			yield return handler.Undo(cmd);
		}

		private bool AddCommand<T>(T cmd, bool merge = false) where T : ICommand
		{
			var handler = this.commandHandlerProvider.GetHandler(cmd.GetType());

			if (handler == null)
			{
				throw new ArgumentException($"No handler registered for command type {cmd.GetType().Name}");
			}

			if (handler.IsRedundant(cmd))
			{
				return false;
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

			return true;
		}

		public void PreventNextMerge()
		{
			this.commandMergeId++;
		}

		public void Execute()
		{
			MapsExtendedEditor.instance.StartCoroutine(this.ExecuteAsync());
		}

		public void Undo()
		{
			MapsExtendedEditor.instance.StartCoroutine(this.UndoAsync());
		}

		public IEnumerator ExecuteAsync()
		{
			this.commandIndex++;
			var cmd = this.commands[this.commandIndex];
			yield return this.ExecuteCommandCoroutine(cmd.Item2);
		}

		public IEnumerator UndoAsync()
		{
			var cmd = this.commands[this.commandIndex];
			yield return this.UndoCommandCoroutine(cmd.Item2);
			this.commandIndex--;
		}
	}
}
