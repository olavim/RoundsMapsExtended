using UnityEngine;

namespace MapsExt.MapObjects
{
	public abstract class BaseEditorMapObjectBlueprint<T> : BaseMapObjectBlueprint<T> where T : MapObject
	{
		public override GameObject Prefab => this.baseBlueprint.Prefab;
		protected IMapObjectBlueprint baseBlueprint => MapsExtended.instance.mapObjectManager.blueprints[typeof(T)];
	}
}
