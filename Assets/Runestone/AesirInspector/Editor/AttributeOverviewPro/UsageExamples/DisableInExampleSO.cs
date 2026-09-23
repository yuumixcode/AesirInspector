using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class DisableInExampleSO : AttributeExampleSO<DisableInExampleSO>
    {
        [Title("Parameter: PrefabKind (All)")]
        [DisableIn(PrefabKind.All)]
        public GameObject defaultDisabled;

        [Title("Parameter: PrefabKind")]
        [DisableIn(PrefabKind.InstanceInScene)]
        public string instanceInScene = "Instances of prefabs in scenes";

        [DisableIn(PrefabKind.InstanceInPrefab)]
        public string instanceInPrefab = "Instances of prefabs nested inside other prefabs";

        [DisableIn(PrefabKind.Regular)]
        public string regular = "Regular prefab assets";

        [DisableIn(PrefabKind.Variant)]
        public string variant = "Prefab variant assets";

        [DisableIn(PrefabKind.NonPrefabInstance)]
        public string nonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

        [DisableIn(PrefabKind.PrefabInstance)]
        public string prefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

        [DisableIn(PrefabKind.PrefabAsset)]
        public string prefabAsset = "Prefab assets and prefab variant assets";

        [DisableIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
        public string prefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";

        public override void AesirInspectorReset()
        {
            defaultDisabled = null;
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
