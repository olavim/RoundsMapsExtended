using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.Events
{
	public abstract class EditorEventHandler : MonoBehaviour, IEquatable<EditorEventHandler>
	{
		private Guid _guid = Guid.NewGuid();

		protected MapEditor Editor { get; private set; }

		protected virtual void Awake()
		{
			this.Editor = this.GetComponentInParent<MapEditor>();
		}

		protected virtual void OnEnable()
		{
			this.Editor.EditorEvent += this.OnEditorEvent;
		}

		protected virtual void OnDisable()
		{
			this.Editor.EditorEvent -= this.OnEditorEvent;
		}

		private void OnEditorEvent(object sender, IEditorEvent evt)
		{
			var subjects = new HashSet<EditorEventHandler>(this.Editor.SelectedObjects.SelectMany(obj => obj.GetComponents<EditorEventHandler>()));
			this.HandleEditorEvent(evt, subjects);
		}

		protected virtual void HandleEditorEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects)
		{
			if (this.ShouldHandleEvent(evt, subjects))
			{
				this.HandleAcceptedEditorEvent(evt, subjects);
			}
		}

		protected abstract void HandleAcceptedEditorEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects);

		protected virtual bool ShouldHandleEvent(IEditorEvent evt, ISet<EditorEventHandler> subjects)
		{
			return subjects.Contains(this) && this.Editor.SelectedObjects.Count == 1;
		}

		public bool Equals(EditorEventHandler other) => this._guid == other._guid;
		public override bool Equals(object other) => other is EditorEventHandler handler && this.Equals(handler);
		public override int GetHashCode() => this._guid.GetHashCode();
	}
}
