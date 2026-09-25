using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Unity Space 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class SpaceExampleSO : AttributeExampleSO<SpaceExampleSO>
    {
        [Title("No Parameters")]
        public int beforeDefaultSpace = 1;

        [Space]
        public int afterDefaultSpace = 2;

        [Title("Parameter: height")]
        public int beforeCustomSpace = 3;

        [Space(40)]
        public int afterCustomSpace = 4;

        public override void AesirInspectorReset()
        {
            beforeDefaultSpace = 1;
            afterDefaultSpace = 2;
            beforeCustomSpace = 3;
            afterCustomSpace = 4;
        }
    }
}
