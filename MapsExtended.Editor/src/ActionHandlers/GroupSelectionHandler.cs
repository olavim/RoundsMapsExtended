using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupMapObjectActionHandler(typeof(SelectionHandler))]
	public class GroupSelectionHandler : SelectionHandler, IGroupMapObjectActionHandler
	{
		private IEnumerable<GameObject> gameObjects;

		protected virtual void Awake()
		{
			this.gameObject.AddComponent<BoxCollider2D>();
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this.gameObjects = gameObjects;

			var bounds = this.gameObjects.First().GetComponent<SelectionHandler>().GetBounds();
			foreach (var obj in this.gameObjects)
			{
				bounds.Encapsulate(obj.GetComponent<SelectionHandler>().GetBounds());
			}

			this.transform.position = bounds.center;
			this.transform.localScale = bounds.size;
		}
	}
}
