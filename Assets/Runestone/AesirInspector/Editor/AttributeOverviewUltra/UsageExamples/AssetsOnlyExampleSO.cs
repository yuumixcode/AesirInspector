using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// AssetsOnly 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class AssetsOnlyExampleSO : AttributeExampleSO<AssetsOnlyExampleSO>
    {
        [Title("No Parameters")]
        [AssetsOnly]
        public List<GameObject> OnlyPrefabs;

        [Title("No Parameters")]
        [AssetsOnly]
        public GameObject SomePrefab;

        [Title("No Parameters")]
        [AssetsOnly]
        public Material MaterialAsset;

        [Title("No Parameters")]
        [AssetsOnly]
        public MeshRenderer SomeMeshRendererOnPrefab;

        public override void AesirInspectorReset()
        {
            OnlyPrefabs = null;
            SomePrefab = null;
            MaterialAsset = null;
            SomeMeshRendererOnPrefab = null;
        }
    }
}
