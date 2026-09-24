using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>FoldoutGroup is used on any property, and organizes properties into a foldout.</para>
	/// <para>Use this to organize properties, and to allow the user to hide properties that are not relevant for them at the moment.</para>
	/// </summary>
	/// <example>
	/// <para>The following example shows how FoldoutGroup is used to organize properties into a foldout.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	///             	{
	///             		[FoldoutGroup("MyGroup")]
	///             		public int A;
	///
	///             		[FoldoutGroup("MyGroup")]
	///             		public int B;
	///
	///             		[FoldoutGroup("MyGroup")]
	///             		public int C;
	///             	}
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example shows how properties can be organizes into multiple foldouts.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	///             	{
	///             		[FoldoutGroup("First")]
	///             		public int A;
	///
	///             		[FoldoutGroup("First")]
	///             		public int B;
	///
	///             		[FoldoutGroup("Second")]
	///             		public int C;
	///             	}
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.BoxGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ButtonGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.TabGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.ToggleGroupAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.PropertyGroupAttribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public class FoldoutGroupAttribute : PropertyGroupAttribute
	{
		private bool expanded;

		/// <summary>
		/// Gets a value indicating whether or not the foldout should be expanded by default.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "expanded", "HasDefinedExpanded" })]
		public bool Expanded
		{
			get
			{
				return expanded;
			}
			set
			{
				expanded = value;
				HasDefinedExpanded = true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether or not the Expanded property has been set.
		/// </summary>
		public bool HasDefinedExpanded { get; private set; }

		/// <summary>
		/// Adds the property to the specified foldout group.
		/// </summary>
		/// <param name="groupName">Name of the foldout group.</param>
		/// <param name="order">The order of the group in the inspector.</param>
		public FoldoutGroupAttribute(string groupName, float order = 0f)
			: base(groupName, order)
		{
		}

		/// <summary>
		/// Adds the property to the specified foldout group.
		/// </summary>
		/// <param name="groupName">Name of the foldout group.</param>
		/// <param name="expanded">Whether or not the foldout should be expanded by default.</param>
		/// <param name="order">The order of the group in the inspector.</param>
		public FoldoutGroupAttribute(string groupName, bool expanded, float order = 0f)
			: base(groupName, order)
		{
			this.expanded = expanded;
			HasDefinedExpanded = true;
		}

		/// <summary>
		/// Combines the foldout property with another.
		/// </summary>
		/// <param name="other">The group to combine with.</param>
		protected override void CombineValuesWith(PropertyGroupAttribute other)
		{
			FoldoutGroupAttribute attr = other as FoldoutGroupAttribute;
			if (attr.HasDefinedExpanded)
			{
				HasDefinedExpanded = true;
				Expanded = attr.Expanded;
			}
			if (HasDefinedExpanded)
			{
				attr.HasDefinedExpanded = true;
				attr.Expanded = Expanded;
			}
		}
	}
}
