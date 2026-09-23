using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class HideInExampleSO : AttributeExampleSO<HideInExampleSO>
    {
        [Title("Parameter: PrefabKind (All)")]
        [HideIn(PrefabKind.All)]
        public GameObject defaultPrefab;

        [Title("Parameter: PrefabKind")]
        [HideIn(PrefabKind.InstanceInScene)]
        public string instanceInScene = "Instances of prefabs in scenes";

        [HideIn(PrefabKind.InstanceInPrefab)]
        public string instanceInPrefab = "Instances of prefabs nested inside other prefabs";

        [HideIn(PrefabKind.Regular)]
        public string regular = "Regular prefab assets";

        [HideIn(PrefabKind.Variant)]
        public string variant = "Prefab variant assets";

        [HideIn(PrefabKind.NonPrefabInstance)]
        public string nonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

        [HideIn(PrefabKind.PrefabInstance)]
        public string prefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

        [HideIn(PrefabKind.PrefabAsset)]
        public string prefabAsset = "Prefab assets and prefab variant assets";

        [HideIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
        public string prefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";

        public override void AesirInspectorReset()
        {
            defaultPrefab = null;
            instanceInScene = "Instances of prefabs in scenes";
            instanceInPrefab = "Instances of prefabs nested inside other prefabs";
            regular = "Regular prefab assets";
            variant = "Prefab variant assets";
            nonPrefabInstance = "Non-prefab component or gameobject instances in scenes";
            prefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";
            prefabAsset = "Prefab assets and prefab variant assets";
            prefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";
        }
    }
}
