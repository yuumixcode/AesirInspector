using System;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// The prefab kind returned by <see cref="!:Sirenix.OdinInspector.Editor.OdinPrefabUtility.GetPrefabKind" /> 
	/// </summary>
	[Flags]
	public enum PrefabKind
	{
		/// <summary>
		/// None. 
		/// </summary>
		None = 0,
		/// <summary>
		/// Instances of prefabs in scenes.
		/// </summary>
		InstanceInScene = 1,
		/// <summary>
		/// Instances of prefabs nested inside other prefabs.
		/// </summary>
		InstanceInPrefab = 2,
		/// <summary>
		/// Regular prefab assets.
		/// </summary>
		Regular = 4,
		/// <summary>
		/// Prefab variant assets.
		/// </summary>
		Variant = 8,
		/// <summary>
		/// Non-prefab component or gameobject instances in scenes.
		/// </summary>
		NonPrefabInstance = 0x10,
		/// <summary>
		/// Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs.
		/// </summary>
		PrefabInstance = 3,
		/// <summary>
		/// Prefab assets and prefab variant assets.
		/// </summary>
		PrefabAsset = 0xC,
		/// <summary>
		/// Prefab Instances, as well as non-prefab instances.
		/// </summary>
		PrefabInstanceAndNonPrefabInstance = 0x13,
		/// <summary>
		/// All kinds
		/// </summary>
		All = 0x1F
	}
}
