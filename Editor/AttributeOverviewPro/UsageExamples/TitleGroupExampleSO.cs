using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// TitleGroup 特性的案例 SO：基础用法（分组标题、副标题、组内按钮、属性）。
    /// </summary>
    [AesirExample]
    public class TitleGroupExampleSO : AttributeExampleSO<TitleGroupExampleSO>
    {
        [TitleGroup("Ints")]
        public int SomeInt1;

        [TitleGroup("Ints", "Optional subtitle", TitleAlignments.Split)]
        public int SomeInt2;

        [TitleGroup("Ints/Buttons")]
        [Button]
        void IntButton() { }

        [TitleGroup("Vectors", "Optional subtitle", TitleAlignments.Centered)]
        public Vector2 SomeVector1;

        [TitleGroup("Vectors")]
        [ShowInInspector]
        public Vector2 SomeVector2 { get; set; }

        [TitleGroup("Vectors")]
        [Button]
        void VectorButton() { }

        public override void AesirInspectorReset()
        {
            SomeInt1 = 0;
            SomeInt2 = 0;
            SomeVector1 = default;
            SomeVector2 = default;
        }
    }
}
