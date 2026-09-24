using Sirenix.OdinInspector.Editor.Drawers;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class ShowIfAttributeStateUpdater : AttributeStateUpdater<ShowIfAttribute>
	{
		private IfAttributeHelper helper;

		protected override void Initialize()
		{
			helper = new IfAttributeHelper(base.Property, base.Attribute.Condition, defaultResult: true);
			ErrorMessage = helper.ErrorMessage;
			base.Property.AnimateVisibility = base.Attribute.Animate;
		}

		public override void OnStateUpdate()
		{
			base.Property.State.Visible = helper.GetValue(base.Attribute.Value);
			ErrorMessage = helper.ErrorMessage;
		}
	}
}
