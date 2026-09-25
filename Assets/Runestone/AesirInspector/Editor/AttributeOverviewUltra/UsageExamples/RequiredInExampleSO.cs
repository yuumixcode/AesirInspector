using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// RequiredIn 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class RequiredInExampleSO : AttributeExampleSO<RequiredInExampleSO>
    {
        [Title("Note")]
        [DisplayAsString(12)]
        [HideLabel]
        public string info =
            "RequiredIn is specific to prefabs. It only shows validation errors when the inspected object is a certain kind of prefab (for example a regular prefab asset or a prefab instance in a scene).";

        [Title("Parameter: PrefabKind")]
        [RequiredIn(PrefabKind.InstanceInScene,
            ErrorMessage = "Error messages can be customized. Odin expressions is supported.")]
        public string instanceInScene = "Instances of prefabs in scenes";

        [Title("Parameter: PrefabKind")]
        [RequiredIn(PrefabKind.InstanceInPrefab)]
        public string instanceInPrefab = "Instances of prefabs nested inside other prefabs";

        [Title("Parameter: PrefabKind")]
        [RequiredIn(PrefabKind.Regular)]
        public string regular = "Regular prefab assets";

        [Title("Parameter: PrefabKind")]
        [RequiredIn(PrefabKind.Variant)]
        public string variant = "Prefab variant assets";

        [Title("Parameter: PrefabKind")]
        [RequiredIn(PrefabKind.NonPrefabInstance)]
        public string nonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

        [Title("Parameter: PrefabKind")]
        [RequiredIn(PrefabKind.PrefabInstance)]
        public string prefabInstance =
            "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

        [Title("Parameter: PrefabKind")]
        [RequiredIn(PrefabKind.PrefabAsset)]
        public string prefabAsset = "Prefab assets and prefab variant assets";

        [Title("Parameter: PrefabKind")]
        [RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
        public string prefabInstanceAndNonPrefabInstance =
            "Prefab instances, as well as non-prefab instances";

        public override void AesirInspectorReset()
        {
            info =
                "RequiredIn is specific to prefabs. It only shows validation errors when the inspected object is a certain kind of prefab (for example a regular prefab asset or a prefab instance in a scene).";
            instanceInScene = "Instances of prefabs in scenes";
            instanceInPrefab = "Instances of prefabs nested inside other prefabs";
            regular = "Regular prefab assets";
            variant = "Prefab variant assets";
            nonPrefabInstance = "Non-prefab component or gameobject instances in scenes";
            prefabInstance =
                "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";
            prefabAsset = "Prefab assets and prefab variant assets";
            prefabInstanceAndNonPrefabInstance = "Prefab instances, as well as non-prefab instances";
        }
    }
}
