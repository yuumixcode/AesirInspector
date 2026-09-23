using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// EnableIf 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class EnableIfExampleSO : AttributeExampleSO<EnableIfExampleSO>
    {
        [Title("Parameter: Condition (bool)")]
        public bool isToggled;

        [EnableIf("isToggled")]
        public int enabledWhenToggled;

        [Title("Parameter: Condition (Enum), Value")]
        [EnumToggleButtons]
        public InfoMessageType someEnum;

        [EnableIf("someEnum", InfoMessageType.Info)]
        public string enabledWhenInfo = "Only editable when someEnum is Info";

        [EnableIf("someEnum", InfoMessageType.Error)]
        public string enabledWhenError = "Only editable when someEnum is Error";

        [Title("Parameter: Condition (Object Reference)")]
        public UnityEngine.Object someObject;

        [EnableIf("someObject")]
        public Vector3 enabledWhenHasReference;

        [Title("Expression (@)")]
        [EnableIf("@this.isToggled && this.someObject != null || this.someEnum == InfoMessageType.Error")]
        public string enabledWithExpression = "Complex condition with expression";

        public override void AesirInspectorReset()
        {
            isToggled = false;
            someEnum = InfoMessageType.None;
            enabledWhenToggled = 0;
            enabledWhenInfo = "Only editable when someEnum is Info";
            enabledWhenError = "Only editable when someEnum is Error";
            someObject = null;
            enabledWhenHasReference = Vector3.zero;
            enabledWithExpression = "Complex condition with expression";
        }
    }
}
