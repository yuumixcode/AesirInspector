using System;
using System.ComponentModel;

namespace Sirenix.Utilities
{
	public static class MemberFinderExtensions
	{
		/// <summary>
		/// <para>Find members of the given type, while providing good error messages based on the following search filters provided.</para>
		/// <para>See <see cref="T:Sirenix.Utilities.MemberFinder" /> for more information.</para>
		/// </summary>
		[Obsolete("MemberFinder is obsolete, due to performance issues and because its various uses have been replaced by the ValueResolver and ActionResolver utilities. Use cases that do not fit those utlities should use manual reflection that is hand-optimized for the best performance in the given case.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static MemberFinder FindMember(this Type type)
		{
			return MemberFinder.Start(type);
		}
	}
}
