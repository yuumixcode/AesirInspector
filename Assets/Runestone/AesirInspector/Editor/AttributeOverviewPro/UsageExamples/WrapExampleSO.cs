using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Wrap 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class WrapExampleSO : AttributeExampleSO<WrapExampleSO>
    {
        [Title("No Parameters")]
        [Wrap(0f, 30f)]
        public int wrappedInt;

        [Title("No Parameters")]
        [Wrap(0f, 30f)]
        public float wrappedFloat;

        [Title("No Parameters")]
        [Wrap(0f, 30f)]
        public Vector3 wrappedVector3;

        [Title("No Parameters")]
        [Wrap(0.0, 100.0)]
        public int IntWrapFrom0To100;

        [Title("No Parameters")]
        [Wrap(0.0, 100.0)]
        public float FloatWrapFrom0To100;

        [Title("No Parameters")]
        [Wrap(0.0, 100.0)]
        public Vector3 Vector3WrapFrom0To100;

        [Title("No Parameters")]
        [Wrap(0.0, 360.0)]
        public float angleWrap;

        [Title("No Parameters")]
        [Wrap(0.0, 6.2831854820251465)]
        public float radianWrap;

        public override void AesirInspectorReset()
        {
            wrappedInt = 0;
            wrappedFloat = 0f;
            wrappedVector3 = Vector3.zero;
            IntWrapFrom0To100 = 0;
            FloatWrapFrom0To100 = 0f;
            Vector3WrapFrom0To100 = Vector3.zero;
            angleWrap = 0f;
            radianWrap = 0f;
        }
    }
}
