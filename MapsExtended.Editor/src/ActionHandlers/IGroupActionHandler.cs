using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	public interface IGroupMapObjectActionHandler
	{
		void Initialize(IEnumerable<GameObject> gameObjects);
	}
}
