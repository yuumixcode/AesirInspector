using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// SceneObjectsOnly 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class SceneObjectsOnlyExampleSO : AttributeExampleSO<SceneObjectsOnlyExampleSO>
    {
        [Title("No Parameters")]
        [SceneObjectsOnly]
        public List<GameObject> onlySceneObjects;

        [Title("No Parameters")]
        [SceneObjectsOnly]
        public GameObject someSceneObject;

        [Title("No Parameters")]
        [SceneObjectsOnly]
        public MeshRenderer someMeshRenderer;

        public override void AesirInspectorReset()
        {
            onlySceneObjects = null;
            someSceneObject = null;
            someMeshRenderer = null;
        }
    }
}
