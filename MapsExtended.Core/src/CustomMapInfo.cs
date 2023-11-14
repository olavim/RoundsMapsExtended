namespace MapsExt
{
	public class CustomMapInfo
	{
		public string Id { get; private set; }
		public string Name { get; private set; }
		public string Version { get; private set; }
		public byte[] Data { get; private set; }

		public CustomMapInfo(string id, string name, string version, byte[] data)
		{
			this.Id = id;
			this.Name = name;
			this.Version = version;
			this.Data = data;
		}
	}
}
