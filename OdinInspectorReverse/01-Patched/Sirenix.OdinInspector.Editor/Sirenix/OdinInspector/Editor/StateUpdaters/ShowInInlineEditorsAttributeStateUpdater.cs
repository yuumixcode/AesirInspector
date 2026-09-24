using Sirenix.OdinInspector.Editor.Drawers;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class ShowInInlineEditorsAttributeStateUpdater : AttributeStateUpdater<ShowInInlineEditorsAttribute>
	{
		public override void OnStateUpdate()
		{
			base.Property.State.Visible = InlineEditorAttributeDrawer.CurrentInlineEditorDrawDepth > 0;
		}
	}
}
