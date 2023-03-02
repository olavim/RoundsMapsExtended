using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupMapObjectActionHandler(typeof(SelectionHandler))]
	public class GroupSelectionHandler : SelectionHandler, IGroupMapObjectActionHandler
	{
		public IEnumerable<GameObject> GameObjects { private get; set; }

		private void Awake()
		{
			this.gameObject.AddComponent<BoxCollider2D>();

			var bounds = this.GameObjects.First().GetComponent<SelectionHandler>().GetBounds();
			foreach (var obj in this.GameObjects)
			{
				bounds.Encapsulate(obj.GetComponent<SelectionHandler>().GetBounds());
			}

			this.transform.position = bounds.center;
			this.transform.localScale = bounds.size;
		}
	}
}
