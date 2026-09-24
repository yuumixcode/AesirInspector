using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Sirenix.OdinValidator.Editor
{
	public interface IValidationProfile
	{
		SdfIconType Icon { get; }

		SessionConfigDataType Type { get; }

		IList<ValidationItem> Include { get; }

		IList<ValidationItem> Exclude { get; }

		void SaveChanges();
	}
}
