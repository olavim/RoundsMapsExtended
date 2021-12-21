using System.Collections.Generic;
using UnityEngine;
using MapsExt.Editor.ActionHandlers;
using System.Collections;

namespace MapsExt.Editor.Commands
{
	public class ResizeCommand : ICommand
	{
		public readonly ActionHandlerLocator[] handlerLocators;
		public readonly Vector3 delta;
		public readonly int resizeDirection;
		public readonly int frameIndex;

		public ResizeCommand(EditorActionHandler handler, Vector3 from, Vector3 to) : this(handler, to - from, 0) { }

		public ResizeCommand(EditorActionHandler handler, Vector3 delta) : this(handler, delta, 0) { }

		public ResizeCommand(EditorActionHandler handler, Vector3 delta, int resizeDirection) : this(new[] { handler }, delta, resizeDirection) { }

		public ResizeCommand(IEnumerable<EditorActionHandler> handlers, Vector3 delta) : this(handlers, delta, 0) { }

		public ResizeCommand(IEnumerable<EditorActionHandler> handlers, Vector3 delta, int resizeDirection)
		{
			this.handlerLocators = ActionHandlerLocator.FromActionHandlers(handlers);
			this.delta = delta;
			this.resizeDirection = resizeDirection;

			var e = handlers.GetEnumerator();
			e.MoveNext();
			this.frameIndex = e.Current.frameIndex;
		}

		public ResizeCommand(ResizeCommand cmd, Vector3 delta)
		{
			this.handlerLocators = cmd.handlerLocators;
			this.delta = delta;
			this.resizeDirection = cmd.resizeDirection;
			this.frameIndex = cmd.frameIndex;
		}
	}

	public class ResizeCommandHandler : CommandHandler<ResizeCommand>
	{
		private MapEditor editor;

		public ResizeCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		public override IEnumerator Execute(ResizeCommand cmd)
		{
			foreach (var locator in cmd.handlerLocators)
			{
				var handler = (SpatialActionHandler) locator.FindActionHandler(this.editor.content);
				handler.Resize(cmd.delta, cmd.resizeDirection, cmd.frameIndex);
			}

			yield break;
		}

		public override IEnumerator Undo(ResizeCommand cmd)
		{
			foreach (var locator in cmd.handlerLocators)
			{
				var handler = (SpatialActionHandler) locator.FindActionHandler(this.editor.content);
				handler.Resize(-cmd.delta, cmd.resizeDirection, cmd.frameIndex);
			}

			yield break;
		}

		public override ResizeCommand Merge(ResizeCommand cmd1, ResizeCommand cmd2)
		{
			return new ResizeCommand(cmd1, cmd1.delta + cmd2.delta);
		}

		public override bool IsRedundant(ResizeCommand cmd)
		{
			return cmd.delta == Vector3.zero;
		}
	}
}
