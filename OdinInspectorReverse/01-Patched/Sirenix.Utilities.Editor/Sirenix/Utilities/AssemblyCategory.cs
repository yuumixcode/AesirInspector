using System;

namespace Sirenix.Utilities
{
	[Flags]
	public enum AssemblyCategory
	{
		None = 0,
		Scripts = 1,
		ImportedAssemblies = 2,
		UnityEngine = 4,
		DotNetRuntime = 8,
		DynamicAssemblies = 0x10,
		Unknown = 0x20,
		ProjectSpecific = 3,
		All = 0x3F
	}
}
