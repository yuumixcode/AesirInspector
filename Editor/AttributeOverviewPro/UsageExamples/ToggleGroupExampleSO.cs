using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ToggleGroup 特性的案例 SO：基础用法（勾选框作为组的开关）。
    /// </summary>
    [AesirExample]
    public class ToggleGroupExampleSO : AttributeExampleSO<ToggleGroupExampleSO>
    {
        [ToggleGroup("MyToggle")]
        public bool MyToggle;

        [ToggleGroup("MyToggle")]
        public float A;

        [HideLabel]
        [ToggleGroup("MyToggle")]
        [Multiline]
        public string B;

        public override void AesirInspectorReset()
        {
            MyToggle = false;
            A = 0f;
            B = null;
        }
    }
}
