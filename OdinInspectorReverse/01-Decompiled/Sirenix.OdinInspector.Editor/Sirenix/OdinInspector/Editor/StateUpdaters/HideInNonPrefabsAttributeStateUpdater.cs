namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class HideInNonPrefabsAttributeStateUpdater : AttributeStateUpdater<HideInNonPrefabsAttribute>
	{
		private bool hide;

		protected override void Initialize()
		{
			PrefabKind kind = OdinPrefabUtility.GetPrefabKind(base.Property);
			hide = kind == PrefabKind.None || (kind & PrefabKind.NonPrefabInstance) != 0;
		}

		public override void OnStateUpdate()
		{
			base.Property.State.Visible = !hide;
		}
	}
}
