using System.Collections.Generic;
using UnityEngine;
using MapsExt.Editor.ActionHandlers;
using System.Collections;

namespace MapsExt.Editor.Commands
{
	public class RotateCommand : ICommand
	{
		public readonly ActionHandlerLocator[] handlerLocators;
		public readonly Quaternion fromRotation;
		public readonly Quaternion toRotation;
		public readonly int frameIndex;

		public RotateCommand(IEnumerable<EditorActionHandler> handlers, Quaternion fromRotation, Quaternion toRotation)
		{
			this.handlerLocators = ActionHandlerLocator.FromActionHandlers(handlers);
			this.fromRotation = fromRotation;
			this.toRotation = toRotation;

			var e = handlers.GetEnumerator();
			e.MoveNext();
			this.frameIndex = e.Current.frameIndex;
		}

		public RotateCommand(EditorActionHandler handler, Quaternion fromRotation, Quaternion toRotation)
		{
			this.handlerLocators = ActionHandlerLocator.FromActionHandlers(new EditorActionHandler[] { handler });
			this.fromRotation = fromRotation;
			this.toRotation = toRotation;
			this.frameIndex = handler.frameIndex;
		}

		public RotateCommand(RotateCommand cmd, Quaternion fromRotation, Quaternion toRotation)
		{
			this.handlerLocators = cmd.handlerLocators;
			this.fromRotation = fromRotation;
			this.toRotation = toRotation;
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

		public override IEnumerator Execute(RotateCommand cmd)
		{
			foreach (var locator in cmd.handlerLocators)
			{
				var handler = (SpatialActionHandler) locator.FindActionHandler(this.editor.content);
				handler.SetRotation(cmd.toRotation, cmd.frameIndex);
			}

			yield break;
		}

		public override IEnumerator Undo(RotateCommand cmd)
		{
			foreach (var locator in cmd.handlerLocators)
			{
				var handler = (SpatialActionHandler) locator.FindActionHandler(this.editor.content);
				handler.SetRotation(cmd.fromRotation, cmd.frameIndex);
			}

			yield break;
		}

		public override RotateCommand Merge(RotateCommand cmd1, RotateCommand cmd2)
		{
			return new RotateCommand(cmd1, cmd1.fromRotation, cmd2.toRotation);
		}

		public override bool IsRedundant(RotateCommand cmd)
		{
			return cmd.fromRotation == cmd.toRotation;
		}
	}
}
