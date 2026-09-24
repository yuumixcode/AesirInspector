namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(DisableInAttribute), "Disable members in the inspector when edited from regular prefab assets, prefab variants, nested prefabs, prefab instances or non prefab instances.")]
	[TypeInfoBox("Note that this drawn example does not represent a GameObject, so the attribute is not shown correctly in the Attribute Overview window. If you copy it to a script, for example via the Save Component Script button, the example will display correctly when you put it on GameObjects in various kinds of prefabs.")]
	internal class DisableInAttributeExamples
	{
		[DisableIn(PrefabKind.InstanceInScene)]
		public string InstanceInScene = "Instances of prefabs in scenes";

		[DisableIn(PrefabKind.InstanceInPrefab)]
		public string InstanceInPrefab = "Instances of prefabs nested inside other prefabs";

		[DisableIn(PrefabKind.Regular)]
		public string Regular = "Regular prefab assets";

		[DisableIn(PrefabKind.Variant)]
		public string Variant = "Prefab variant assets";

		[DisableIn(PrefabKind.NonPrefabInstance)]
		public string NonPrefabInstance = "Non-prefab component or gameobject instances in scenes";

		[DisableIn(PrefabKind.PrefabInstance)]
		public string PrefabInstance = "Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs";

		[DisableIn(PrefabKind.PrefabAsset)]
		public string PrefabAsset = "Prefab assets and prefab variant assets";

		[DisableIn(PrefabKind.PrefabInstanceAndNonPrefabInstance)]
		public string PrefabInstanceAndNonPrefabInstance = "Prefab Instances, as well as non-prefab instances";
	}
}
