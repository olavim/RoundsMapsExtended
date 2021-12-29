using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using HarmonyLib;
using UnityEngine;

namespace MapsExt.Editor.Commands
{
	public class CreateCommand : ICommand
	{
		public readonly MapObject[] data;
		public readonly bool initialized;

		public CreateCommand(Type type)
		{
			this.data = new MapObject[] { (MapObject) AccessTools.CreateInstance(type) };
			this.initialized = false;
		}

		public CreateCommand(IEnumerable<MapObject> data)
		{
			this.data = data.ToArray();

			foreach (var mapObject in this.data)
			{
				mapObject.mapObjectId = Guid.NewGuid().ToString();
			}

			this.initialized = true;
		}

		public CreateCommand(MapObject data) : this(new[] { data }) { }
	}

	public class CreateCommandHandler : CommandHandler<CreateCommand>
	{
		private MapEditor editor;

		public CreateCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		public override IEnumerator Execute(CreateCommand cmd)
		{
			int waiting = cmd.data.Length;
			this.editor.ClearSelected();

			foreach (var data in cmd.data)
			{
				MapsExtendedEditor.instance.SpawnObject(this.editor.content, data, obj =>
				{
					var handlers = obj.GetComponentsInChildren<EditorActionHandler>();

					if (!cmd.initialized)
					{
						float scaleStep = Mathf.Max(1, this.editor.GridSize * 2f);
						obj.transform.localScale = EditorUtils.SnapToGrid(obj.transform.localScale, scaleStep);
						obj.transform.position = Vector3.zero;
					}
					else
					{
						foreach (var handler in handlers)
						{
							handler.Move(new Vector3(1, -1, 0));
						}
					}

					this.editor.AddSelected(handlers);
					waiting--;
				});
			}

			while (waiting > 0)
			{
				yield return null;
			}

			this.editor.ResetSpawnLabels();
			this.editor.UpdateRopeAttachments(false);
		}

		public override IEnumerator Undo(CreateCommand cmd)
		{
			this.editor.DetachRopes();

			foreach (var mapObject in cmd.data)
			{
				var instance = mapObject.FindInstance(this.editor.content).gameObject;

				if (instance == this.editor.animationHandler.animation?.gameObject)
				{
					this.editor.animationHandler.SetAnimation(null);
				}

				GameObject.Destroy(instance);
			}

			this.editor.ClearSelected();
			this.editor.ResetSpawnLabels();
			this.editor.UpdateRopeAttachments(true);
			yield break;
		}

		public override CreateCommand Merge(CreateCommand cmd1, CreateCommand cmd2)
		{
			return cmd2;
		}

		public override bool IsRedundant(CreateCommand cmd)
		{
			return cmd.data.Length == 0;
		}
	}
}
