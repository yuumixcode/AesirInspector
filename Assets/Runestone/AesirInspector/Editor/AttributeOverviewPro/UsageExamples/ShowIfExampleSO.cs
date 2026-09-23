using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class ShowIfExampleSO : AttributeExampleSO<ShowIfExampleSO>
    {
        [Title("Parameter: Condition (bool)")]
        public bool showFields;

        [ShowIf("showFields")]
        public int visibleWhenToggled;

        [Title("Parameter: Condition (Enum), Value")]
        [EnumToggleButtons]
        public InfoMessageType messageType;

        [ShowIf("messageType", InfoMessageType.Info, true)]
        public Vector3 visibleWhenInfo;

        [ShowIf("messageType", InfoMessageType.Warning, true)]
        public Vector2 visibleWhenWarning;

        [ShowIf("messageType", InfoMessageType.Error, true)]
        public Vector3 visibleWhenError;

        [Title("Parameter: Condition (Object Reference)")]
        public UnityEngine.Object someObject;

        [ShowIf("someObject", true)]
        public Vector3 visibleWhenObjectAssigned;

        [Title("Expression (@)")]
        [ShowIf("@this.showFields && this.messageType == InfoMessageType.Error")]
        public int visibleWithExpression;

        public override void AesirInspectorReset()
        {
            showFields = false;
            messageType = InfoMessageType.Info;
            visibleWhenToggled = 0;
            visibleWhenInfo = Vector3.zero;
            visibleWhenWarning = Vector2.zero;
            visibleWhenError = Vector3.zero;
            someObject = null;
            visibleWhenObjectAssigned = Vector3.zero;
            visibleWithExpression = 0;
        }
    }
}
