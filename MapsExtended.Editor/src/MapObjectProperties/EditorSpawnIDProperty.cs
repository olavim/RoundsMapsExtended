using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(SpawnIDProperty))]
	public class EditorSpawnIDPropertySerializer : SpawnIDPropertySerializer, IPropertyReader<SpawnIDProperty>
	{
		public virtual SpawnIDProperty ReadProperty(GameObject instance)
		{
			var spawnPoint = instance.gameObject.GetComponent<SpawnPoint>();
			return new()
			{
				Id = spawnPoint.ID,
				TeamId = spawnPoint.TEAMID
			};
		}
	}
}
