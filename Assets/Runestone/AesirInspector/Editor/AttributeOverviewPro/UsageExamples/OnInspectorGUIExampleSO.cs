using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnInspectorGUI 特性案例。
    /// </summary>
    [AesirExample]
    internal class OnInspectorGUIExampleSO : AttributeExampleSO<OnInspectorGUIExampleSO>
    {
        [Title("Parameter: Action (Before Field)")]
        [OnInspectorGUI("DrawLabelBefore", false)]
        public string FieldWithLabel = "Hello";

        [Title("Parameter: Action (After Field)")]
        [OnInspectorGUI("DrawButtonAfter")]
        public int FieldWithButton;

        [Title("Parameter: Action, Append")]
        [OnInspectorGUI("DrawPreview", true)]
        public Texture2D Texture;

        [Title("Parameter: Prepend, Append")]
        [OnInspectorGUI("DrawLabelBefore", "DrawButtonAfter")]
        public float FieldWithBoth;

        [Title("No Parameters (On Method)")]
        [OnInspectorGUI]
        void DrawCustomGUI()
        {
            var rect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(rect, Color.grey);
        }

        void DrawLabelBefore()
        {
            GUILayout.Label("This label is drawn before the field.", EditorStyles.boldLabel);
        }

        void DrawButtonAfter()
        {
            if (GUILayout.Button("Click Me!"))
            {
                Debug.Log("Button clicked!");
            }
        }

        void DrawPreview()
        {
            if (!(Texture == null))
            {
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(Texture);
                GUILayout.EndVertical();
            }
        }

        public override void AesirInspectorReset()
        {
            FieldWithLabel = "Hello";
            FieldWithButton = 0;
            Texture = null;
            FieldWithBoth = 0f;
        }
    }
}
