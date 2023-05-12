namespace MapsExt.Properties
{
	public interface IPropertySerializer<T> : IPropertyWriter<T>, IPropertyReader<T> where T : IProperty { }
}
