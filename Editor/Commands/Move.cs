using MapsExt.Editor.ActionHandlers;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.Commands
{
	public class MoveCommand : ICommand
	{
		public readonly ActionHandlerLocator[] handlerLocators;
		public readonly Vector3 delta;
		public readonly int frameIndex;

		public MoveCommand(IEnumerable<EditorActionHandler> handlers, Vector3 delta, int frameIndex = 0)
		{
			this.handlerLocators = ActionHandlerLocator.FromActionHandlers(handlers);
			this.delta = delta;
			this.frameIndex = frameIndex;
		}

		public MoveCommand(EditorActionHandler handler, Vector3 delta, int frameIndex = 0)
		{
			this.handlerLocators = ActionHandlerLocator.FromActionHandlers(new EditorActionHandler[] { handler });
			this.delta = delta;
			this.frameIndex = frameIndex;
		}

		public MoveCommand(MoveCommand cmd, Vector3 delta)
		{
			this.handlerLocators = cmd.handlerLocators;
			this.delta = delta;
			this.frameIndex = cmd.frameIndex;
		}
	}

	public class MoveCommandHandler : CommandHandler<MoveCommand>
	{
		private MapEditor editor;

		public MoveCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		public override void Execute(MoveCommand cmd)
		{
			foreach (var locator in cmd.handlerLocators)
			{
				var handler = locator.FindActionHandler(this.editor.content);
				handler.Move(cmd.delta);

				var anim = handler.GetComponent<MapObjectAnimation>();
				if (anim)
				{
					anim.keyframes[cmd.frameIndex].position = handler.transform.position;
					this.editor.animationHandler.RefreshKeyframeMapObject();
				}
			}
		}

		public override void Undo(MoveCommand cmd)
		{
			foreach (var locator in cmd.handlerLocators)
			{
				var handler = locator.FindActionHandler(this.editor.content);
				handler.Move(-cmd.delta);

				var anim = handler.GetComponent<MapObjectAnimation>();
				if (anim)
				{
					anim.keyframes[cmd.frameIndex].position = handler.transform.position;
					this.editor.animationHandler.RefreshKeyframeMapObject();
				}
			}

			this.editor.UpdateRopeAttachments();
		}

		public override MoveCommand Merge(MoveCommand cmd1, MoveCommand cmd2)
		{
			return new MoveCommand(cmd1, cmd1.delta + cmd2.delta);
		}

		public override bool IsRedundant(MoveCommand cmd)
		{
			return cmd.delta == Vector3.zero;
		}
	}
}
