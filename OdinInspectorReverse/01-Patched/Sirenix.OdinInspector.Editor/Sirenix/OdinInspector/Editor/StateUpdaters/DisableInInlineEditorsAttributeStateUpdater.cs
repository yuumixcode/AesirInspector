using Sirenix.OdinInspector.Editor.Drawers;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class DisableInInlineEditorsAttributeStateUpdater : AttributeStateUpdater<DisableInInlineEditorsAttribute>
	{
		public override void OnStateUpdate()
		{
			if (base.Property.State.Enabled && InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth > 0)
			{
				base.Property.State.Enabled = false;
			}
		}
	}
}
