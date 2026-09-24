using System.Reflection;

namespace Sirenix.Utilities
{
	/// <summary>
	/// This class encapsulates common <see cref="T:System.Reflection.BindingFlags" /> combinations.
	/// </summary>
	public static class Flags
	{
		/// <summary>
		/// Search criteria encompassing all public and non-public members, including base members.
		/// Note that you also need to specify either the Instance or Static flag.
		/// </summary>
		public const BindingFlags AnyVisibility = BindingFlags.Public | BindingFlags.NonPublic;

		/// <summary>
		/// Search criteria encompassing all public instance members, including base members.
		/// </summary>
		public const BindingFlags InstancePublic = BindingFlags.Instance | BindingFlags.Public;

		/// <summary>
		/// Search criteria encompassing all non-public instance members, including base members.
		/// </summary>
		public const BindingFlags InstancePrivate = BindingFlags.Instance | BindingFlags.NonPublic;

		/// <summary>
		/// Search criteria encompassing all public and non-public instance members, including base members.
		/// </summary>
		public const BindingFlags InstanceAnyVisibility = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		/// <summary>
		/// Search criteria encompassing all public static members, including base members.
		/// </summary>
		public const BindingFlags StaticPublic = BindingFlags.Static | BindingFlags.Public;

		/// <summary>
		/// Search criteria encompassing all non-public static members, including base members.
		/// </summary>
		public const BindingFlags StaticPrivate = BindingFlags.Static | BindingFlags.NonPublic;

		/// <summary>
		/// Search criteria encompassing all public and non-public static members, including base members.
		/// </summary>
		public const BindingFlags StaticAnyVisibility = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		/// <summary>
		/// Search criteria encompassing all public instance members, excluding base members.
		/// </summary>
		public const BindingFlags InstancePublicDeclaredOnly = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

		/// <summary>
		/// Search criteria encompassing all non-public instance members, excluding base members.
		/// </summary>
		public const BindingFlags InstancePrivateDeclaredOnly = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic;

		/// <summary>
		/// Search criteria encompassing all public and non-public instance members, excluding base members.
		/// </summary>
		public const BindingFlags InstanceAnyDeclaredOnly = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		/// <summary>
		/// Search criteria encompassing all public static members, excluding base members.
		/// </summary>
		public const BindingFlags StaticPublicDeclaredOnly = BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public;

		/// <summary>
		/// Search criteria encompassing all non-public static members, excluding base members.
		/// </summary>
		public const BindingFlags StaticPrivateDeclaredOnly = BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic;

		/// <summary>
		/// Search criteria encompassing all public and non-public static members, excluding base members.
		/// </summary>
		public const BindingFlags StaticAnyDeclaredOnly = BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		/// <summary>
		/// Search criteria encompassing all members, including base and static members.
		/// </summary>
		public const BindingFlags StaticInstanceAnyVisibility = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		/// <summary>
		/// Search criteria encompassing all members (public and non-public, instance and static), including base members.
		/// </summary>
		public const BindingFlags AllMembers = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
	}
}
