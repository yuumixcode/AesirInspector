using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace Sirenix.OdinValidator.Editor
{
	public class MainValidationProfile : ValidationProfile, IGlobalConfigEvents
	{
		public static MainValidationProfile Instance => GlobalConfigUtility<Sirenix.OdinValidator.Editor.MainValidationProfile>.GetInstance(ValidationProfile.DefaultConfigFolderPath, "Main Profile");

		void IGlobalConfigEvents.OnConfigAutoCreated()
		{
			icon = SdfIconType.CircleFill;
		}

		void IGlobalConfigEvents.OnConfigInstanceFirstAccessed()
		{
		}
	}
}
