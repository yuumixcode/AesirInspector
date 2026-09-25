using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.Editor
{
    [AesirExample]
    public class EnableInExampleSO : AttributeExampleSO<EnableInExampleSO>
    {
        [Title("Parameter: PrefabKind (All)")]
        [EnableIn(PrefabKind.All)]
        public GameObject defaultEnabled;

        [Title("Parameter: PrefabKind")]
        [EnableIn(PrefabKind.InstanceInScene)]
        public string instanceInScene = "Instances of prefabs in scenes";

        [EnableIn(PrefabKind.InstanceInPrefab)]
        public string instanceInPrefab = "Instances of prefabs nested inside other prefabs";

        [EnableIn(PrefabKind.Regular)]
        public string regular = "Regular prefab assets";

        [EnableIn(PrefabKind.Variant)]
        public string variant = "Prefab variant assets";

        [EnableIn(PrefabKind.NonPrefabInstance)]
        public string nonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

        [EnableIn(PrefabKind.PrefabInstance)]
        public string prefabInstance =
            "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

        [EnableIn(PrefabKind.PrefabAsset)]
        public string prefabAsset = "Prefab assets and prefab variant assets";

        [EnableIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
        public string prefabInstanceAndNonPrefabInstance =
            "Prefab Instances, as well as non-prefab instances";

        public override void AesirInspectorReset()
        {
            defaultEnabled = null;
            instanceInScene = "Instances of prefabs in scenes";
            instanceInPrefab = "Instances of prefabs nested inside other prefabs";
            regular = "Regular prefab assets";
            variant = "Prefab variant assets";
            nonPrefabInstance = "Non-prefab component or gameobject instances in scenes";
            prefabInstance =
                "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";
            prefabAsset = "Prefab assets and prefab variant assets";
            prefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";
        }
    }
}
