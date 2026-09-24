using System;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>Contains information about an editor type which is assigned to draw a certain type in the inspector.</para>
	/// <para>This class uses the <see cref="F:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig.TypeBinder" /> instance to bind types to names, and names to types.</para>
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfigDrawer" />.
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.InspectorTypeDrawingConfig" />.
	/// <seealso cref="!:EditorCompilation" />.
	[Serializable]
	public struct TypeDrawerPair : IEquatable<TypeDrawerPair>
	{
		/// <summary>
		/// A default, empty <see cref="T:Sirenix.OdinInspector.Editor.TypeDrawerPair" /> value.
		/// </summary>
		public static readonly TypeDrawerPair Default;

		/// <summary>
		/// The name of the type to be drawn.
		/// </summary>
		[SerializeField]
		public string DrawnTypeName;

		/// <summary>
		/// The name of the editor type.
		/// </summary>
		[SerializeField]
		public string EditorTypeName;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.TypeDrawerPair" /> struct.
		/// </summary>
		/// <param name="drawnType">The drawn type.</param>
		/// <exception cref="T:System.ArgumentNullException">drawnType is null</exception>
		public TypeDrawerPair(Type drawnType)
		{
			if (drawnType == null)
			{
				throw new ArgumentNullException("drawnType");
			}
			DrawnTypeName = InspectorTypeDrawingConfig.TypeBinder.BindToName(drawnType);
			EditorTypeName = string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.TypeDrawerPair" /> struct.
		/// </summary>
		/// <param name="drawnType">The drawn type.</param>
		/// <param name="editorType">The editor type.</param>
		/// <exception cref="T:System.ArgumentNullException">drawnType is null</exception>
		public TypeDrawerPair(Type drawnType, Type editorType)
			: this(drawnType)
		{
			if (editorType == null)
			{
				EditorTypeName = string.Empty;
			}
			else
			{
				EditorTypeName = InspectorTypeDrawingConfig.TypeBinder.BindToName(editorType);
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:Sirenix.OdinInspector.Editor.TypeDrawerPair" /> is equal to this instance.
		/// </summary>
		/// <param name="other">The <see cref="T:Sirenix.OdinInspector.Editor.TypeDrawerPair" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="T:Sirenix.OdinInspector.Editor.TypeDrawerPair" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public bool Equals(TypeDrawerPair other)
		{
			if (other.EditorTypeName == EditorTypeName)
			{
				return other.DrawnTypeName == DrawnTypeName;
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode()
		{
			return ((EditorTypeName != null) ? (EditorTypeName.GetHashCode() * 7) : 0) ^ ((DrawnTypeName != null) ? (DrawnTypeName.GetHashCode() * 13) : 0);
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" />, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="T:System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != typeof(TypeDrawerPair))
			{
				return false;
			}
			return Equals((TypeDrawerPair)obj);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(TypeDrawerPair x, TypeDrawerPair y)
		{
			return x.Equals(y);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(TypeDrawerPair x, TypeDrawerPair y)
		{
			return !x.Equals(y);
		}
	}
}
