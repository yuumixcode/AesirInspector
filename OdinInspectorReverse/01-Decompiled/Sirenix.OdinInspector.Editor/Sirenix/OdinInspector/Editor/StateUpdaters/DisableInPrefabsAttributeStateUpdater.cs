namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class DisableInPrefabsAttributeStateUpdater : AttributeStateUpdater<DisableInPrefabsAttribute>
	{
		private bool disable;

		protected override void Initialize()
		{
			disable = ((OdinPrefabUtility.GetPrefabKind(base.Property) & PrefabKind.PrefabAsset) | PrefabKind.PrefabInstance) != PrefabKind.None;
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
