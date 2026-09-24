using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// DisallowModificationsIn 特性的案例 SO。
    /// </summary>
    [AesirExample]
    public class DisallowModificationsInExampleSO : AttributeExampleSO<DisallowModificationsInExampleSO>
    {
        [InfoBox("该示例本身不是 GameObject，因此特性在 Attribute Overview 窗口中不会体现；把它复制到脚本并挂到各类 Prefab 的 GameObject 上即可看到效果。",
            InfoMessageType.Warning)]
        [Title("Parameter: PrefabKind")]
        [DisallowModificationsIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
        public string PrefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";

        [DisallowModificationsIn(PrefabKind.InstanceInScene)]
        public string InstanceInScene = "Instances of prefabs in scenes";

        [DisallowModificationsIn(PrefabKind.InstanceInPrefab)]
        public string InstanceInPrefab = "Instances of prefabs nested inside other prefabs";

        [DisallowModificationsIn(PrefabKind.Variant)]
        public string Variant = "Prefab variant assets";

        [DisallowModificationsIn(PrefabKind.NonPrefabInstance)]
        public string NonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

        [DisallowModificationsIn(PrefabKind.PrefabInstance)]
        public string PrefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

        public override void AesirInspectorReset()
        {
            PrefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";
            InstanceInScene = "Instances of prefabs in scenes";
            InstanceInPrefab = "Instances of prefabs nested inside other prefabs";
            Variant = "Prefab variant assets";
            NonPrefabInstance = "Non-prefab component or gameobject instances in scenes";
            PrefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";
        }
    }
}
