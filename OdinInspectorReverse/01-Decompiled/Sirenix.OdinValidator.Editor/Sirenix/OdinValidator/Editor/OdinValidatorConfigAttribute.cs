using Sirenix.Utilities;

namespace Sirenix.OdinValidator.Editor
{
	internal class OdinValidatorConfigAttribute : GlobalConfigAttribute
	{
		public OdinValidatorConfigAttribute()
			: base(SirenixAssetPaths.SirenixPluginPath + "Odin Validator/Editor/Config/")
		{
		}
	}
}
