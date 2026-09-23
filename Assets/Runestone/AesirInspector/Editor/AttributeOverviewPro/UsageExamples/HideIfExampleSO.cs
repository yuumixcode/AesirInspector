using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class HideIfExampleSO : AttributeExampleSO<HideIfExampleSO>
    {
        [Title("Parameter: Condition (bool)")]
        public bool isToggled;

        [HideIf("isToggled")]
        public int hideIfToggled;

        [Title("Parameter: Condition (Enum), Value")]
        [EnumToggleButtons]
        public InfoMessageType someEnum;

        [HideIf("someEnum", InfoMessageType.Info)]
        public Vector2 hideIfInfo;

        [HideIf("someEnum", InfoMessageType.Error)]
        public Vector2 hideIfError;

        [HideIf("someEnum", InfoMessageType.Warning)]
        public Vector2 hideIfWarning;

        [Title("Parameter: Condition (Object Reference)")]
        public UnityEngine.Object someObject;

        [HideIf("someObject")]
        public Vector3 hideWhenIsNotNull;

        [Title("Parameter: Condition (Method)")]
        [HideIf("Method")]
        public int hideWithMethod;

        [Title("Expression (@)")]
        [HideIf("@this.isToggled && this.someObject != null || this.someEnum == InfoMessageType.Error")]
        public int hideWithExpression;

        bool Method() => (isToggled && someObject != null) || someEnum == InfoMessageType.Error;

        public override void AesirInspectorReset()
        {
            isToggled = false;
            someEnum = InfoMessageType.Info;
            someObject = null;
            hideIfToggled = 0;
            hideIfInfo = Vector2.zero;
            hideIfError = Vector2.zero;
            hideIfWarning = Vector2.zero;
            hideWhenIsNotNull = Vector3.zero;
            hideWithMethod = 0;
            hideWithExpression = 0;
        }
    }
}
