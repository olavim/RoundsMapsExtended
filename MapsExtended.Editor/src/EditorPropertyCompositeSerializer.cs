using MapsExt.MapObjects;
using System;

namespace MapsExt.Editor
{
	public class EditorPropertyCompositeSerializer : PropertyCompositeSerializer, IEditorMapObjectSerializer
	{
		private readonly EditorPropertyManager _propertyManager;

		public EditorPropertyCompositeSerializer(EditorPropertyManager propertyManager) : base(propertyManager)
		{
			this._propertyManager = propertyManager;
		}

		public MapObjectData Serialize(MapObjectInstance mapObjectInstance)
		{
			try
			{
				var data = this._propertyManager.ReadMapObject(mapObjectInstance);
				data.mapObjectId = mapObjectInstance.MapObjectId;
				data.active = mapObjectInstance.gameObject.activeSelf;
				return data;
			}
			catch (Exception ex)
			{
				throw new MapObjectSerializationException($"Could not serialize map object: {mapObjectInstance.gameObject.name}", ex);
			}
		}
	}
}
