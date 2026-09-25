using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// InlineButton 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class InlineButtonExampleSO : AttributeExampleSO<InlineButtonExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [InlineButton("A")]
        public int defaultLabelButton;

        [FoldoutGroup("Parameter: Label")]
        [InlineButton("OnButtonClick", "Click Me")]
        public int inlineButton;

        [FoldoutGroup("Multiple Buttons")]
        [InlineButton("A")]
        [InlineButton("B", "Custom Button Name")]
        public int multiButtons;

        [FoldoutGroup("Parameter: Icon")]
        [InlineButton("C", SdfIconType.Dice6Fill, "Random")]
        public int iconButton;

        [FoldoutGroup("Parameter: ShowIf")]
        public bool showButton = true;

        [FoldoutGroup("Parameter: ShowIf")]
        [InlineButton("C", "Conditional", ShowIf = "showButton")]
        public int conditionalButton;

        [FoldoutGroup("Parameter: ButtonColor, TextColor")]
        [InlineButton("C", "Colored", ButtonColor = "lightgreen", TextColor = "darkblue")]
        public int coloredButton;

        void OnButtonClick() => Debug.Log("Button Clicked!");
        void A() => Debug.Log("A Clicked!");
        void B() => Debug.Log("B Clicked!");
        void C() => Debug.Log("C Clicked!");

        public override void AesirInspectorReset()
        {
            defaultLabelButton = 0;
            inlineButton = 0;
            multiButtons = 0;
            iconButton = 0;
            showButton = true;
            conditionalButton = 0;
            coloredButton = 0;
        }
    }
}
