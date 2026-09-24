namespace Sirenix.OdinInspector.Editor.Examples
{
	[TypeInfoBox("Note that this drawn example does not represent a GameObject, so the attribute is not shown correctly in the Attribute Overview window. If you copy it to a script, for example via the Save Component Script button, the example will display correctly when you put it on GameObjects in various kinds of prefabs.")]
	[AttributeExample(typeof(EnableInAttribute), "Enable members in the inspector when edited from regular prefab assets, prefab variants, nested prefabs, prefab instances or non prefab instances.")]
	internal class EnableInAttributeExamples
	{
		[EnableIn(PrefabKind.InstanceInScene)]
		public string InstanceInScene = "Instances of prefabs in scenes";

		[EnableIn(PrefabKind.InstanceInPrefab)]
		public string InstanceInPrefab = "Instances of prefabs nested inside other prefabs";

		[EnableIn(PrefabKind.Regular)]
		public string Regular = "Regular prefab assets";

		[EnableIn(PrefabKind.Variant)]
		public string Variant = "Prefab variant assets";

		[EnableIn(PrefabKind.NonPrefabInstance)]
		public string NonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

		[EnableIn(PrefabKind.PrefabInstance)]
		public string PrefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

		[EnableIn(PrefabKind.PrefabAsset)]
		public string PrefabAsset = "Prefab assets and prefab variant assets";

		[EnableIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
		public string PrefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";
	}
}
