using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ChildGameObjectsOnly 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class ChildGameObjectOnlyExampleSO : AttributeExampleSO<ChildGameObjectOnlyExampleSO>
    {
        [Title("No Parameters")]
        [InfoBox("On a ScriptableObject the dropdown has no child hierarchy to search; the attribute is intended for components on scene GameObjects.", InfoMessageType.Info)]
        [ChildGameObjectsOnly]
        public Transform childOrSelfTransform;

        [Title("No Parameters")]
        [ChildGameObjectsOnly]
        public GameObject childGameObject;

        [Title("Parameter: IncludeSelf")]
        [ChildGameObjectsOnly(IncludeSelf = false)]
        public Light[] lights;

        public override void AesirInspectorReset()
        {
            childOrSelfTransform = null;
            childGameObject = null;
            lights = null;
        }
    }
}
