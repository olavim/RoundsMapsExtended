using MapsExt.MapObjects;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace MapsExt.Editor.Commands
{
	public class DeleteCommand : ICommand
	{
		public readonly MapObject[] mapObjects;

		public DeleteCommand(IEnumerable<GameObject> instances)
		{
			this.mapObjects = instances.Select(obj => MapsExtendedEditor.instance.mapObjectManager.Serialize(obj)).ToArray();
		}

		public DeleteCommand(IEnumerable<MapObjectInstance> instances)
		{
			this.mapObjects = instances.Select(obj => MapsExtendedEditor.instance.mapObjectManager.Serialize(obj)).ToArray();
		}
	}

	public class DeleteCommandHandler : CommandHandler<DeleteCommand>
	{
		private MapEditor editor;

		public DeleteCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		override public void Execute(DeleteCommand cmd)
		{
			foreach (var mapObject in cmd.mapObjects)
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

		override public void Undo(DeleteCommand cmd)
		{
			this.editor.StartCoroutine(this.UndoCoroutine(cmd));
		}

		private IEnumerator UndoCoroutine(DeleteCommand cmd)
		{
			int waiting = cmd.mapObjects.Length;

			for (int i = 0; i < cmd.mapObjects.Length; i++)
			{
				MapsExtendedEditor.instance.SpawnObject(
					this.editor.content,
					cmd.mapObjects[i],
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

		override public DeleteCommand Merge(DeleteCommand cmd1, DeleteCommand cmd2)
		{
			return cmd2;
		}
	}
}
