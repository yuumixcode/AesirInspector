using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ColorPalette 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ColorPaletteExampleSO : AttributeExampleSO<ColorPaletteExampleSO>
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

        [FoldoutGroup("No Parameters")]
        [ColorPalette]
        public Color color1 = Color.white;

        [FoldoutGroup("Parameter: PaletteName")]
        [ColorPalette(PaletteName = "Color3")]
        public Color color2 = Color.white;

        [FoldoutGroup("Parameter: PaletteName")]
        [ColorPalette("Underwater")]
        public Color UnderwaterColor = Color.white;

        [FoldoutGroup("Parameter: PaletteName")]
        [ColorPalette("My Palette")]
        public Color MyColor = Color.white;

        [FoldoutGroup("Parameter: ShowAlpha")]
        [ColorPalette(ShowAlpha = true)]
        public Color color3 = Color.white;

        [FoldoutGroup("Member Reference ($)")]
        public string DynamicPaletteName = "Clovers";

        [FoldoutGroup("Member Reference ($)")]
        [ColorPalette("$DynamicPaletteName")]
        public Color DynamicPaletteColor = Color.white;

        [FoldoutGroup("Combining With HideLabel")]
        [ColorPalette("Fall")]
        [HideLabel]
        public Color WideColorPalette = Color.white;

        [FoldoutGroup("Usage with Collections")]
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

        public override void AesirInspectorReset()
        {
            color1 = Color.white;
            color2 = Color.white;
            UnderwaterColor = Color.white;
            MyColor = Color.white;
            color3 = Color.white;
            DynamicPaletteName = "Clovers";
            DynamicPaletteColor = Color.white;
            WideColorPalette = Color.white;
            ColorArray = null;
            ColorPalettes = null;
        }
    }
}
