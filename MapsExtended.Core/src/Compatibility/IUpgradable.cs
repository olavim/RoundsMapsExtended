namespace MapsExt.Compatibility
{
	public interface IUpgradable<T>
	{
		T Upgrade();
	}
}