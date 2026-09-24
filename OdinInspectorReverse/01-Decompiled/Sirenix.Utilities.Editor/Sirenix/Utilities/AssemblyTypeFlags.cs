using System;

namespace Sirenix.Utilities
{
	/// <summary>
	/// AssemblyTypeFlags is a bitmask used to filter types and assemblies related to Unity.
	/// </summary>
	/// <seealso cref="T:Sirenix.Utilities.AssemblyUtilities" />
	[Flags]
	[Obsolete("AssemblyTypeFlags have been made obsolete, because they cannot be determined accurately for all assemblies. Use AssemblyUtilities.GetAssemblyCategory(assembly) instead.", false)]
	public enum AssemblyTypeFlags
	{
		/// <summary>
		/// Excludes all types.
		/// </summary>
		None = 0,
		/// <summary>
		/// UserTypes includes all custom user scripts that are not located in an editor or plugin folder.
		/// </summary>
		UserTypes = 1,
		/// <summary>
		/// PluginTypes includes all types located in the plugins folder and are not located in an editor folder.
		/// </summary>
		PluginTypes = 2,
		/// <summary>
		/// UnityTypes includes all types depended on UnityEngine and from UnityEngine, except editor, plugin and user types.
		/// </summary>
		UnityTypes = 4,
		/// <summary>
		/// UserEditorTypes includes all custom user scripts that are located in an editor folder but not in a plugins folder.
		/// </summary>
		UserEditorTypes = 8,
		/// <summary>
		/// PluginEditorTypes includes all editor types located in the plugins folder.
		/// </summary>
		PluginEditorTypes = 0x10,
		/// <summary>
		/// UnityEditorTypes includes all editor types that are not user editor types nor plugin editor types.
		/// </summary>
		UnityEditorTypes = 0x20,
		/// <summary>
		/// OtherTypes includes all other types that are not depended on UnityEngine or UnityEditor.
		/// </summary>
		OtherTypes = 0x40,
		/// <summary>
		/// CustomTypes includes includes all types manually added to the Unity project.
		/// This includes UserTypes, UserEditorTypes, PluginTypes and PluginEditorTypes.
		/// </summary>
		CustomTypes = 0x1B,
		/// <summary>
		/// GameTypes includes all assemblies that are likely to be included in builds.
		/// This includes UserTypes, PluginTypes, UnityTypes and OtherTypes.
		/// </summary>
		GameTypes = 0x47,
		/// <summary>
		/// EditorTypes includes UserEditorTypes, PluginEditorTypes and UnityEditorTypes.
		/// </summary>
		EditorTypes = 0x38,
		/// <summary>
		/// All includes UserTypes, PluginTypes, UnityTypes, UserEditorTypes, PluginEditorTypes, UnityEditorTypes and OtherTypes.
		/// </summary>
		All = 0x7F
	}
}
