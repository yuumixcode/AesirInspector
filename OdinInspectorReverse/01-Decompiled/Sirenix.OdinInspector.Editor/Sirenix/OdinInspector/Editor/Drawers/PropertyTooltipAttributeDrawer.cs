using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.PropertyTooltipAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyTooltipAttribute" />
	[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
	public sealed class PropertyTooltipAttributeDrawer : OdinAttributeDrawer<PropertyTooltipAttribute>
	{
		private ValueResolver<string> tooltipResolver;

		protected override void Initialize()
		{
			tooltipResolver = ValueResolver.GetForString(base.Property, base.Attribute.Tooltip);
		}

		protected override bool CanDrawAttributeProperty(InspectorProperty property)
		{
			return property.Info.PropertyType != PropertyType.Method;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (label != null)
			{
				if (tooltipResolver.HasError)
				{
					SirenixEditorGUI.MessageBox(tooltipResolver.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				}
				label.tooltip = tooltipResolver.GetValue();
			}
			CallNextDrawer(label);
		}
	}
}
