namespace Sirenix.Serialization.Utilities.Editor
{
	/// <summary>
	/// Defines how an assembly's import settings should be configured.
	/// </summary>
	public enum OdinAssemblyImportSettings
	{
		/// <summary>
		/// Include the assembly in the build, but not in the editor.
		/// </summary>
		IncludeInBuildOnly,
		/// <summary>
		/// Include the assembly in the editor, but not in the build.
		/// </summary>
		IncludeInEditorOnly,
		/// <summary>
		/// Include the assembly in both the build and in the editor.
		/// </summary>
		IncludeInAll,
		/// <summary>
		/// Exclude the assembly from both the build and from the editor.
		/// </summary>
		ExcludeFromAll
	}
}
