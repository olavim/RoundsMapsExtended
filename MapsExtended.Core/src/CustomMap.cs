using MapsExt.MapObjects;
using System;
using UnityEngine;

namespace MapsExt
{
	public class CustomMap : ISerializationCallbackReceiver
	{
		[SerializeField] private readonly string _id;
		[SerializeField] private readonly string _name;
		[SerializeField] private readonly string _version;
		[SerializeField] private CustomMapSettings _settings;
		[SerializeField] private readonly MapObjectData[] _mapObjects;

		[Obsolete("Deprecated")]
		[NonSerialized]
		public string id;

		public string Id => this._id;
		public string Name => this._name;
		public string Version => this._version;
		public CustomMapSettings Settings => this._settings;
		public MapObjectData[] MapObjects => this._mapObjects;

		public CustomMap(string id, string name, string version, CustomMapSettings settings, MapObjectData[] mapObjects)
		{
			this._id = id;
			this._name = name;
			this._settings = settings;
			this._mapObjects = mapObjects;
			this._version = version;

#pragma warning disable CS0618
			this.id = id;
#pragma warning restore CS0618
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (this._mapObjects == null)
			{
				throw new Exception($"Could not load map: {this.Name ?? "<unnamed>"} ({this.Id})");
			}

			if (this._settings == null) {
				this._settings = new CustomMapSettings();
			}

			for (int i = 0; i < this._mapObjects.Length; i++)
			{
				if (this._mapObjects[i] == null)
				{
					throw new Exception($"Could not load map: {this.Name ?? "<unnamed>"} ({this.Id}) (index: {i})");
				}
			}

#pragma warning disable CS0618
			this.id = this._id;
#pragma warning restore CS0618
		}

		public void OnBeforeSerialize() { }
	}
}
