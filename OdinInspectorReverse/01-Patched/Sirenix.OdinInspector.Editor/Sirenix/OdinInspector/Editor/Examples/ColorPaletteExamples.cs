using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ColorPaletteAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Linq", "System.Collections.Generic" })]
	internal class ColorPaletteExamples
	{
		[Serializable]
		public class ColorPalette
		{
			[HideInInspector]
			public string Name;

			[LabelText("$Name")]
			[ListDrawerSettings(IsReadOnly = true, ShowFoldout = false)]
			public Color[] Colors;
		}

		[ColorPalette]
		public Color ColorOptions;

		[ColorPalette("Underwater")]
		public Color UnderwaterColor;

		[ColorPalette("My Palette")]
		public Color MyColor;

		public string DynamicPaletteName = "Clovers";

		[ColorPalette("$DynamicPaletteName")]
		public Color DynamicPaletteColor;

		[ColorPalette("Fall")]
		[HideLabel]
		public Color WideColorPalette;

		[ColorPalette("Clovers")]
		public Color[] ColorArray;

		[FoldoutGroup("Color Palettes", false, 0f)]
		[ListDrawerSettings(IsReadOnly = true)]
		[PropertyOrder(9f)]
		public List<ColorPalette> ColorPalettes;

		[FoldoutGroup("Color Palettes", 0f)]
		[Button(ButtonSizes.Large)]
		[GUIColor(0f, 1f, 0f, 1f)]
		[PropertyOrder(8f)]
		private void FetchColorPalettes()
		{
			ColorPalettes = GlobalConfig<ColorPaletteManager>.Instance.ColorPalettes.Select((Sirenix.OdinInspector.Editor.ColorPalette x) => new ColorPalette
			{
				Name = x.Name,
				Colors = x.Colors.ToArray()
			}).ToList();
		}
	}
}
