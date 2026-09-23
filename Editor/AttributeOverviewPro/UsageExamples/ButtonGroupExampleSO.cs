using System;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ButtonGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ButtonGroupExampleSO : AttributeExampleSO<ButtonGroupExampleSO>
    {
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        [HideLabel]
        public struct IconButtonGroupExamples
        {
            [ButtonGroup("_DefaultGroup", 0f, ButtonHeight = 25)]
            [Button(SdfIconType.ArrowsMove, "")]
            private void ArrowsMove() { }

            [ButtonGroup("_DefaultGroup", 0f)]
            [Button(SdfIconType.Crop, "")]
            private void Crop() { }

            [ButtonGroup("_DefaultGroup", 0f)]
            [Button(SdfIconType.TextLeft, "")]
            private void TextLeft() { }

            [ButtonGroup("_DefaultGroup", 0f)]
            [Button(SdfIconType.TextRight, "")]
            private void TextRight() { }

            [ButtonGroup("_DefaultGroup", 0f)]
            [Button(SdfIconType.TextParagraph, "")]
            private void TextParagraph() { }

            [ButtonGroup("_DefaultGroup", 0f)]
            [Button(SdfIconType.Textarea, "")]
            private void Textarea() { }
        }

        [Title("No Parameters")]
        public IconButtonGroupExamples iconButtonGroupExamples;

        [Title("No Parameters")]
        [ButtonGroup]
        void ButtonA() => Debug.Log("Button A Clicked");

        [Title("No Parameters")]
        [ButtonGroup]
        void ButtonB() => Debug.Log("Button B Clicked");

        [Title("No Parameters")]
        [ButtonGroup]
        void ButtonC() => Debug.Log("Button C Clicked");

        [Title("No Parameters")]
        [ButtonGroup]
        void ButtonD() => Debug.Log("Button D Clicked");

        [Title("Parameter: Group")]
        [ButtonGroup("My Button Group", 0f)]
        [Button(ButtonSizes.Large)]
        void BigButtonInGroup() { }

        [Title("Parameter: Group")]
        [ButtonGroup("My Button Group", 0f)]
        [GUIColor(0f, 1f, 0f, 1f)]
        void GreenButtonInGroup() { }

        [Title("Parameter: Order")]
        [ButtonGroup("Ordered", Order = 20)]
        void LateButton() => Debug.Log("Late Button (Order=20)");

        [Title("Parameter: Order")]
        [ButtonGroup("Ordered", Order = 10)]
        void EarlyButton() => Debug.Log("Early Button (Order=10)");

        [Title("Parameter: ButtonHeight")]
        [ButtonGroup("Tall", ButtonHeight = 40)]
        void TallButton() => Debug.Log("Tall Button (ButtonHeight=40)");

        public override void AesirInspectorReset()
        {
            iconButtonGroupExamples = default;
        }
    }
}
