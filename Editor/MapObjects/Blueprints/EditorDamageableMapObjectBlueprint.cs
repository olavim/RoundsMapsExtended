using MapsExt.MapObjects;
using MapsExt.Editor.UI;

namespace MapsExt.Editor.MapObjects
{
	public abstract class EditorDamageableMapObjectBlueprint<T> : EditorSpatialMapObjectBlueprint<T> where T : DamageableMapObject
	{
		public override void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			base.OnInspectorLayout(inspector, builder);
			int dividerIndex = builder.propertyBuilders.FindIndex(el => el is InspectorDividerBuilder);

			var propBuilder = builder.Property<bool>("Damageable by Environment")
				.ValueSetter(value => inspector.target.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment = value)
				.ValueGetter(() => inspector.target.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment);

			builder.propertyBuilders.Insert(dividerIndex, propBuilder);
		}
	}
}
