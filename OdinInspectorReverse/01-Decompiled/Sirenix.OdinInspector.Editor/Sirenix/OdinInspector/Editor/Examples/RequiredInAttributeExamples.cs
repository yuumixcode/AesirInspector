namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(RequiredInAttribute), "Required members in the inspector when edited from regular prefab assets, prefab variants, nested prefabs, prefab instances or non prefab instances.")]
	[TypeInfoBox("Note that this drawn example does not represent a GameObject, so the attribute is not shown correctly in the Attribute Overview window. If you copy it to a script, for example via the Save Component Script button, the example will display correctly when you put it on GameObjects in various kinds of prefabs.")]
	internal class RequiredInAttributeExamples
	{
		[RequiredIn(PrefabKind.InstanceInScene, ErrorMessage = "Error messages can be customized. Odin expressions is supported.")]
		public string InstanceInScene = "Instances of prefabs in scenes";

		[RequiredIn(PrefabKind.InstanceInPrefab)]
		public string InstanceInPrefab = "Instances of prefabs nested inside other prefabs";

		[RequiredIn(PrefabKind.Regular)]
		public string Regular = "Regular prefab assets";

		[RequiredIn(PrefabKind.Variant)]
		public string Variant = "Prefab variant assets";

		[RequiredIn(PrefabKind.NonPrefabInstance)]
		public string NonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

		[RequiredIn(PrefabKind.PrefabInstance)]
		public string PrefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

		[RequiredIn(PrefabKind.PrefabAsset)]
		public string PrefabAsset = "Prefab assets and prefab variant assets";

		[RequiredIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
		public string PrefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";
	}
}
