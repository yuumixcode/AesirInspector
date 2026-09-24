using Sirenix.OdinInspector.Editor.Drawers;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class EnableIfAttributeStateUpdater : AttributeStateUpdater<EnableIfAttribute>
	{
		private IfAttributeHelper helper;

		protected override void Initialize()
		{
			helper = new IfAttributeHelper(base.Property, base.Attribute.Condition, defaultResult: true);
			ErrorMessage = helper.ErrorMessage;
		}

		public override void OnStateUpdate()
		{
			base.Property.State.Enabled = helper.GetValue(base.Attribute.Value);
			ErrorMessage = helper.ErrorMessage;
		}
	}
}
