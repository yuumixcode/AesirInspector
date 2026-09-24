using Sirenix.OdinInspector.Editor.ActionResolvers;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class OnInspectorInitStateUpdater : AttributeStateUpdater<OnInspectorInitAttribute>
	{
		protected override void Initialize()
		{
			ActionResolver action = ActionResolver.Get(base.Property, base.Attribute.Action);
			action.DoActionForAllSelectionIndices();
			ErrorMessage = action.ErrorMessage;
		}
	}
}
