using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ReadOnly 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class ReadOnlyExampleSO : AttributeExampleSO<ReadOnlyExampleSO>
    {
        [Title("No Parameters")]
        [ReadOnly]
        public string MyString = "This is displayed as text";

        [Title("No Parameters")]
        [ReadOnly]
        public int MyInt = 9001;

        [Title("Usage with Collections")]
        [ReadOnly]
        public int[] MyIntList = new int[7] { 1, 2, 3, 4, 5, 6, 7 };

        [Title("Usage with Collections")]
        [ReadOnly]
        public List<int> readOnlyList = new List<int> { 1, 2, 3 };

        [Title("Usage with Properties")]
        [ShowInInspector]
        [ReadOnly]
        public int ReadOnlyProperty => 42;

        public override void AesirInspectorReset()
        {
            MyString = "This is displayed as text";
            MyInt = 9001;
            MyIntList = new int[7] { 1, 2, 3, 4, 5, 6, 7 };
            readOnlyList = new List<int> { 1, 2, 3 };
        }
    }
}
