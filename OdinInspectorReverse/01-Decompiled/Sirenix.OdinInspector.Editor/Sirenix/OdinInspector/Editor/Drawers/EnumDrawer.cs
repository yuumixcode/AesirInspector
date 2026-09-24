using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Enum property drawer.
	/// </summary>
	public sealed class EnumDrawer<T> : OdinValueDrawer<T>
	{
		/// <summary>
		/// Returns <c>true</c> if the drawer can draw the type.
		/// </summary>
		public override bool CanDrawTypeFilter(Type type)
		{
			return type.IsEnum;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			if (GlobalConfig<GeneralDrawerConfig>.Instance.UseImprovedEnumDropDown)
			{
				entry.SmartValue = EnumSelector<T>.DrawEnumField(label, entry.SmartValue);
			}
			else
			{
				entry.WeakSmartValue = SirenixEditorFields.EnumDropdown(label, (Enum)entry.WeakSmartValue);
			}
		}
	}
}
