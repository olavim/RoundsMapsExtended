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
			this.initialized = true;
		}

		public CreateCommand(MapObject data)
		{
			this.data = new MapObject[] { data };
			this.initialized = true;
		}
	}

	public class CreateCommandHandler : CommandHandler<CreateCommand>
	{
		private MapEditor editor;

		public CreateCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		public override void Execute(CreateCommand cmd)
		{
			this.editor.StartCoroutine(this.ExecuteCoroutine(cmd));
		}

		private IEnumerator ExecuteCoroutine(CreateCommand cmd)
		{
			int waiting = cmd.data.Length;
			this.editor.ClearSelected();

			foreach (var data in cmd.data)
			{
				MapsExtendedEditor.instance.SpawnObject(this.editor.content, data, obj =>
				{
					if (!cmd.initialized)
					{
						float scaleStep = Mathf.Max(1, this.editor.GridSize * 2f);
						obj.transform.localScale = EditorUtils.SnapToGrid(obj.transform.localScale, scaleStep);
						obj.transform.position = Vector3.zero;
					}

					this.editor.AddSelected(obj.GetComponentsInChildren<EditorActionHandler>());
					waiting--;
				});
			}

			while (waiting > 0)
			{
				yield return null;
			}

			this.editor.ResetSpawnLabels();
			this.editor.UpdateRopeAttachments();
		}

		public override void Redo(CreateCommand cmd)
		{
			this.Execute(cmd);
		}

		public override void Undo(CreateCommand cmd)
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

			this.editor.ClearSelected();
			this.editor.ResetSpawnLabels();
			this.editor.UpdateRopeAttachments();
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
