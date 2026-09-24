namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class DisableInNonPrefabsAttributeStateUpdater : AttributeStateUpdater<DisableInNonPrefabsAttribute>
	{
		private bool disable;

		protected override void Initialize()
		{
			disable = (OdinPrefabUtility.GetPrefabKind(base.Property) & PrefabKind.NonPrefabInstance) != 0;
		}

		public override void OnStateUpdate()
		{
			if (base.Property.State.Enabled && disable)
			{
				base.Property.State.Enabled = false;
			}
		}
	}
}
