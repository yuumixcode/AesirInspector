using System;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Use this attribute to prevent a type from being included in Odin systems.
	/// The attribute can be applied to Odin drawers, Odin property resolvers and Odin attribute processor types.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class OdinDontRegisterAttribute : Attribute
	{
	}
}
