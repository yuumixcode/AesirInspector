using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DrawWithUnity 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class DrawWithUnityExampleSO : AttributeExampleSO<DrawWithUnityExampleSO>
    {
        [InfoBox("If you ever experience trouble with one of Odin's attributes, there is a good chance that DrawWithUnity will come in handy; it will make Odin draw the value as Unity normally would.", InfoMessageType.Info)]
        public GameObject ObjectDrawnWithOdin;

        [DrawWithUnity]
        public GameObject ObjectDrawnWithUnity;

        public override void AesirInspectorReset()
        {
            ObjectDrawnWithOdin = null;
            ObjectDrawnWithUnity = null;
        }
    }
}
