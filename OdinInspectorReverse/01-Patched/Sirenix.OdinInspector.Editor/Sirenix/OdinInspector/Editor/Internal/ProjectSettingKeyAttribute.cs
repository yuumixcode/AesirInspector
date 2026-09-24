using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public class ProjectSettingKeyAttribute : Attribute
	{
		public readonly string Key;

		public readonly object DefaultValue;

		public ProjectSettingKeyAttribute(string key, object defaultValue)
		{
			Key = key;
			DefaultValue = defaultValue;
		}
	}
}
