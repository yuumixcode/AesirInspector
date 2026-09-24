using Sirenix.OdinInspector.Editor.Drawers;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class HideInInlineEditorsAttributeStateUpdater : AttributeStateUpdater<HideInInlineEditorsAttribute>
	{
		public override void OnStateUpdate()
		{
			base.Property.State.Visible = InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth <= 0;
		}
	}
}
