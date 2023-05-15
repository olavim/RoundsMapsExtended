using System;
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
			if (this.ShouldHandleEvent(evt))
			{
				this.HandleEvent(evt);
			}
		}

		protected abstract void HandleEvent(IEditorEvent evt);

		protected virtual bool ShouldHandleEvent(IEditorEvent evt)
		{
			return this.Editor.ActiveMapObjectPart == this.gameObject;
		}

		public bool Equals(EditorEventHandler other) => this._guid == other._guid;
		public override bool Equals(object other) => other is EditorEventHandler handler && this.Equals(handler);
		public override int GetHashCode() => this._guid.GetHashCode();
	}
}
