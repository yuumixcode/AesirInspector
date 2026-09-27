using System;
using System.Runtime.InteropServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ButtonGroup 特性的案例 SO：默认组（"_DefaultGroup"）。
    /// </summary>
    [AesirExample]
    public class ButtonGroupExampleSO : AttributeExampleSO<ButtonGroupExampleSO>
    {
        public IconButtonGroupExamples iconButtonGroupExamples;

        [ButtonGroup]
        void ButtonA() => Debug.Log("Button A Clicked");

        [ButtonGroup]
        void ButtonB() => Debug.Log("Button B Clicked");

        [ButtonGroup]
        void ButtonC() => Debug.Log("Button C Clicked");

        [ButtonGroup]
        void ButtonD() => Debug.Log("Button D Clicked");

        public override void AesirInspectorReset()
        {
            iconButtonGroupExamples = default;
        }

        [Serializable]
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        [HideLabel]
        public struct IconButtonGroupExamples
        {
            [ButtonGroup(ButtonHeight = 25)]
            [Button(SdfIconType.ArrowsMove, "")]
            void ArrowsMove() { }

            [ButtonGroup()]
            [Button(SdfIconType.Crop, "")]
            void Crop() { }

            [ButtonGroup()]
            [Button(SdfIconType.TextLeft, "")]
            void TextLeft() { }

            [ButtonGroup()]
            [Button(SdfIconType.TextRight, "")]
            void TextRight() { }

            [ButtonGroup()]
            [Button(SdfIconType.TextParagraph, "")]
            void TextParagraph() { }

            [ButtonGroup()]
            [Button(SdfIconType.Textarea, "")]
            void Textarea() { }
        }
    }
}
