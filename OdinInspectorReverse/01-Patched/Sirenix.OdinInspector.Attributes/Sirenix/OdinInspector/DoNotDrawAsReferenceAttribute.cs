using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Indicates that the member should not be drawn as a value reference, if it becomes a reference to another value in the tree. Beware, and use with care! This may lead to infinite draw loops!
	/// </summary>
	[Conditional("UNITY_EDITOR")]
	public sealed class DoNotDrawAsReferenceAttribute : Attribute
	{
	}
}
