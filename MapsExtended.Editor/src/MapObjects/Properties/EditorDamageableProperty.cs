using MapsExt.Editor.UI;
using MapsExt.MapObjects.Properties;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorPropertySerializer(typeof(DamageableProperty))]
	public class EditorDamageablePropertySerializer : DamageablePropertySerializer { }

	[PropertyInspector(typeof(DamageableProperty))]
	public class DamageableElement : BooleanElement
	{
		protected override bool Value
		{
			get => this.Context.InspectorTarget.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment;
			set => this.Context.InspectorTarget.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment = value;
		}

		public DamageableElement() : base("Damageable by Environment") { }
	}
}
