using System;
using System.ComponentModel;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Hides the ObjectMembers in Visual Studio IntelliSense
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IHideObjectMembers
	{
		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" />, is equal to this instance.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		new bool Equals(object obj);

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		new int GetHashCode();

		/// <summary>
		/// Gets the type.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		new Type GetType();

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		new string ToString();
	}
}
