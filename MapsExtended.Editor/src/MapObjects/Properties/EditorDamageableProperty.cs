using MapsExt.Editor.UI;
using MapsExt.MapObjects.Properties;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorPropertySerializer]
	public class EditorDamageablePropertySerializer : DamageablePropertySerializer, IInspectable
	{
		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			var propBuilder = builder.Property<bool>("Damageable by Environment")
				.ValueSetter(value => inspector.target.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment = value)
				.ValueGetter(() => inspector.target.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment);
		}
	}
}
