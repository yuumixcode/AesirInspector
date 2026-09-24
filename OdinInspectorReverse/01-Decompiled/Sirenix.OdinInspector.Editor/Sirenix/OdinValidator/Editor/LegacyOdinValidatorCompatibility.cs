using System.Collections.Generic;

namespace Sirenix.OdinValidator.Editor
{
	internal static class LegacyOdinValidatorCompatibility
	{
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
		{
			return new HashSet<T>(source);
		}
	}
}
