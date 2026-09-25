using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// ShowPropertyResolver 特性的案例 SO。
    /// </summary>
    [AesirExample]
    [ShowOdinSerializedPropertiesInInspector]
    public class ShowPropertyResolverExampleSO : OdinAttributeExampleSO<ShowPropertyResolverExampleSO>
    {
        [Title("No Parameters")]
        [OdinSerialize]
        [ShowPropertyResolver]
        public Dictionary<int, Vector3> MyDictionary = new Dictionary<int, Vector3>();

        [Title("No Parameters")]
        [ShowPropertyResolver]
        public List<int> myList = new List<int>();

        [Title("No Parameters")]
        [ShowPropertyResolver]
        public string myString = "Hello";

        public override void AesirInspectorReset()
        {
            MyDictionary = new Dictionary<int, Vector3>();
            myList = new List<int>();
            myString = "Hello";
        }
    }
}
