namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class HideInPrefabAssetsAttributeStateUpdater : AttributeStateUpdater<HideInPrefabAssetsAttribute>
	{
		private bool hide;

		protected override void Initialize()
		{
			hide = (OdinPrefabUtility.GetPrefabKind(base.Property) & PrefabKind.PrefabAsset) != 0;
		}

		public override void OnStateUpdate()
		{
			base.Property.State.Visible = !hide;
		}
	}
}
