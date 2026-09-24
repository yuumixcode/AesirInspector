using System;

namespace Sirenix.OdinValidator.Editor
{
	[Flags]
	public enum ConfigSourceType
	{
		None = 0,
		Default = 1,
		Project = 2,
		Local = 4,
		All = 7
	}
}
