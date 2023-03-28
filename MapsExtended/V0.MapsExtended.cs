using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine;
using MapsExt.MapObjects;

#pragma warning disable CS0618

namespace MapsExt
{
	public sealed partial class MapsExtended : BaseUnityPlugin
	{
		class VirtualMapObject : IMapObject
		{
			public GameObject Prefab { get; }

			public VirtualMapObject(GameObject prefab)
			{
				this.Prefab = prefab;
			}

			public void OnInstantiate(GameObject instance) { }
		}

		private void RegisterV0MapObjects(Assembly assembly)
		{
			var types = assembly.GetTypes();
			foreach (var type in types.Where(t => t.GetCustomAttribute<MapObjectSpec>() != null))
			{
				try
				{
					var attr = type.GetCustomAttribute<MapObjectSpec>();
					var prefab = ReflectionUtils.GetAttributedProperty<GameObject>(type, typeof(MapObjectPrefab));
					var serializerAction = ReflectionUtils.GetAttributedMethod<SerializerAction<MapObject>>(type, typeof(MapObjectSerializer));
					var deserializerAction = ReflectionUtils.GetAttributedMethod<DeserializerAction<MapObject>>(type, typeof(MapObjectDeserializer));

					var serializer = new MapObjectSpecSerializer(serializerAction, deserializerAction);

					this.mapObjectManager.RegisterMapObject(attr.dataType, new VirtualMapObject(prefab), serializer);
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError($"Could not register legacy map object {type.Name}: {ex.Message}");

#if DEBUG
					UnityEngine.Debug.LogError(ex.StackTrace);
#endif
				}
			}
		}
	}
}

#pragma warning restore CS0618
