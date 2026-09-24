using UnityEngine;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class HideInInspectorAttributeStateUpdater : AttributeStateUpdater<HideInInspector>
	{
		private bool showInInspectorAttribute;

		private PropertyContext<bool> isInReference;

		protected override void Initialize()
		{
			showInInspectorAttribute = base.Property.Attributes.HasAttribute<ShowInInspectorAttribute>();
		}

		public override void OnStateUpdate()
		{
			if (showInInspectorAttribute || (base.Property.Parent != null && base.Property.Parent.ChildResolver is ICollectionResolver))
			{
				base.Property.State.Visible = true;
				return;
			}
			if (isInReference == null)
			{
				isInReference = base.Property.Context.GetGlobal("is_in_reference", defaultValue: false);
			}
			if (isInReference.Value)
			{
				base.Property.State.Visible = true;
			}
			else
			{
				base.Property.State.Visible = false;
			}
		}
	}
}
