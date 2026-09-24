using System;

namespace Sirenix.OdinInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class TypeDrawerSettingsAttribute : Attribute
	{
		/// <summary>
		/// Specifies whether a base type should be used instead of all types.
		/// </summary>
		public Type BaseType;

		/// <summary>
		/// Filters the result.
		/// </summary>
		public TypeInclusionFilter Filter = TypeInclusionFilter.IncludeAll;
	}
}
