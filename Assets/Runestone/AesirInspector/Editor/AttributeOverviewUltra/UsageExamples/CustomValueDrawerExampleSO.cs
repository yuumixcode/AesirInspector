using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// CustomValueDrawer 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class CustomValueDrawerExampleSO : AttributeExampleSO<CustomValueDrawerExampleSO>
    {
        [Title("Parameter: Action (float value, GUIContent label) : Static")]
        [CustomValueDrawer("MyCustomDrawerStatic")]
        public float CustomDrawerStatic;

        [Title("Parameter: Action (float value, GUIContent label) : Instance")]
        public float From = 2f;

        [Title("Parameter: Action (float value, GUIContent label) : Instance")]
        public float To = 7f;

        [Title("Parameter: Action (float value, GUIContent label) : Instance")]
        [CustomValueDrawer("MyCustomDrawerInstance")]
        public float CustomDrawerInstance;

        [Title("Parameter: Action (float value, GUIContent label) : Instance")]
        [CustomValueDrawer("DrawSlider")]
        public float customSlider = 5f;

        [Title("Parameter: Action (float value, GUIContent label, Func<GUIContent, bool> callNextDrawer)")]
        [CustomValueDrawer("MyCustomDrawerAppendRange")]
        public float AppendRange;

        [Title("Parameter: Action (float value)")]
        [CustomValueDrawer("MyCustomDrawerArrayNoLabel")]
        public float[] CustomDrawerArrayNoLabel = { 3f, 5f, 6f };

        [Title("Parameter: Action (Color value, GUIContent label)")]
        [CustomValueDrawer("DrawColorBox")]
        public Color customColor = Color.red;

        static float MyCustomDrawerStatic(float value, GUIContent label) =>
            EditorGUILayout.Slider(label, value, 0f, 10f);

        float MyCustomDrawerInstance(float value, GUIContent label) =>
            EditorGUILayout.Slider(label, value, From, To);

        float DrawSlider(float value, GUIContent label) => EditorGUILayout.Slider(label, value, From, To);

        float MyCustomDrawerAppendRange(float value, GUIContent label, Func<GUIContent, bool> callNextDrawer)
        {
            SirenixEditorGUI.BeginBox();
            callNextDrawer(label);
            var result = EditorGUILayout.Slider(value, From, To);
            SirenixEditorGUI.EndBox();
            return result;
        }

        float MyCustomDrawerArrayNoLabel(float value) => EditorGUILayout.Slider(value, From, To);

        Color DrawColorBox(Color value, GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect();
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            var newColor = EditorGUI.ColorField(rect, value);
            return newColor;
        }

        public override void AesirInspectorReset()
        {
            CustomDrawerStatic = 0f;
            From = 2f;
            To = 7f;
            CustomDrawerInstance = 0f;
            customSlider = 5f;
            AppendRange = 0f;
            CustomDrawerArrayNoLabel = new[] { 3f, 5f, 6f };
            customColor = Color.red;
        }
    }
}
