using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// BoxGroup 特性的案例 SO：与其他特性组合（HorizontalGroup、Button 嵌套在 BoxGroup 中）。
    /// </summary>
    [AesirExample]
    public class BoxGroupCombiningExampleSO : AttributeExampleSO<BoxGroupCombiningExampleSO>
    {
        [Button(ButtonSizes.Large)]
        [HorizontalGroup("Buttons in Boxes")]
        [BoxGroup("Buttons in Boxes/One")]
        void Button1() { }

        [Button(ButtonSizes.Large)]
        [HorizontalGroup("Buttons in Boxes")]
        [BoxGroup("Buttons in Boxes/Two")]
        void Button2() { }

        [HorizontalGroup("Buttons in Boxes", Width = 60f)]
        [BoxGroup("Buttons in Boxes/Double")]
        [Button]
        void Accept() { }

        [Button]
        [BoxGroup("Buttons in Boxes/Double")]
        void Cancel() { }

        public override void AesirInspectorReset() { }
    }
}
