using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// GUIColor 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class GUIColorExampleSO : AttributeExampleSO<GUIColorExampleSO>
    {
        [Title("Parameter: r, g, b, a")]
        [GUIColor(0.3f, 0.8f, 0.8f)]
        public int ColoredInt1;

        [Title("Parameter: r, g, b, a")]
        [GUIColor(0.3f, 0.8f, 0.8f)]
        public int ColoredInt2;

        [Title("Parameter: r, g, b, a")]
        [GUIColor(1f, 0.8f, 0.4f)]
        public int rgbaExample;

        [Title("Parameter: getColor (Hex, RGB(), RGBA(), Named Color)")]
        [GUIColor("#FF0000")]
        public int Hex1;

        [Title("Parameter: getColor (Hex, RGB(), RGBA(), Named Color)")]
        [GUIColor("#FF000077")]
        public int Hex2;

        [Title("Parameter: getColor (Hex, RGB(), RGBA(), Named Color)")]
        [GUIColor("RGB(0, 1, 0)")]
        public int Rgb;

        [Title("Parameter: getColor (Hex, RGB(), RGBA(), Named Color)")]
        [GUIColor("RGBA(0, 1, 0, 0.5)")]
        public int Rgba;

        [Title("Parameter: getColor (Hex, RGB(), RGBA(), Named Color)")]
        [GUIColor("orange")]
        public int NamedColors;

        [Title("Combining With ButtonGroup")]
        [ButtonGroup]
        [GUIColor(0f, 1f, 0f)]
        void Apply() { }

        [Title("Combining With ButtonGroup")]
        [ButtonGroup]
        [GUIColor(1f, 0.6f, 0.4f)]
        void Cancel() { }

        [Title("Member Reference")]
        [GUIColor("GetButtonColor")]
        [Button("I Am Fabulous", ButtonSizes.Gigantic)]
        [InfoBox("You can also reference a color member to dynamically change the color of a property.")]
        static void IAmFabulous() { }

        [Title("Member Reference")]
        static Color GetButtonColor()
        {
            GUIHelper.RequestRepaint();
            return Color.HSVToRGB(Mathf.Cos((float)EditorApplication.timeSinceStartup + 1f) * 0.225f + 0.325f,
                1f, 1f);
        }

        [Title("Expression (@)")]
        [Button(ButtonSizes.Large)]
        [GUIColor(
            "@Color.Lerp(Color.red, Color.green, Mathf.Abs(Mathf.Sin((float)EditorApplication.timeSinceStartup)))")]
        static void Expressive() { }

        public override void AesirInspectorReset()
        {
            ColoredInt1 = 0;
            ColoredInt2 = 0;
            rgbaExample = 0;
            Hex1 = 0;
            Hex2 = 0;
            Rgb = 0;
            Rgba = 0;
            NamedColors = 0;
        }
    }
}
