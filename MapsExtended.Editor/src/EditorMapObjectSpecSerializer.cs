using HarmonyLib;
using MapsExt.MapObjects;
using System;

namespace MapsExt.Editor
{
	[Obsolete("Deprecated")]
	public class EditorMapObjectSpecSerializer : MapObjectSpecSerializer, IEditorMapObjectSerializer
	{
		protected SerializerAction<MapObject> Serializer { get; }

		public EditorMapObjectSpecSerializer(SerializerAction<MapObject> serializer, DeserializerAction<MapObject> deserializer) : base(deserializer)
		{
			this.Serializer = serializer ?? throw new ArgumentException("Serializer cannot be null");
		}

		public MapObjectData Serialize(MapObjectInstance mapObjectInstance)
		{
			try
			{
				var data = (MapObject) AccessTools.CreateInstance(mapObjectInstance.DataType);

				data.active = mapObjectInstance.gameObject.activeSelf;

				this.Serializer(mapObjectInstance.gameObject, data);
				return data;
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not serialize {mapObjectInstance.gameObject.name}", ex);
			}
		}
	}
}
