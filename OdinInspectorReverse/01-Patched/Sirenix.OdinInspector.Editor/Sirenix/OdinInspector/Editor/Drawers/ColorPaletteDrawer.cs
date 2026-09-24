using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Color palette property drawer.
	/// </summary>
	internal sealed class ColorPaletteDrawer : OdinValueDrawer<ColorPalette>
	{
		private bool isEditing;

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<ColorPalette> entry = base.ValueEntry;
			entry.SmartValue.Name = entry.SmartValue.Name ?? "Palette Name";
			SirenixEditorGUI.BeginBox();
			SirenixEditorGUI.BeginToolbarBoxHeader();
			GUILayout.Label(entry.SmartValue.Name);
			GUILayout.FlexibleSpace();
			if (SirenixEditorGUI.IconButton(EditorIcons.Pen))
			{
				isEditing = !isEditing;
			}
			SirenixEditorGUI.EndToolbarBoxHeader();
			if (entry.SmartValue.Colors == null)
			{
				entry.SmartValue.Colors = new List<Color>();
			}
			if (SirenixEditorGUI.BeginFadeGroup(entry.SmartValue, entry, isEditing))
			{
				CallNextDrawer(null);
			}
			SirenixEditorGUI.EndFadeGroup();
			if (SirenixEditorGUI.BeginFadeGroup(entry.SmartValue, entry.SmartValue, !isEditing))
			{
				Color col = default(Color);
				bool stretch = GlobalConfig<ColorPaletteManager>.Instance.StretchPalette;
				int size = GlobalConfig<ColorPaletteManager>.Instance.SwatchSize;
				int margin = GlobalConfig<ColorPaletteManager>.Instance.SwatchSpacing;
				ColorPaletteAttributeDrawer.DrawColorPaletteColorPicker(entry, entry.SmartValue, ref col, entry.SmartValue.ShowAlpha, stretch, size, 20f, margin);
			}
			SirenixEditorGUI.EndFadeGroup();
			SirenixEditorGUI.EndToolbarBox();
		}
	}
}
