using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnCollectionChanged 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class
        OnCollectionChangedExampleWithBeforeSO : AttributeExampleSO<OnCollectionChangedExampleWithBeforeSO>
    {
        [Title("Parameter: Before (Method Name)")]
        [OnCollectionChanged(Before = "BeforeWithChangeInfo")]
        public List<string> methodNameExample = new List<string> { "str1", "str2", "str3" };

        [Title("Expression (@) (Member Reference)")]
        public string beforeMessage = "A change is about to occur (member message)";

        [Title("Expression (@) (Member Reference)")]
        [OnCollectionChanged(Before = "@Debug.Log(beforeMessage, this)")]
        public List<string> memberReferenceExample = new List<string> { "A", "B", "C" };

        [Title("Expression (@)")]
        [OnCollectionChanged(Before = "@Debug.Log(\"A change is about to occur\", this)")]
        public List<string> expressionExample = new List<string> { "X", "Y", "Z" };

#if UNITY_EDITOR
        void BeforeWithChangeInfo(CollectionChangeInfo info, object value)
        {
            Debug.Log("Received callback BEFORE CHANGE with the following info: " + info +
                      ", and the following collection instance: " + value);
        }
#endif

        public override void AesirInspectorReset()
        {
            methodNameExample = new List<string> { "str1", "str2", "str3" };
            beforeMessage = "A change is about to occur (member message)";
            memberReferenceExample = new List<string> { "A", "B", "C" };
            expressionExample = new List<string> { "X", "Y", "Z" };
        }
    }
}
