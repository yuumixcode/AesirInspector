using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// AssetList 特性 CustomFilterMethod 参数的案例 SO。
    /// </summary>
    [AesirExample]
    public class AssetListExampleWithCustomFilterMethodSO : AttributeExampleSO<AssetListExampleWithCustomFilterMethodSO>
    {
        [Title("Parameter: CustomFilterMethod (GameObject obj)")]
        [AssetList(CustomFilterMethod = "$HasRigidbodyComponent")]
        [InlineButton("LogRigidbodyPrefabs", "Output Info")]
        public List<GameObject> rigidbodyPrefabs;

        bool HasRigidbodyComponent(GameObject obj) => obj.GetComponent<Rigidbody>() != null;

        void LogRigidbodyPrefabs()
        {
            if (rigidbodyPrefabs != null)
            {
                foreach (var prefab in rigidbodyPrefabs)
                {
                    if (prefab != null)
                    {
                        Debug.Log("Rigidbody Prefab: " + prefab.name);
                    }
                }
            }
        }

        public override void AesirInspectorReset()
        {
            rigidbodyPrefabs = null;
        }
    }
}
