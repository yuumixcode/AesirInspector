using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BilingualButton 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class BilingualButtonExampleSO : AttributeExampleSO<BilingualButtonExampleSO>
    {
        [BilingualTitle("按钮点击状态", "Button Click State")]
        public bool toggled;

        [BilingualButton("基础按钮", "Basic Button")]
        void BasicButton() => toggled = !toggled;

        [BilingualButton("大号按钮", "Large Button", ButtonSizes.Large)]
        void LargeButton() => toggled = !toggled;

        [BilingualButton("带图标按钮", "Icon Button", icon: SdfIconType.HeartFill)]
        void IconButton() => toggled = !toggled;

        [BilingualButton("不拉伸按钮", "Not Stretched", stretch: false)]
        void NotStretchedButton() => toggled = !toggled;

        [BilingualButton("带参数按钮", "Button With Parameters")]
        void ParameterButton(string text, int count) =>
            Debug.Log(string.Format("Text: {0}, Count: {1}", text, count));

        public override void AesirInspectorReset() => toggled = false;
    }
}
