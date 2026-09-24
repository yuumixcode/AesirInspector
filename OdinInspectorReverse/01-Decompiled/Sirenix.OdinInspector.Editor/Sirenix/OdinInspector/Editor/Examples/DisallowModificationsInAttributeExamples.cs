namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(DisallowModificationsInAttribute), "DisallowModificationsIn disables / grays out members, preventing modifications from being made and enables validation, providing error messages in case a modification was made prior to introducing the attribute. Modifications can be prevented for prefab variants and prefab instances in scenes and prefabs.")]
	[TypeInfoBox("Note that this drawn example does not represent a GameObject, so the attribute is not shown correctly in the Attribute Overview window. If you copy it to a script, for example via the Save Component Script button, the example will display correctly when you put it on GameObjects in various kinds of prefabs.")]
	internal class DisallowModificationsInAttributeExamples
	{
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
	}
}
