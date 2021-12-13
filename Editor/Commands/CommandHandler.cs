namespace MapsExt.Editor.Commands
{
	public abstract class CommandHandler<T> : ICommandHandler<T>, ICommandHandler where T : ICommand
	{
		void ICommandHandler.Execute(ICommand cmd) => this.Execute((T) cmd);
		void ICommandHandler.Redo(ICommand cmd) => this.Redo((T) cmd);
		void ICommandHandler.Undo(ICommand cmd) => this.Undo((T) cmd);
		ICommand ICommandHandler.Merge(ICommand cmd1, ICommand cmd2) => this.Merge((T) cmd1, (T) cmd2);
		bool ICommandHandler.IsRedundant(ICommand cmd) => this.IsRedundant((T) cmd);

		public abstract void Execute(T cmd);
		public abstract void Redo(T cmd);
		public abstract void Undo(T cmd);
		public abstract T Merge(T cmd1, T cmd2);
		public abstract bool IsRedundant(T cmd);
	}
}
