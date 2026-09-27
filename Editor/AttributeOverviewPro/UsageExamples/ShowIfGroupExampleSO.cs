using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowIfGroup 特性的案例 SO：基础用法（开关字段控制整个组的显示）与与 BoxGroup 组合。
    /// </summary>
    [AesirExample]
    public class ShowIfGroupExampleSO : AttributeExampleSO<ShowIfGroupExampleSO>
    {
        public bool toggle = true;

        [ShowIfGroup("toggle")]
        [BoxGroup("toggle/Shown Box")]
        public int a;

        [BoxGroup("toggle/Shown Box")]
        public int b;

        public InfoMessageType messageType = InfoMessageType.Info;

        [ShowIfGroup("toggle/messageType", Value = InfoMessageType.Info)]
        [BoxGroup("toggle/messageType/Border", ShowLabel = false)]
        public string fieldName;

        [BoxGroup("toggle/messageType/Border")]
        public Vector3 vector;

        [ShowIfGroup("Box/toggle")]
        [BoxGroup("Box")]
        public Vector3 x;

        [ShowIfGroup("Box/toggle")]
        [BoxGroup("Box")]
        public Vector3 y;

        public override void AesirInspectorReset()
        {
            toggle = true;
            a = 0;
            b = 0;
            messageType = InfoMessageType.Info;
            fieldName = string.Empty;
            vector = Vector3.zero;
            x = Vector3.zero;
            y = Vector3.zero;
        }
    }
}
