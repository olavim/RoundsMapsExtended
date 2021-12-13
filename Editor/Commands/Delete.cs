using MapsExt.MapObjects;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace MapsExt.Editor.Commands
{
	public class DeleteCommand : ICommand
	{
		public readonly MapObject[] data;

		public DeleteCommand(IEnumerable<GameObject> instances)
		{
			this.data = instances.Select(obj => MapsExtendedEditor.instance.mapObjectManager.Serialize(obj)).ToArray();
		}

		public DeleteCommand(IEnumerable<MapObjectInstance> instances)
		{
			this.data = instances.Select(obj => MapsExtendedEditor.instance.mapObjectManager.Serialize(obj)).ToArray();
		}
	}

	public class DeleteCommandHandler : CommandHandler<DeleteCommand>
	{
		private MapEditor editor;

		public DeleteCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		public override void Execute(DeleteCommand cmd)
		{
			foreach (var mapObject in cmd.data)
			{
				var instance = mapObject.FindInstance(this.editor.content).gameObject;

				if (instance == this.editor.animationHandler.animation?.gameObject)
				{
					this.editor.animationHandler.SetAnimation(null);
				}

				GameObject.Destroy(instance);
			}

			this.editor.ResetSpawnLabels();
			this.editor.ClearSelected();
			this.editor.UpdateRopeAttachments();
		}

		public override void Redo(DeleteCommand cmd)
		{
			this.Execute(cmd);
		}

		public override void Undo(DeleteCommand cmd)
		{
			this.editor.StartCoroutine(this.UndoCoroutine(cmd));
		}

		private IEnumerator UndoCoroutine(DeleteCommand cmd)
		{
			int waiting = cmd.data.Length;

			for (int i = 0; i < cmd.data.Length; i++)
			{
				MapsExtendedEditor.instance.SpawnObject(
					this.editor.content,
					cmd.data[i],
					_ => waiting--
				);
			}

			while (waiting > 0)
			{
				yield return null;
			}

			this.editor.ResetSpawnLabels();
			this.editor.UpdateRopeAttachments();
		}

		public override DeleteCommand Merge(DeleteCommand cmd1, DeleteCommand cmd2)
		{
			return cmd2;
		}

		public override bool IsRedundant(DeleteCommand cmd)
		{
			return cmd.data.Length == 0;
		}
	}
}
