using System.Collections.Generic;
using UnityEngine;
using MapsExt.Editor.ActionHandlers;

namespace MapsExt.Editor.Commands
{
	public class RotateCommand : ICommand
	{
		public readonly ActionHandlerLocator[] handlerLocators;
		public readonly Quaternion delta;
		public readonly int frameIndex;

		public RotateCommand(IEnumerable<EditorActionHandler> handlers, Quaternion delta, int frameIndex = 0)
		{
			this.handlerLocators = ActionHandlerLocator.FromActionHandlers(handlers);
			this.delta = delta;
			this.frameIndex = frameIndex;
		}

		public RotateCommand(EditorActionHandler handler, Quaternion delta, int frameIndex = 0)
		{
			this.handlerLocators = ActionHandlerLocator.FromActionHandlers(new EditorActionHandler[] { handler });
			this.delta = delta;
			this.frameIndex = frameIndex;
		}

		public RotateCommand(RotateCommand cmd, Quaternion delta)
		{
			this.handlerLocators = cmd.handlerLocators;
			this.delta = delta;
			this.frameIndex = cmd.frameIndex;
		}
	}

	public class RotateCommandHandler : CommandHandler<RotateCommand>
	{
		private MapEditor editor;

		public RotateCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		override public void Execute(RotateCommand cmd)
		{
			foreach (var locator in cmd.handlerLocators)
			{
				var handler = locator.FindActionHandler(this.editor.content);
				handler.Rotate(cmd.delta);

				var anim = handler.GetComponent<MapObjectAnimation>();
				if (anim)
				{
					anim.keyframes[cmd.frameIndex].rotation = handler.transform.rotation;
					this.editor.animationHandler.RefreshKeyframeMapObject();
				}
			}
		}

		override public void Undo(RotateCommand cmd)
		{
			foreach (var locator in cmd.handlerLocators)
			{
				var handler = locator.FindActionHandler(this.editor.content);
				handler.Rotate(Quaternion.Inverse(cmd.delta));

				var anim = handler.GetComponent<MapObjectAnimation>();
				if (anim)
				{
					anim.keyframes[cmd.frameIndex].rotation = handler.transform.rotation;
					this.editor.animationHandler.RefreshKeyframeMapObject();
				}
			}

			this.editor.UpdateRopeAttachments();
		}

		override public RotateCommand Merge(RotateCommand cmd1, RotateCommand cmd2)
		{
			return new RotateCommand(cmd1, cmd1.delta * cmd2.delta);
		}
	}
}
