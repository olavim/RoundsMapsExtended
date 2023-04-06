using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(SelectionHandler))]
	public class GroupSizeHandler : MonoBehaviour, IGroupMapObjectActionHandler
	{
		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			var bounds = gameObjects.First().GetComponent<SelectionHandler>().GetBounds();
			foreach (var obj in gameObjects)
			{
				bounds.Encapsulate(obj.GetComponent<SelectionHandler>().GetBounds());
			}

			this.transform.localScale = bounds.size;
		}
	}
}
