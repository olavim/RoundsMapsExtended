namespace MapsExt.Compatibility
{
	[System.Obsolete("Use `public static implicit operator T()` instead")]
	public interface IUpgradable<T>
	{
		T Upgrade();
	}
}