using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Odin drawer for <see cref="T:Sirenix.OdinInspector.ColorPaletteAttribute" />.
	/// </summary>
	[DrawerPriority(DrawerPriorityLevel.AttributePriority)]
	public sealed class ColorPaletteAttributeDrawer : OdinAttributeDrawer<ColorPaletteAttribute, Color>
	{
		private int paletteIndex;

		private string currentName;

		private LocalPersistentContext<string> persistentName;

		private bool showAlpha;

		private string[] names;

		private ValueResolver<string> nameGetter;

		/// <summary>
		/// Initializes the drawer.
		/// </summary>
		protected override void Initialize()
		{
			paletteIndex = 0;
			currentName = base.Attribute.PaletteName;
			showAlpha = base.Attribute.ShowAlpha;
			names = GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes.Select((ColorPalette x) => x.Name).ToArray();
			if (base.Attribute.PaletteName == null)
			{
				persistentName = base.ValueEntry.Context.GetPersistent<string>(this, "ColorPaletteName", null);
				List<string> list = names.ToList();
				currentName = persistentName.Value;
				if (currentName != null && list.Contains(currentName))
				{
					paletteIndex = list.IndexOf(currentName);
				}
			}
			else
			{
				nameGetter = ValueResolver.GetForString(base.Property, base.Attribute.PaletteName);
			}
		}

		/// <summary>
		/// Not yet documented.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<Color> entry = base.ValueEntry;
			ColorPaletteAttribute attribute = base.Attribute;
			SirenixEditorGUI.BeginIndentedHorizontal();
			if (label != null)
			{
				GUILayout.Label(label, GUILayoutOptions.Width(GUIHelper.BetterLabelWidth - 4f).ExpandWidth(expand: false));
			}
			else
			{
				GUILayout.Space(5f);
			}
			Rect rect = EditorGUILayout.BeginHorizontal();
			rect.x -= 3f;
			rect.width = 25f;
			entry.SmartValue = SirenixEditorGUI.DrawColorField(rect, entry.SmartValue, useAlphaInPreview: false, showAlpha);
			bool openInEditorShown = false;
			GUILayout.Space(28f);
			SirenixEditorGUI.BeginInlineBox();
			if (attribute.PaletteName == null || GlobalConfig<ColorPaletteManager>.Instance.ShowPaletteName)
			{
				SirenixEditorGUI.BeginToolbarBoxHeader();
				if (attribute.PaletteName == null)
				{
					int newValue = EditorGUILayout.Popup(paletteIndex, names, GUILayoutOptions.ExpandWidth());
					if (paletteIndex != newValue)
					{
						paletteIndex = newValue;
						currentName = names[newValue];
						persistentName.Value = currentName;
						GUIHelper.RemoveFocusControl();
					}
				}
				else
				{
					GUILayout.Label(currentName);
					GUILayout.FlexibleSpace();
				}
				openInEditorShown = true;
				if (SirenixEditorGUI.IconButton(EditorIcons.SettingsCog))
				{
					GlobalConfig<ColorPaletteManager>.Instance.OpenInEditor();
				}
				SirenixEditorGUI.EndToolbarBoxHeader();
			}
			ColorPalette colorPalette = ((attribute.PaletteName != null) ? GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes.FirstOrDefault((ColorPalette x) => x.Name == nameGetter.GetValue()) : GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes.FirstOrDefault((ColorPalette x) => x.Name == names[paletteIndex]));
			if (colorPalette == null)
			{
				GUILayout.BeginHorizontal();
				if (attribute.PaletteName != null && GUILayout.Button("Create color palette: " + nameGetter.GetValue()))
				{
					GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes.Add(new ColorPalette
					{
						Name = nameGetter.GetValue()
					});
					GlobalConfig<ColorPaletteManager>.Instance.OpenInEditor();
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				currentName = colorPalette.Name;
				showAlpha = attribute.ShowAlpha && colorPalette.ShowAlpha;
				if (!openInEditorShown)
				{
					GUILayout.BeginHorizontal();
				}
				Color color = entry.SmartValue;
				bool stretch = GlobalConfig<ColorPaletteManager>.Instance.StretchPalette;
				int size = GlobalConfig<ColorPaletteManager>.Instance.SwatchSize;
				int margin = GlobalConfig<ColorPaletteManager>.Instance.SwatchSpacing;
				if (DrawColorPaletteColorPicker(entry, colorPalette, ref color, colorPalette.ShowAlpha, stretch, size, 20f, margin))
				{
					entry.SmartValue = color;
				}
				if (!openInEditorShown)
				{
					GUILayout.Space(4f);
					if (SirenixEditorGUI.IconButton(EditorIcons.SettingsCog))
					{
						GlobalConfig<ColorPaletteManager>.Instance.OpenInEditor();
					}
					GUILayout.EndHorizontal();
				}
			}
			SirenixEditorGUI.EndInlineBox();
			EditorGUILayout.EndHorizontal();
			SirenixEditorGUI.EndIndentedHorizontal();
		}

		internal static bool DrawColorPaletteColorPicker(object key, ColorPalette colorPalette, ref Color color, bool drawAlpha, bool stretchPalette, float width = 20f, float height = 20f, float margin = 0f)
		{
			bool result = false;
			Rect rect = SirenixEditorGUI.BeginHorizontalAutoScrollBox(key, GUILayoutOptions.ExpandWidth().ExpandHeight(expand: false));
			if (stretchPalette)
			{
				rect.width -= margin * (float)colorPalette.Colors.Count - margin;
				width = Mathf.Max(width, rect.width / (float)colorPalette.Colors.Count);
			}
			bool isMouseDown = Event.current.type == EventType.MouseDown;
			Rect innerRect = GUILayoutUtility.GetRect((width + margin) * (float)colorPalette.Colors.Count, height, GUIStyle.none);
			float spacing = width + margin;
			Rect cellRect = innerRect;
			cellRect.width = width;
			for (int i = 0; i < colorPalette.Colors.Count; i++)
			{
				cellRect.x = spacing * (float)i;
				if (drawAlpha)
				{
					EditorGUIUtility.DrawColorSwatch(cellRect, colorPalette.Colors[i]);
				}
				else
				{
					Color c = colorPalette.Colors[i];
					c.a = 1f;
					SirenixEditorGUI.DrawSolidRect(cellRect, c);
				}
				if (isMouseDown && cellRect.Contains(Event.current.mousePosition))
				{
					color = colorPalette.Colors[i];
					result = true;
					GUI.changed = true;
					Event.current.Use();
				}
			}
			SirenixEditorGUI.EndHorizontalAutoScrollBox();
			return result;
		}
	}
}
