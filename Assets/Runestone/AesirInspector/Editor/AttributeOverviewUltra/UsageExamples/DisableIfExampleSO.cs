using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisableIf 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class DisableIfExampleSO : AttributeExampleSO<DisableIfExampleSO>
    {
        [Title("Parameter: Condition (bool)")]
        public bool isToggled;

        [DisableIf("isToggled")]
        public int disabledWhenToggled;

        [Title("Parameter: Condition (Enum), Value")]
        [EnumToggleButtons]
        public InfoMessageType someEnum;

        [DisableIf("someEnum", InfoMessageType.Info)]
        public string disabledWhenInfo = "Disabled when someEnum is Info";

        [DisableIf("someEnum", InfoMessageType.Error)]
        public string disabledWhenError = "Disabled when someEnum is Error";

        [Title("Parameter: Condition (Object Reference)")]
        public Object someObject;

        [DisableIf("someObject")]
        public Vector3 disabledWhenNotNull;

        [Title("Expression (@)")]
        [DisableIf("@this.isToggled && this.someObject != null || this.someEnum == InfoMessageType.Error")]
        public string disabledWithExpression = "Complex condition with expression";

        public override void AesirInspectorReset()
        {
            isToggled = false;
            someEnum = InfoMessageType.None;
            disabledWhenToggled = 0;
            disabledWhenInfo = "Disabled when someEnum is Info";
            disabledWhenError = "Disabled when someEnum is Error";
            someObject = null;
            disabledWhenNotNull = Vector3.zero;
            disabledWithExpression = "Complex condition with expression";
        }
    }
}
