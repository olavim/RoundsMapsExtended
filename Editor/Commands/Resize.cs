using System.Collections.Generic;
using UnityEngine;
using MapsExt.Editor.ActionHandlers;

namespace MapsExt.Editor.Commands
{
	public class ResizeCommand : ICommand
	{
		public readonly ActionHandlerLocator[] handlerLocators;
		public readonly Vector3 delta;
		public readonly int resizeDirection;
		public readonly int frameIndex;

		public ResizeCommand(IEnumerable<EditorActionHandler> handlers, Vector3 delta, int resizeDirection, int frameIndex = 0)
		{
			this.handlerLocators = ActionHandlerLocator.FromActionHandlers(handlers);
			this.delta = delta;
			this.resizeDirection = resizeDirection;
			this.frameIndex = frameIndex;
		}

		public ResizeCommand(EditorActionHandler handlers, Vector3 delta, int resizeDirection, int frameIndex = 0)
		{
			this.handlerLocators = ActionHandlerLocator.FromActionHandlers(new EditorActionHandler[] { handlers });
			this.delta = delta;
			this.resizeDirection = resizeDirection;
			this.frameIndex = frameIndex;
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

		public override void Execute(ResizeCommand cmd)
		{
			foreach (var locator in cmd.handlerLocators)
			{
				var handler = (SpatialActionHandler) locator.FindActionHandler(this.editor.content);
				handler.Resize(cmd.delta, cmd.resizeDirection, cmd.frameIndex);
			}
		}

		public override void Redo(ResizeCommand cmd)
		{
			this.Execute(cmd);
			this.editor.UpdateRopeAttachments();
		}

		public override void Undo(ResizeCommand cmd)
		{
			foreach (var locator in cmd.handlerLocators)
			{
				var handler = (SpatialActionHandler) locator.FindActionHandler(this.editor.content);
				handler.Resize(-cmd.delta, cmd.resizeDirection, cmd.frameIndex);
			}

			this.editor.UpdateRopeAttachments();
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
