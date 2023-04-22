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
			var boundsArr = gameObjects.Select(obj => obj.GetComponent<SelectionHandler>().GetBounds()).ToArray();
			var bounds = boundsArr[0];
			for (var i = 1; i < boundsArr.Length; i++)
			{
				bounds.Encapsulate(boundsArr[i]);
			}

			this.transform.localScale = bounds.size;
		}
	}
}
