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
        OnCollectionChangedExampleWithAfterSO : AttributeExampleSO<OnCollectionChangedExampleWithAfterSO>
    {
        [Title("Parameter: After (Method Name)")]
        [OnCollectionChanged("AfterWithChangeInfo")]
        public List<string> methodNameExample = new List<string> { "str1", "str2", "str3" };

        [Title("Expression (@) (Member Reference)")]
        public string afterMessage = "A change occurred (member message)";

        [Title("Expression (@) (Member Reference)")]
        [OnCollectionChanged(After = "@Debug.Log(afterMessage, this)")]
        public List<string> memberReferenceExample = new List<string> { "A", "B", "C" };

        [Title("Expression (@)")]
        [OnCollectionChanged(After = "@Debug.Log(\"A change occurred\", this)")]
        public List<string> expressionExample = new List<string> { "X", "Y", "Z" };

#if UNITY_EDITOR
        void AfterWithChangeInfo(CollectionChangeInfo info, object value)
        {
            Debug.Log("Received callback AFTER CHANGE with the following info: " + info +
                      ", and the following collection instance: " + value);
        }
#endif

        public override void AesirInspectorReset()
        {
            methodNameExample = new List<string> { "str1", "str2", "str3" };
            afterMessage = "A change occurred (member message)";
            memberReferenceExample = new List<string> { "A", "B", "C" };
            expressionExample = new List<string> { "X", "Y", "Z" };
        }
    }
}
