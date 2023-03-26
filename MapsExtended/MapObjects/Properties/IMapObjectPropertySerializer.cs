using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface IMapObjectPropertySerializer
	{
		void Serialize(GameObject instance, IMapObjectProperty property);
		void Deserialize(IMapObjectProperty property, GameObject target);
	}

	public interface IMapObjectPropertySerializer<T> : IMapObjectPropertySerializer where T : IMapObjectProperty
	{
		void Serialize(GameObject instance, T property);
		void Deserialize(T property, GameObject target);
	}

	public abstract class MapObjectPropertySerializer<T> : IMapObjectPropertySerializer<T> where T : IMapObjectProperty
	{
		public void Deserialize(IMapObjectProperty property, GameObject target)
		{
			this.Deserialize((T) property, target);
		}

		void IMapObjectPropertySerializer.Serialize(GameObject instance, IMapObjectProperty property)
		{
			this.Serialize(instance, (T) property);
		}

		public abstract void Deserialize(T property, GameObject target);
		public abstract void Serialize(GameObject instance, T property);
	}
}
