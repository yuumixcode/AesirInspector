using System;
using Sirenix.OdinInspector.Editor.ActionResolvers;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class OnInspectorDisposeStateUpdater : AttributeStateUpdater<OnInspectorDisposeAttribute>, IDisposable
	{
		private ActionResolver action;

		protected override void Initialize()
		{
			action = ActionResolver.Get(base.Property, base.Attribute.Action);
			ErrorMessage = action.ErrorMessage;
		}

		public void Dispose()
		{
			action.DoActionForAllSelectionIndices();
		}
	}
}
