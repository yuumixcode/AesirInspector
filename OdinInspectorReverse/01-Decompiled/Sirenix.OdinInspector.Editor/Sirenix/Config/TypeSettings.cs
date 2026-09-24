using Sirenix.OdinInspector;
using UnityEngine;

namespace Sirenix.Config
{
	public class TypeSettings
	{
		public string Name;

		public string Category;

		public SdfIconType Icon;

		public Color? LightIconColor;

		public Color? DarkIconColor;

		public bool IsDefault()
		{
			if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(Category) && Icon == SdfIconType.None && (!LightIconColor.HasValue || LightIconColor.Value.a == 0f))
			{
				if (DarkIconColor.HasValue)
				{
					return DarkIconColor.Value.a == 0f;
				}
				return true;
			}
			return false;
		}
	}
}
