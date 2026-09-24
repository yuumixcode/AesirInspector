namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class EnableInAttributeStateUpdater : AttributeStateUpdater<EnableInAttribute>
	{
		private bool disable;

		protected override void Initialize()
		{
			disable = (OdinPrefabUtility.GetPrefabKind(base.Property) & base.Attribute.PrefabKind) == 0;
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
