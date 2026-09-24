namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class HideInPrefabInstancesAttributeStateUpdater : AttributeStateUpdater<HideInPrefabInstancesAttribute>
	{
		private bool hide;

		protected override void Initialize()
		{
			hide = (OdinPrefabUtility.GetPrefabKind(base.Property) & PrefabKind.PrefabInstance) != 0;
		}

		public override void OnStateUpdate()
		{
			base.Property.State.Visible = !hide;
		}
	}
}
