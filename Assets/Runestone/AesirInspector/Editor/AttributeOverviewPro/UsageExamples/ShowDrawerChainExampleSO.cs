using System;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowDrawerChain 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ShowDrawerChainExampleSO : AttributeExampleSO<ShowDrawerChainExampleSO>
    {
        [Title("No Parameters")]
        [ShowDrawerChain]
        public int intValue;

        [Title("No Parameters")]
        [ShowDrawerChain]
        public float floatValue;

        [Title("No Parameters")]
        [ShowDrawerChain]
        public bool boolValue;

        [Title("No Parameters")]
        [ShowDrawerChain]
        public string stringValue = "Unity Built-in";

        [Title("No Parameters")]
        [ShowDrawerChain]
        public Vector2 vector2Value;

        [Title("No Parameters")]
        [ShowDrawerChain]
        public LayerMask layerMask;

        [Title("No Parameters")]
        [ShowDrawerChain]
        public Color color = Color.white;

        [Title("Combining With Other Attributes")]
        [HorizontalGroup]
        [ShowInInspector]
        [ToggleLeft]
        public bool ToggleHideIf
        {
            get
            {
                GUIHelper.RequestRepaint();
                return EditorApplication.timeSinceStartup % 3.0 < 1.5;
            }
        }

        [Title("Combining With Other Attributes")]
        [ProgressBar(0.0, 1.5, 0.15f, 0.47f, 0.74f)]
        [HideLabel]
        [ShowInInspector]
        [HorizontalGroup]
        private double Animate => Math.Abs(EditorApplication.timeSinceStartup % 3.0 - 1.5);

        [Title("Combining With Other Attributes")]
        [InfoBox("Any drawer not used will be greyed out so that you can more easily debug the drawer chain. You can see this by toggling the above toggle field.\n\nIf you have any custom drawers they will show up with green names in the drawer chain.", InfoMessageType.Info)]
        [HideIf("ToggleHideIf", true)]
        [ShowDrawerChain]
        public GameObject SomeObject;

        [Title("Combining With Other Attributes")]
        [Range(0f, 10f)]
        [ShowDrawerChain]
        public float SomeRange;

        public override void AesirInspectorReset()
        {
            intValue = 0;
            floatValue = 0f;
            boolValue = false;
            stringValue = "Unity Built-in";
            vector2Value = Vector2.zero;
            layerMask = 0;
            color = Color.white;
            SomeObject = null;
            SomeRange = 0f;
        }
    }
}
