using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ColorPalette 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class
        ColorPaletteExampleWithPaletteNameSO : AttributeExampleSO<ColorPaletteExampleWithPaletteNameSO>
    {
        [Title("Literal String Example")]
        [ColorPalette("Sepia")]
        public Color literalStringExample = Color.white;

        [Title("Field Name Example")]
        [ColorPalette("$SepiaPaletteName")]
        public Color fieldNameExample = Color.white;

        [Title("Attribute Expression Example")]
        [ColorPalette("@UseTropical ? TropicalPaletteName : SepiaPaletteName")]
        public Color attributeExpressionExample = Color.white;

        [Title("Property Name Example")]
        [ColorPalette("$PaletteNameProperty")]
        public Color propertyNameExample = Color.white;

        [Title("Method Name Example")]
        [ColorPalette("$GetPaletteName")]
        public Color methodNameExample = Color.white;

        public string SepiaPaletteName = "Sepia";
        public string TropicalPaletteName = "Tropical";
        public bool UseTropical;

        public string PaletteNameProperty => UseTropical ? TropicalPaletteName : SepiaPaletteName;

        public override void AesirInspectorReset()
        {
            SepiaPaletteName = "Sepia";
            TropicalPaletteName = "Tropical";
            UseTropical = false;
            literalStringExample = Color.white;
            fieldNameExample = Color.white;
            attributeExpressionExample = Color.white;
            propertyNameExample = Color.white;
            methodNameExample = Color.white;
        }

        string GetPaletteName() => UseTropical ? TropicalPaletteName : SepiaPaletteName;
    }
}
