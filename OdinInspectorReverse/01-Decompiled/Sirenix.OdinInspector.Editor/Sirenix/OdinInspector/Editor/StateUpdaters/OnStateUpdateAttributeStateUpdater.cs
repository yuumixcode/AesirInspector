using Sirenix.OdinInspector.Editor.ActionResolvers;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class OnStateUpdateAttributeStateUpdater : AttributeStateUpdater<OnStateUpdateAttribute>
	{
		private ActionResolver action;

		protected override void Initialize()
		{
			action = ActionResolver.Get(base.Property, base.Attribute.Action);
			ErrorMessage = action.ErrorMessage;
		}

		public override void OnStateUpdate()
		{
			action.DoActionForAllSelectionIndices();
			ErrorMessage = action.ErrorMessage;
		}
	}
}
