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
    public class GUIColorExampleWithColorSO : AttributeExampleSO<GUIColorExampleWithColorSO>
    {
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

        [Title("Member Reference ($) : Field")]
        public Color color = Color.green;

        [Title("Member Reference ($) : Field")]
        [GUIColor("$color")]
        public string fieldNameExample;

        [Title("Member Reference ($) : Property")]
        [GUIColor("$ColorProperty")]
        public string propertyNameExample;

        [Title("Member Reference ($) : Method")]
        [GUIColor("$GetColor")]
        public string methodNameExample;

        [Title("Member Reference ($) : Method")]
        [GUIColor("$GetDynamicColor")]
        public int dynamicColorExample;

        [Title("Expression (@)")]
        public bool useRed;

        [Title("Expression (@)")]
        [GUIColor("@useRed ? UnityEngine.Color.red : UnityEngine.Color.green")]
        public string attributeExpressionExample;

        public Color ColorProperty => color;

        Color GetColor() => useRed ? Color.red : Color.green;

        static Color GetDynamicColor()
        {
            GUIHelper.RequestRepaint();
            return Color.HSVToRGB(Mathf.Cos((float)EditorApplication.timeSinceStartup) * 0.225f + 0.325f, 1, 1);
        }

        public override void AesirInspectorReset()
        {
            Hex1 = 0;
            Hex2 = 0;
            Rgb = 0;
            Rgba = 0;
            NamedColors = 0;
            color = Color.green;
            fieldNameExample = string.Empty;
            propertyNameExample = string.Empty;
            methodNameExample = string.Empty;
            dynamicColorExample = 0;
            useRed = false;
            attributeExpressionExample = string.Empty;
        }
    }
}
