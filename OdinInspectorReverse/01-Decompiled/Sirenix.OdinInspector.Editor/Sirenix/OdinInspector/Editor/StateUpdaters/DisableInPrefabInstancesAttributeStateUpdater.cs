namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class DisableInPrefabInstancesAttributeStateUpdater : AttributeStateUpdater<DisableInPrefabInstancesAttribute>
	{
		private bool disable;

		protected override void Initialize()
		{
			disable = (OdinPrefabUtility.GetPrefabKind(base.Property) & PrefabKind.PrefabInstance) != 0;
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
