using MapsExt.Editor.UI;
using MapsExt.Properties;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(DamageableProperty))]
	public class EditorDamageablePropertySerializer : DamageablePropertySerializer { }

	[PropertyInspector(typeof(DamageableProperty))]
	public class DamageableElement : BooleanElement
	{
		public DamageableElement() : base("Damageable by Environment") { }

		protected override bool GetValue() => this.Context.InspectorTarget.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment;

		protected override void OnChange(bool value)
		{
			this.Context.InspectorTarget.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment = value;
			this.Context.Editor.TakeSnaphot();
		}
	}
}
