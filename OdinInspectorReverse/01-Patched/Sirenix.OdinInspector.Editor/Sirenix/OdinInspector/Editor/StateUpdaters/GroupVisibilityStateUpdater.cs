using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor.StateUpdaters
{
	public sealed class GroupVisibilityStateUpdater<TAttr> : AttributeStateUpdater<TAttr>, IOnChildStateChangedNotification where TAttr : PropertyGroupAttribute
	{
		private ValueResolver<bool> visibleIfResolver;

		private IfAttributeHelper visibleIfHelper;

		private object helperValue;

		private bool negateVisibleIf;

		public override bool CanUpdateProperty(InspectorProperty property)
		{
			return property.Info.PropertyType == PropertyType.Group;
		}

		protected override void Initialize()
		{
			base.Property.AnimateVisibility = base.Attribute.AnimateVisibility;
			if (base.Attribute is ShowIfGroupAttribute)
			{
				visibleIfHelper = new IfAttributeHelper(base.Property, (base.Attribute as ShowIfGroupAttribute).Condition, defaultResult: true);
				ErrorMessage = visibleIfHelper.ErrorMessage;
				helperValue = (base.Attribute as ShowIfGroupAttribute).Value;
			}
			else if (base.Attribute is HideIfGroupAttribute)
			{
				visibleIfHelper = new IfAttributeHelper(base.Property, (base.Attribute as HideIfGroupAttribute).Condition);
				ErrorMessage = visibleIfHelper.ErrorMessage;
				helperValue = (base.Attribute as HideIfGroupAttribute).Value;
				negateVisibleIf = true;
			}
			else
			{
				visibleIfResolver = ValueResolver.Get(base.Property, base.Attribute.VisibleIf, fallbackValue: true);
				ErrorMessage = visibleIfResolver.ErrorMessage;
			}
			UpdateVisibility();
		}

		public override void OnStateUpdate()
		{
			if (visibleIfResolver != null || visibleIfHelper != null)
			{
				UpdateVisibility();
			}
		}

		public void OnChildStateChanged(int childIndex, string state)
		{
			if (state == "Visible")
			{
				UpdateVisibility();
			}
		}

		public void UpdateVisibility()
		{
			if (visibleIfResolver != null || visibleIfHelper != null)
			{
				bool resolvedValue;
				if (visibleIfResolver != null)
				{
					resolvedValue = visibleIfResolver.GetValue();
					ErrorMessage = visibleIfResolver.ErrorMessage;
				}
				else
				{
					resolvedValue = visibleIfHelper.GetValue(helperValue);
					ErrorMessage = visibleIfHelper.ErrorMessage;
				}
				if (negateVisibleIf)
				{
					resolvedValue = !resolvedValue;
				}
				base.Property.State.Visible = resolvedValue;
				if (base.Attribute.HideWhenChildrenAreInvisible && base.Property.State.Visible)
				{
					base.Property.State.Visible &= AreAnyChildrenVisible();
				}
			}
			else if (base.Attribute.HideWhenChildrenAreInvisible)
			{
				base.Property.State.Visible = AreAnyChildrenVisible();
			}
		}

		public bool AreAnyChildrenVisible()
		{
			bool anyChildrenVisible = false;
			for (int i = 0; i < base.Property.Children.Count; i++)
			{
				if (base.Property.Children[i].State.Visible)
				{
					anyChildrenVisible = true;
					break;
				}
			}
			return anyChildrenVisible;
		}
	}
}
