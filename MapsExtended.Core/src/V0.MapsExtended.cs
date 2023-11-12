using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using UnityEngine;
using MapsExt.MapObjects;
using MapsExt.Utils;

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

			public virtual void OnWrite(GameObject instance, MapObjectData data) { }
		}

		private void RegisterV0MapObjects(Assembly assembly)
		{
			Type[] types;
			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				types = e.Types.Where(t => t != null).ToArray();
			}

			foreach (var type in types.Where(t => Attribute.IsDefined(t, typeof(MapObjectSpec))))
			{
				try
				{
					var attr = type.GetCustomAttribute<MapObjectSpec>();
					var prefab = ReflectionUtils.GetAttributedProperty<GameObject>(type, typeof(MapObjectPrefab));
					var deserializerAction = ReflectionUtils.GetAttributedMethod<DeserializerAction<MapObjectData>>(type, typeof(MapObjectDeserializer));
					var serializerAction = ReflectionUtils.GetAttributedMethod<SerializerAction<MapObjectData>>(type, typeof(MapObjectSerializer));

					var serializer = new MapObjectSpecSerializer(deserializerAction, serializerAction);

					this._mapObjectManager.RegisterMapObject(attr.dataType, new VirtualMapObject(prefab), serializer);
				}
				catch (Exception ex)
				{
					MapsExtended.Log.LogError($"Could not register legacy map object {type.Name}: {ex.Message}");

#if DEBUG
					MapsExtended.Log.LogError(ex.StackTrace);
#endif
				}
			}
		}
	}
}

#pragma warning restore CS0618
