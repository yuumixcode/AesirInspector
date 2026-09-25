using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// OnCollectionChanged 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class OnCollectionChangedExampleSO : AttributeExampleSO<OnCollectionChangedExampleSO>
    {
        [Title("Parameter: After")]
        [OnCollectionChanged("After")]
        public List<string> afterOnlyList = new List<string> { "str1", "str2", "str3" };

        [Title("Parameter: Before, After")]
        [InfoBox("Change the collection to get callbacks detailing the changes that are being made.")]
        [OnCollectionChanged("Before", "After")]
        public List<string> list = new List<string> { "str1", "str2", "str3" };

        [Title("Parameter: Before, After")]
        [OnCollectionChanged("Before", "After")]
        [OdinSerialize]
        public Dictionary<string, string> dictionary = new Dictionary<string, string>
        {
            { "key1", "str1" },
            { "key2", "str2" },
            { "key3", "str3" }
        };

        [Title("Parameter: Before, After")]
        [OnCollectionChanged("Before", "After")]
        [OdinSerialize]
        public HashSet<string> hashset = new HashSet<string> { "str1", "str2", "str3" };

        public override void AesirInspectorReset()
        {
            afterOnlyList = new List<string> { "str1", "str2", "str3" };
            list = new List<string> { "str1", "str2", "str3" };
            hashset = new HashSet<string> { "str1", "str2", "str3" };
            dictionary = new Dictionary<string, string>
            {
                { "key1", "str1" },
                { "key2", "str2" },
                { "key3", "str3" }
            };
        }

#if UNITY_EDITOR
        public void Before(CollectionChangeInfo info, object value)
        {
            Debug.Log("Received callback BEFORE CHANGE with the following info: " + info +
                      ", and the following collection instance: " + value);
        }

        public void After(CollectionChangeInfo info, object value)
        {
            Debug.Log("Received callback AFTER CHANGE with the following info: " + info +
                      ", and the following collection instance: " + value);
        }
#endif
    }
}
