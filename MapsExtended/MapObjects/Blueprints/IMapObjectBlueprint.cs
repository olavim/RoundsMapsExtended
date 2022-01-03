using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObjectBlueprint
	{
		GameObject Prefab { get; }
		void Serialize(GameObject instance, MapObject target);
		void Deserialize(MapObject data, GameObject target);
	}

	public interface IMapObjectBlueprint<T> : IMapObjectBlueprint where T : MapObject
	{
		void Serialize(GameObject instance, T target);
		void Deserialize(T data, GameObject target);
	}

	public abstract class BaseMapObjectBlueprint<T> : IMapObjectBlueprint<T> where T : MapObject
	{
		public abstract GameObject Prefab { get; }

		public void Deserialize(MapObject data, GameObject target) => this.Deserialize((T) data, target);
		public void Serialize(GameObject instance, MapObject target) => this.Serialize(instance, (T) target);

		public abstract void Deserialize(T data, GameObject target);
		public abstract void Serialize(GameObject instance, T target);
	}
}
