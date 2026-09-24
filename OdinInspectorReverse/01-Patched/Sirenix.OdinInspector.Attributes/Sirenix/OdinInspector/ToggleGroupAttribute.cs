using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>ToggleGroup is used on any field, and create a toggleable group of options.</para>
	/// <para>Use this to create options that can be enabled or disabled.</para>
	/// </summary>
	/// <remarks>
	/// <para>The <see cref="P:Sirenix.OdinInspector.ToggleGroupAttribute.ToggleMemberName" /> functions as the ID for the ToggleGroup, and therefore all members of a toggle group must specify the same toggle member.</para>
	/// <note note="Note">This attribute does not support static members!</note>
	/// </remarks>
	/// <example>
	/// <para>The following example shows how ToggleGroup is used to create two separate toggleable groups.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	///             	{
	///             		// This attribute has a title specified for the group. The title only needs to be applied to a single attribute for a group.
	///             		[ToggleGroup("FirstToggle", order: -1, groupTitle: "First")]
	///             		public bool FirstToggle;
	///
	///             		[ToggleGroup("FirstToggle")]
	///             		public int MyInt;
	///
	///             		// This group specifies a member string as the title of the group. A property or a function can also be used.
	///             		[ToggleGroup("SecondToggle", titleStringMemberName: "SecondGroupTitle")]
	///             		public bool SecondToggle { get; set; }
	///
	///             		[ToggleGroup("SecondToggle")]
	///             		public float MyFloat;
	///
	///             		[HideInInspector]
	///             		public string SecondGroupTitle = "Second";
	///             	}
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.ToggleAttribute" />
	/// <seealso cref="!:ToggleListAttribute" />"/&gt;
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	public sealed class ToggleGroupAttribute : PropertyGroupAttribute
	{
		/// <summary>
		/// Title of the toggle group in the inspector.
		/// If <c>null</c> <see cref="P:Sirenix.OdinInspector.ToggleGroupAttribute.ToggleMemberName" /> will be used instead.
		/// </summary>
		public string ToggleGroupTitle;

		/// <summary>
		/// If true, all other open toggle groups will collapse once another one opens.
		/// </summary>
		[LabelWidth(160f)]
		public bool CollapseOthersOnExpand;

		/// <summary>
		/// Name of any bool field, property or function to enable or disable the ToggleGroup.
		/// </summary>
		public string ToggleMemberName => GroupName;

		/// <summary>
		/// Name of any string field, property or function, to title the toggle group in the inspector.
		/// If <c>null</c> <see cref="F:Sirenix.OdinInspector.ToggleGroupAttribute.ToggleGroupTitle" /> will be used instead.
		/// </summary>
		[Obsolete("Add a $ infront of group title instead, i.e: \"$MyStringMember\".")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public string TitleStringMemberName { get; set; }

		/// <summary>
		/// Creates a ToggleGroup. See <see cref="T:Sirenix.OdinInspector.ToggleGroupAttribute" />.
		/// </summary>
		/// <param name="toggleMemberName">Name of any bool field or property to enable or disable the ToggleGroup.</param>
		/// <param name="order">The order of the group.</param>
		/// <param name="groupTitle">Use this to name the group differently than toggleMemberName.</param>
		public ToggleGroupAttribute(string toggleMemberName, float order = 0f, string groupTitle = null)
			: base(toggleMemberName, order)
		{
			ToggleGroupTitle = groupTitle;
			CollapseOthersOnExpand = true;
		}

		/// <summary>
		/// Creates a ToggleGroup. See <see cref="T:Sirenix.OdinInspector.ToggleGroupAttribute" />.
		/// </summary>
		/// <param name="toggleMemberName">Name of any bool field or property to enable or disable the ToggleGroup.</param>
		/// <param name="groupTitle">Use this to name the group differently than toggleMemberName.</param>
		public ToggleGroupAttribute(string toggleMemberName, string groupTitle)
			: this(toggleMemberName, 0f, groupTitle)
		{
		}

		/// <summary>
		/// Obsolete constructor overload.
		/// </summary>
		/// <param name="toggleMemberName">Obsolete overload.</param>
		/// <param name="order">Obsolete overload.</param>
		/// <param name="groupTitle">Obsolete overload.</param>
		/// <param name="titleStringMemberName">Obsolete overload.</param>
		[Obsolete("Use [ToggleGroup(\"toggleMemberName\", groupTitle: \"$titleStringMemberName\")] instead")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ToggleGroupAttribute(string toggleMemberName, float order, string groupTitle, string titleStringMemberName)
			: base(toggleMemberName, order)
		{
			ToggleGroupTitle = groupTitle;
			CollapseOthersOnExpand = true;
		}

		/// <summary>
		/// Combines the ToggleGroup with another ToggleGroup.
		/// </summary>
		/// <param name="other">Another ToggleGroup.</param>
		protected override void CombineValuesWith(PropertyGroupAttribute other)
		{
			ToggleGroupAttribute attr = other as ToggleGroupAttribute;
			if (ToggleGroupTitle == null)
			{
				ToggleGroupTitle = attr.ToggleGroupTitle;
			}
			else if (attr.ToggleGroupTitle == null)
			{
				attr.ToggleGroupTitle = ToggleGroupTitle;
			}
			CollapseOthersOnExpand = CollapseOthersOnExpand && attr.CollapseOthersOnExpand;
			attr.CollapseOthersOnExpand = CollapseOthersOnExpand;
		}
	}
}
