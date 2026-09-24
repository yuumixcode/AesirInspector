using System;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// InspectorDefaultEditors is a bitmask used to tell <see cref="T:Sirenix.OdinInspector.Editor.InspectorConfig" /> which types should have an Odin Editor generated.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.InspectorConfig" />
	[Flags]
	public enum InspectorDefaultEditors
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
		/// OtherTypes include all other types that are not depended on UnityEngine or UnityEditor.
		/// </summary>
		OtherTypes = 8
	}
}
