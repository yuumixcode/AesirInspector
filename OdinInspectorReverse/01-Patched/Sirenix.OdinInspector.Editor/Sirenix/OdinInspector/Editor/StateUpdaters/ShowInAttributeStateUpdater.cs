namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class ShowInAttributeStateUpdater : AttributeStateUpdater<ShowInAttribute>
	{
		private bool show;

		protected override void Initialize()
		{
			show = (OdinPrefabUtility.GetPrefabKind(base.Property) & base.Attribute.PrefabKind) != 0;
		}

		public override void OnStateUpdate()
		{
			base.Property.State.Visible = show;
		}
	}
}
