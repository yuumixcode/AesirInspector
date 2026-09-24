using Sirenix.OdinInspector.Editor.ActionResolvers;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />.
	/// Calls the method, the attribute is either attached to, or the method that has been specified in the attribute, to allow for custom GUI drawing.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnValueChangedAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ValidateInputAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DrawWithUnityAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InlineEditorAttribute" />
	[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
	public sealed class OnInspectorGUIAttributeDrawer : OdinAttributeDrawer<OnInspectorGUIAttribute>
	{
		private ActionResolver propertyMethod;

		private ActionResolver prependGUI;

		private ActionResolver appendGUI;

		protected override void Initialize()
		{
			if (base.Property.Info.PropertyType == PropertyType.Method)
			{
				propertyMethod = ActionResolver.Get(base.Property, null);
				return;
			}
			if (base.Attribute.Prepend != null)
			{
				prependGUI = ActionResolver.Get(base.Property, base.Attribute.Prepend);
			}
			if (base.Attribute.Append != null)
			{
				appendGUI = ActionResolver.Get(base.Property, base.Attribute.Append);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (base.Property.Info.PropertyType == PropertyType.Method)
			{
				if (propertyMethod.HasError)
				{
					propertyMethod.DrawError();
				}
				else
				{
					propertyMethod.DoAction();
				}
				return;
			}
			ActionResolver.DrawErrors(prependGUI, appendGUI);
			if (prependGUI != null && !prependGUI.HasError)
			{
				prependGUI.DoAction();
			}
			CallNextDrawer(label);
			if (appendGUI != null && !appendGUI.HasError)
			{
				appendGUI.DoAction();
			}
		}
	}
}
