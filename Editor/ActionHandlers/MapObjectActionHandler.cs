using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	/// <summary>
	/// An action handler makes a map object selectable in the editor.
	/// </summary>
	public abstract class MapObjectActionHandler : MonoBehaviour
	{
		protected MapEditor Editor => this.GetComponentInParent<MapEditor>();
		public Action OnChange = () => { };

		public virtual void OnSelect() { }
		public virtual void OnDeselect() { }
		public virtual void OnPointerDown() { }
		public virtual void OnPointerUp() { }
		public virtual void OnKeyDown(KeyCode key) { }
		public virtual void OnKeyUp(KeyCode key) { }
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class GroupMapObjectActionHandler : Attribute
	{
		public readonly Type[] requiredHandlerTypes;

		public GroupMapObjectActionHandler(params Type[] requiredHandlers)
		{
			this.requiredHandlerTypes = requiredHandlers;
		}
	}

	public interface IGroupMapObjectActionHandler
	{
		void SetHandlers(IEnumerable<GameObject> gameObjects);
	}
}
