using MapsExt.Editor.UI;
using MapsExt.Properties;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(DamageableProperty))]
	public class EditorDamageablePropertySerializer : DamageablePropertySerializer { }

	[PropertyInspector(typeof(DamageableProperty))]
	public class DamageableElement : BooleanElement
	{
		public override bool Value
		{
			get => this.Context.InspectorTarget.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment;
			set => this.Context.InspectorTarget.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment = value;
		}

		public DamageableElement() : base("Damageable by Environment") { }
	}
}
