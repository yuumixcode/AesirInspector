using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Button 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class ButtonExampleSO : AttributeExampleSO<ButtonExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        public bool Toggle;

        [FoldoutGroup("Member Reference ($)")]
        public string dynamicButtonName = "Click Me!";

        [FoldoutGroup("No Parameters")]
        [Button("Simple Button")]
        void SimpleButton() => Debug.Log("Button Clicked!");

        [FoldoutGroup("No Parameters")]
        [Button]
        void DefaultSizedButton() => Toggle = !Toggle;

        [FoldoutGroup("Member Reference ($)")]
        [Button("$dynamicButtonName")]
        void DynamicButton() => Debug.Log("Dynamic Button Clicked!");

        [FoldoutGroup("Expression (@)")]
        [Button("@\"Expression label: \" + DateTime.Now.ToString(\"HH:mm:ss\")")]
        void ExpressionLabel() => Toggle = !Toggle;

        [FoldoutGroup("Parameter: ButtonSize")]
        [Button(ButtonSizes.Small)]
        void SmallButton() { }

        [FoldoutGroup("Parameter: ButtonSize")]
        [Button(ButtonSizes.Medium)]
        void MediumButton() { }

        [FoldoutGroup("Parameter: ButtonSize")]
        [Button(ButtonSizes.Large)]
        void LargeButton() { }

        [FoldoutGroup("Parameter: ButtonSize")]
        [Button(ButtonSizes.Gigantic)]
        void GiganticButton() { }

        [FoldoutGroup("Parameter: ButtonSize")]
        [Button(50)]
        void CustomHeightButton() { }

        [FoldoutGroup("Parameter: Icon, IconAlignment")]
        [Button(SdfIconType.HeartFill, IconAlignment.LeftOfText)]
        void HeartButton() { }

        [FoldoutGroup("Parameter: Icon, IconAlignment")]
        [Button(SdfIconType.Dice2Fill, IconAlignment.RightOfText)]
        void IconButtonRightOfText() { }

        [FoldoutGroup("Parameter: Icon, IconAlignment")]
        [Button(SdfIconType.Dice3Fill, IconAlignment.LeftEdge)]
        void IconButtonLeftEdge() { }

        [FoldoutGroup("Parameter: Icon, IconAlignment")]
        [Button(SdfIconType.Dice4Fill, IconAlignment.RightEdge)]
        void IconButtonRightEdge() { }

        [FoldoutGroup("Parameter: Stretch, ButtonAlignment")]
        [Button(SdfIconType.Dice5Fill, IconAlignment.RightEdge, Stretch = false)]
        void DontStretch() { }

        [FoldoutGroup("Parameter: Stretch, ButtonAlignment")]
        [Button(SdfIconType.Dice5Fill, IconAlignment.RightEdge, Stretch = false, ButtonAlignment = 1f)]
        void DontStretchAndAlign() { }

        [FoldoutGroup("Combining With Other Attributes")]
        [GUIColor(0.4f, 0.8f, 1f)]
        [Button(ButtonSizes.Large)]
        [HorizontalGroup("Combining With Other Attributes/Split", 0.5f)]
        [DisableIf("Toggle")]
        void FanzyButton1() => Toggle = !Toggle;

        [FoldoutGroup("Combining With Other Attributes")]
        [GUIColor(0f, 1f, 0f)]
        [Button(ButtonSizes.Large)]
        [HideIf("Toggle")]
        [VerticalGroup("Combining With Other Attributes/Split/right")]
        void FanzyButton2() => Toggle = !Toggle;

        [FoldoutGroup("Combining With Other Attributes")]
        [ShowIf("Toggle")]
        [VerticalGroup("Combining With Other Attributes/Split/right")]
        [Button(ButtonSizes.Large)]
        [GUIColor(1f, 0.2f, 0f)]
        void FanzyButton3() => Toggle = !Toggle;

        [FoldoutGroup("Method Parameters")]
        [Button]
        void Default(float a, float b, GameObject c) { }

        [FoldoutGroup("Method Parameters")]
        [Button]
        void DefaultArray(float a, float b, float[] c) { }

        [FoldoutGroup("Method Parameters")]
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton)]
        int FoldoutButton(int a = 2, int b = 2) => a + b;

        [FoldoutGroup("Method Parameters")]
        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton)]
        void FoldoutButtonWithRef(int a, int b, ref int result) => result = a + b;

        [FoldoutGroup("Method Parameters")]
        [Button(ButtonStyle.Box)]
        void MethodWithParameters(string text, int count)
        {
            Debug.Log(string.Format("Text: {0}, Count: {1}", text, count));
        }

        [FoldoutGroup("Method Parameters")]
        [Button(ButtonStyle.Box)]
        void Full(float a, float b, out float c)
        {
            c = a + b;
        }

        [FoldoutGroup("Method Parameters")]
        [Button(ButtonStyle.CompactBox, Expanded = true)]
        void CompactExpanded(float a, float b, GameObject c) { }

        [FoldoutGroup("Method Parameters")]
        [Button(ButtonSizes.Medium, ButtonStyle.Box, Expanded = true)]
        void FullExpanded(float a, float b) { }

        public override void AesirInspectorReset()
        {
            dynamicButtonName = "Click Me!";
            Toggle = false;
        }
    }
}
