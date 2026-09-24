namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class HideInAttributeStateUpdater : AttributeStateUpdater<HideInAttribute>
	{
		private bool hide;

		protected override void Initialize()
		{
			hide = (OdinPrefabUtility.GetPrefabKind(base.Property) & base.Attribute.PrefabKind) != 0;
		}

		public override void OnStateUpdate()
		{
			base.Property.State.Visible = !hide;
		}
	}
}
