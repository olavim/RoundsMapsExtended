using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(SelectionHandler))]
	public class GroupSelectionHandler : SelectionHandler, IGroupMapObjectActionHandler
	{
		protected virtual void Awake()
		{
			this.gameObject.AddComponent<BoxCollider2D>();
		}

		public void Initialize(IEnumerable<GameObject> gameObjects) { }
	}
}
