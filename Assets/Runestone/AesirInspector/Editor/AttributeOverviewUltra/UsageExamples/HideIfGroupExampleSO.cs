using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// HideIfGroup 特性的案例 SO：基础用法（开关字段控制整个组的隐藏）与与 BoxGroup 组合。
    /// </summary>
    [AesirExample]
    public class HideIfGroupExampleSO : AttributeExampleSO<HideIfGroupExampleSO>
    {
        public bool toggle = true;

        [HideIfGroup("toggle")]
        [BoxGroup("toggle/Hidden Box")]
        public int a;

        [BoxGroup("toggle/Hidden Box")]
        public int b;

        public InfoMessageType messageType = InfoMessageType.Info;

        [HideIfGroup("toggle/messageType", Value = InfoMessageType.Info)]
        [BoxGroup("toggle/messageType/Border", ShowLabel = false)]
        public string fieldName;

        [BoxGroup("toggle/messageType/Border")]
        public Vector3 vector;

        [HideIfGroup("Box/toggle")]
        [BoxGroup("Box")]
        public Vector3 x;

        [HideIfGroup("Box/toggle")]
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
