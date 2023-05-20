namespace MapsExt.Compatibility
{
	internal interface IUpgradable<T>
	{
		T Upgrade();
	}
}