namespace Sirenix.Utilities
{
	/// <summary>
	/// <para>This attribute is used by classes deriving from GlobalConfig and specifies the menu item path for the preference window and the asset path for the generated config file.</para>
	/// <para>The scriptable object created will be located at the OdinEditorConfigs path unless other is specified.</para>
	/// <para>Classes implementing this attribute will be part of the Odin Preferences window.</para>
	/// </summary>
	/// <seealso cref="T:Sirenix.Utilities.SirenixGlobalConfigAttribute" />
	public class SirenixEditorConfigAttribute : GlobalConfigAttribute
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Utilities.SirenixEditorConfigAttribute" /> class.
		/// </summary>
		public SirenixEditorConfigAttribute()
			: base(SirenixAssetPaths.OdinEditorConfigsPath)
		{
		}
	}
}
