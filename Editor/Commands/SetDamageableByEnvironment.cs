using MapsExt.Editor.ActionHandlers;
using MapsExt.MapObjects;
using System.Collections;

namespace MapsExt.Editor.Commands
{
	public class SetDamageableByEnvironmentCommand : ICommand
	{
		public readonly DamageableMapObject data;
		public readonly bool toValue;

		public SetDamageableByEnvironmentCommand(EditorActionHandler handler, bool damageable)
		{
			this.data = (DamageableMapObject) MapsExtendedEditor.instance.mapObjectManager.Serialize(handler.gameObject);
			this.toValue = damageable;
		}

		public SetDamageableByEnvironmentCommand(SetDamageableByEnvironmentCommand cmd, bool damageable)
		{
			this.data = cmd.data;
			this.toValue = damageable;
		}
	}

	public class SetDamageableByEnvironmentCommandHandler : CommandHandler<SetDamageableByEnvironmentCommand>
	{
		public MapEditor editor;

		public SetDamageableByEnvironmentCommandHandler(MapEditor editor)
		{
			this.editor = editor;
		}

		public override IEnumerator Execute(SetDamageableByEnvironmentCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content);
			instance.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment = cmd.toValue;
			yield break;
		}

		public override IEnumerator Undo(SetDamageableByEnvironmentCommand cmd)
		{
			var instance = cmd.data.FindInstance(this.editor.content);
			instance.GetComponent<DamageableMapObjectInstance>().damageableByEnvironment = cmd.data.damageableByEnvironment;
			yield break;
		}

		public override SetDamageableByEnvironmentCommand Merge(SetDamageableByEnvironmentCommand cmd1, SetDamageableByEnvironmentCommand cmd2)
		{
			return new SetDamageableByEnvironmentCommand(cmd1, cmd1.toValue || cmd2.toValue);
		}

		public override bool IsRedundant(SetDamageableByEnvironmentCommand cmd)
		{
			return cmd.toValue == cmd.data.damageableByEnvironment;
		}
	}
}
