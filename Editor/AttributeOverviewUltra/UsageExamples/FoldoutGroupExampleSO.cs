using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// FoldoutGroup 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class FoldoutGroupExampleSO : AttributeExampleSO<FoldoutGroupExampleSO>
    {
        [Title("No Parameters")]
        [FoldoutGroup("Group 1")]
        public int A;

        [FoldoutGroup("Group 1")]
        public int B;

        [FoldoutGroup("Group 1")]
        public int C;

        [Title("Parameter: Expanded")]
        [FoldoutGroup("Collapsed group", false)]
        public int D;

        [FoldoutGroup("Collapsed group")]
        public int E;

        [FoldoutGroup("Initially Expanded", true)]
        public int expandedInt1;

        [FoldoutGroup("Initially Expanded")]
        public int expandedInt2;

        [Title("Member Reference ($)")]
        [FoldoutGroup("$GroupTitle")]
        public int One;

        [FoldoutGroup("$GroupTitle")]
        public int Two;

        public string GroupTitle = "Dynamic group title";

        [Title("Combining With BoxGroup")]
        [FoldoutGroup("Nested")]
        [BoxGroup("Nested/Inside Box")]
        public int nestedInt;

        [Title("Combining With Button")]
        [FoldoutGroup("Buttons in Foldout")]
        [Button(ButtonSizes.Large)]
        void Button1() { }

        [FoldoutGroup("Buttons in Foldout")]
        [Button(ButtonSizes.Large)]
        void Button2() { }

        public override void AesirInspectorReset()
        {
            A = 0;
            B = 0;
            C = 0;
            D = 0;
            E = 0;
            expandedInt1 = 0;
            expandedInt2 = 0;
            One = 0;
            Two = 0;
            GroupTitle = "Dynamic group title";
            nestedInt = 0;
        }
    }
}
