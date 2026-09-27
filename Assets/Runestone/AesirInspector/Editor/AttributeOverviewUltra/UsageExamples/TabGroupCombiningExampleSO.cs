using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TabGroup 特性的案例 SO：与 TitleGroup、HorizontalGroup、ResponsiveButtonGroup、嵌套 TabGroup 组合。
    /// </summary>
    [AesirExample]
    public class TabGroupCombiningExampleSO : AttributeExampleSO<TabGroupCombiningExampleSO>
    {
        [TitleGroup("Tabs")]
        [HorizontalGroup("Tabs/Split", 0.5f)]
        [TabGroup("Tabs/Split/Parameters", "A")]
        public string NameA;

        [TitleGroup("Tabs")]
        [HorizontalGroup("Tabs/Split", 0.5f)]
        [TabGroup("Tabs/Split/Parameters", "A")]
        public string NameB;

        [TitleGroup("Tabs")]
        [HorizontalGroup("Tabs/Split", 0.5f)]
        [TabGroup("Tabs/Split/Parameters", "A")]
        public string NameC;

        [TabGroup("Tabs/Split/Parameters", "B")]
        public int ValueA;

        [TabGroup("Tabs/Split/Parameters", "B")]
        public int ValueB;

        [TabGroup("Tabs/Split/Parameters", "B")]
        public int ValueC;

        [TabGroup("Tabs/Split/Buttons", "Responsive")]
        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void Hello() { }

        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void World() { }

        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void And() { }

        [ResponsiveButtonGroup("Tabs/Split/Buttons/Responsive/ResponsiveButtons")]
        public void Such() { }

        [TabGroup("Tabs/Split/Buttons", "More Tabs")]
        [TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "A")]
        [Button]
        public void SubButtonA() { }

        [TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "A")]
        [Button]
        public void SubButtonB() { }

        [TabGroup("Tabs/Split/Buttons/More Tabs/SubTabGroup", "B")]
        [Button(ButtonSizes.Gigantic)]
        public void SubButtonC() { }

        public override void AesirInspectorReset()
        {
            NameA = null;
            NameB = null;
            NameC = null;
            ValueA = 0;
            ValueB = 0;
            ValueC = 0;
        }
    }
}
