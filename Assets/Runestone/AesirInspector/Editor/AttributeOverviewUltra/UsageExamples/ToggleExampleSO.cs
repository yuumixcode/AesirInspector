using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Toggle 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ToggleExampleSO : AttributeExampleSO<ToggleExampleSO>
    {
        [Title("Parameter: toggleMemberName")]
        [Toggle("Enabled")]
        public MyToggleable Toggler = new MyToggleable();

        [Title("Usage On Types")]
        public ToggleableClass Toggleable = new ToggleableClass();

        public override void AesirInspectorReset()
        {
            Toggler = new MyToggleable();
            Toggleable = new ToggleableClass();
        }

        [Serializable]
        public class MyToggleable
        {
            public bool Enabled;

            public int MyValue;
        }

        [Serializable]
        [Toggle("Enabled")]
        public class ToggleableClass
        {
            public bool Enabled;

            public string Text;
        }
    }
}
