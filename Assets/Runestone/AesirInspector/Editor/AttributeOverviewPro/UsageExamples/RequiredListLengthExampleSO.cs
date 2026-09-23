using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// RequiredListLength 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class RequiredListLengthExampleSO : AttributeExampleSO<RequiredListLengthExampleSO>
    {
        [Title("Parameter: FixedLength")]
        [RequiredListLength(10)]
        public int[] fixedLength;

        [Title("Parameter: MinLength")]
        [RequiredListLength(1, null)]
        public int[] minLength;

        [Title("Parameter: MaxLength, PrefabKind")]
        [RequiredListLength(null, 10, PrefabKind = PrefabKind.InstanceInScene)]
        public List<int> maxLength;

        [Title("Parameter: MinLength, MaxLength")]
        [RequiredListLength(3, 10)]
        public List<int> minAndMaxLength;

        [Title("Expression (@)")]
        public int SomeNumber;

        [Title("Expression (@)")]
        [RequiredListLength("@this.SomeNumber")]
        public List<GameObject> matchLengthOfOther;

        [Title("Expression (@)")]
        [RequiredListLength("@this.SomeNumber", null)]
        public int[] minLengthExpression;

        [Title("Expression (@)")]
        [RequiredListLength(null, "@this.SomeNumber")]
        public List<int> maxLengthExpression;

        public override void AesirInspectorReset()
        {
            fixedLength = null;
            minLength = null;
            maxLength = null;
            minAndMaxLength = null;
            SomeNumber = 0;
            matchLengthOfOther = null;
            minLengthExpression = null;
            maxLengthExpression = null;
        }
    }
}
