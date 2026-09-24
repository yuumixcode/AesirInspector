using System;
using System.ComponentModel;

namespace Sirenix.Serialization
{
	/// <summary>
	/// <para>
	/// Causes Odin's inspector to completely ignore a given member, preventing it from even being included in an Odin PropertyTree,
	/// and such will not cause any performance hits in the inspector.
	/// </para>
	/// <para>Note that Odin can still serialize an excluded member - it is merely ignored in the inspector itself.</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	[Obsolete("Use [HideInInspector] instead - it now also excludes the member completely from becoming a property in the property tree.", false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class ExcludeDataFromInspectorAttribute : Attribute
	{
	}
}
