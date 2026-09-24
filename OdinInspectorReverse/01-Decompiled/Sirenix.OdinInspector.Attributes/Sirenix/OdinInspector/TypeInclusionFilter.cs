using System;

namespace Sirenix.OdinInspector
{
	/// <summary> Specifies the types to include based on certain criteria. </summary>
	[Flags]
	public enum TypeInclusionFilter
	{
		None = 0,
		/// <summary> Represents types that are not interfaces, abstracts, or generics. </summary>
		IncludeConcreteTypes = 1,
		IncludeGenerics = 2,
		IncludeAbstracts = 4,
		IncludeInterfaces = 8,
		IncludeAll = 0xF
	}
}
