using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class HideIfGroupExampleSO : AttributeExampleSO<HideIfGroupExampleSO>
    {
        [Title("No Parameters")]
        public bool toggle = true;

        [HideIfGroup("toggle")]
        [BoxGroup("toggle/Hidden Box")]
        public int a;

        [BoxGroup("toggle/Hidden Box")]
        public int b;

        [Title("Parameter: Value")]
        public InfoMessageType messageType = InfoMessageType.Info;

        [HideIfGroup("toggle/messageType", Value = InfoMessageType.Info)]
        [BoxGroup("toggle/messageType/Border", ShowLabel = false)]
        public string fieldName;

        [BoxGroup("toggle/messageType/Border")]
        public Vector3 vector;

        [Title("Combining With BoxGroup")]
        [HideIfGroup("Box/toggle")]
        [BoxGroup("Box")]
        public Vector3 x;

        [HideIfGroup("Box/toggle")]
        [BoxGroup("Box")]
        public Vector3 y;

        [Title("Parameter: Condition")]
        [HideIfGroup("DemoGroup", Condition = "toggle")]
        public GameObject gameObject;

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
            gameObject = null;
        }
    }
}
