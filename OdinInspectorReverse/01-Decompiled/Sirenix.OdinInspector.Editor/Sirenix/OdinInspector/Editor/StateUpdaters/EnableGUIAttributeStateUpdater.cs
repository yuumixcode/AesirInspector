namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class EnableGUIAttributeStateUpdater : AttributeStateUpdater<EnableGUIAttribute>
	{
		public override void OnStateUpdate()
		{
			base.Property.State.Enabled = true;
		}
	}
}
