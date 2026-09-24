using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sirenix.OdinInspector.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// <para>TabGroup is used on any property, and organizes properties into different tabs.</para>
	/// <para>Use this to organize different value to make a clean and easy to use inspector.</para>
	/// </summary>
	/// <remarks>
	/// <para>Use groups to create multiple tab groups, each with multiple tabs and even sub tabs.</para>
	/// </remarks>
	/// <example>
	/// <para>The following example shows how to create a tab group with two tabs.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	///             	{
	///             		[TabGroup("First")]
	///             		public int MyFirstInt;
	///
	///             		[TabGroup("First")]
	///             		public int AnotherInt;
	///
	///             		[TabGroup("Second")]
	///             		public int MySecondInt;
	///             	}
	/// </code>
	/// </example>
	/// <example>
	/// <para>The following example shows how multiple groups of tabs can be created.</para>
	/// <code>
	///             	public class MyComponent : MonoBehaviour
	///             	{
	///             		[TabGroup("A", "FirstGroup")]
	///             		public int FirstGroupA;
	///
	///             		[TabGroup("B", "FirstGroup")]
	///             		public int FirstGroupB;
	///
	///             		// The second tab group has been configured to have constant height across all tabs.
	///             		[TabGroup("A", "SecondGroup", true)]
	///             		public int SecondgroupA;
	///
	///             		[TabGroup("B", "SecondGroup")]
	///             		public int SecondGroupB;
	///
	///             		[TabGroup("B", "SecondGroup")]
	///             		public int AnotherInt;
	///             	}
	/// </code>
	/// </example>
	/// <example>
	/// <para>This example demonstrates how multiple tabs groups can be combined to create tabs in tabs.</para>
	/// <code>
	/// public class MyComponent : MonoBehaviour
	/// {
	///     [TabGroup("ParentGroup", "First Tab")]
	///     public int A;
	///
	///     [TabGroup("ParentGroup", "Second Tab")]
	///     public int B;
	///
	///     // Specify 'First Tab' as a group, and another child group to the 'First Tab' group.
	///     [TabGroup("ParentGroup/First Tab/InnerGroup", "Inside First Tab A")]
	///     public int C;
	///
	///     [TabGroup("ParentGroup/First Tab/InnerGroup", "Inside First Tab B")]
	///     public int D;
	///
	///     [TabGroup("ParentGroup/Second Tab/InnerGroup", "Inside Second Tab")]
	///     public int E;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="!:TabListAttribute" />
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	public class TabGroupAttribute : PropertyGroupAttribute, ISubGroupProviderAttribute
	{
		[Conditional("UNITY_EDITOR")]
		public class TabSubGroupAttribute : PropertyGroupAttribute
		{
			public string Name;

			public SdfIconType Icon;

			/// <summary> Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow. </summary>
			[ColorResolver]
			public string TextColor;

			public TabSubGroupAttribute(TabGroupAttribute tab, string groupId, float order)
				: base(groupId, order)
			{
				if (tab == null)
				{
					Name = null;
					Icon = SdfIconType.None;
					TextColor = null;
				}
				else
				{
					Name = tab.TabName;
					Icon = tab.Icon;
					TextColor = tab.TextColor;
				}
			}

			public TabSubGroupAttribute(string groupId, float order, string tabName, SdfIconType tabIcon, string textColor)
				: base(groupId, order)
			{
				Name = tabName;
				Icon = tabIcon;
				TextColor = textColor;
			}

			protected override void CombineValuesWith(PropertyGroupAttribute other)
			{
				if (other is TabSubGroupAttribute otherTab)
				{
					if (TextColor == null)
					{
						TextColor = otherTab.TextColor;
					}
					if (Icon == SdfIconType.None)
					{
						Icon = otherTab.Icon;
					}
					if (Name == null)
					{
						Name = otherTab.Name;
					}
				}
			}
		}

		/// <summary>
		/// The default tab group name which is used when the single-parameter constructor is called.
		/// </summary>
		public const string DEFAULT_NAME = "_DefaultTabGroup";

		/// <summary>
		/// Name of the tab.
		/// </summary>
		[HideInInspector]
		public string TabName;

		[HideInInspector]
		public string TabId;

		/// <summary>
		/// Should this tab be the same height as the rest of the tab group.
		/// </summary>
		public bool UseFixedHeight;

		/// <summary>
		/// If true, the content of each page will not be contained in any box.
		/// </summary>
		public bool Paddingless;

		/// <summary>
		/// If true, the tab group will be hidden if it only contains one tab.
		/// </summary>
		[LabelWidth(270f)]
		public bool HideTabGroupIfTabGroupOnlyHasOneTab;

		/// <summary> Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow. </summary>
		[HideInInspector]
		public string TextColor;

		[HideInInspector]
		public SdfIconType Icon;

		/// <summary>
		/// Specify how tabs should be layouted.
		/// </summary>
		public TabLayouting TabLayouting;

		/// <summary>
		/// Name of all tabs in this group.
		/// </summary>
		public List<TabGroupAttribute> Tabs;

		/// <summary>
		/// Organizes the property into the specified tab in the default group.
		/// Default group name is '_DefaultTabGroup'
		/// </summary>
		/// <param name="tab">The tab.</param>
		/// <param name="useFixedHeight">if set to <c>true</c> [use fixed height].</param>
		/// <param name="order">The order.</param>
		public TabGroupAttribute(string tab, bool useFixedHeight = false, float order = 0f)
			: this("_DefaultTabGroup", tab, useFixedHeight, order)
		{
		}

		/// <summary>
		/// Organizes the property into the specified tab in the specified group.
		/// </summary>
		/// <param name="group">The group to attach the tab to.</param>
		/// <param name="tab">The name of the tab.</param>
		/// <param name="useFixedHeight">Set to true to have a constant height across the entire tab group.</param>
		/// <param name="order">The order of the group.</param>
		public TabGroupAttribute(string group, string tab, bool useFixedHeight = false, float order = 0f)
			: base(group, order)
		{
			TabId = tab;
			UseFixedHeight = useFixedHeight;
			Tabs = new List<TabGroupAttribute> { this };
		}

		/// <summary>
		/// Organizes the property into the specified tab in the specified group.
		/// </summary>
		/// <param name="group">The group to attach the tab to.</param>
		/// <param name="tab">The name of the tab.</param>
		/// <param name="useFixedHeight">Set to true to have a constant height across the entire tab group.</param>
		/// <param name="order">The order of the group.</param>
		public TabGroupAttribute(string group, string tab, SdfIconType icon, bool useFixedHeight = false, float order = 0f)
			: this(group, tab, useFixedHeight, order)
		{
			Icon = icon;
		}

		protected override void CombineValuesWith(PropertyGroupAttribute other)
		{
			TabGroupAttribute otherTab = other as TabGroupAttribute;
			if (otherTab.TabId == null)
			{
				return;
			}
			if (otherTab.TabLayouting != TabLayouting.MultiRow)
			{
				TabLayouting = otherTab.TabLayouting;
			}
			UseFixedHeight = UseFixedHeight || otherTab.UseFixedHeight;
			Paddingless = Paddingless || otherTab.Paddingless;
			HideTabGroupIfTabGroupOnlyHasOneTab = HideTabGroupIfTabGroupOnlyHasOneTab || otherTab.HideTabGroupIfTabGroupOnlyHasOneTab;
			bool hasTabIDAlready = false;
			for (int i = 0; i < Tabs.Count; i++)
			{
				TabGroupAttribute tab = Tabs[i];
				if (tab.TabId == otherTab.TabId)
				{
					if (tab.TextColor == null)
					{
						tab.TextColor = otherTab.TextColor;
					}
					if (tab.Icon == SdfIconType.None)
					{
						tab.Icon = otherTab.Icon;
					}
					if (tab.TabName == null)
					{
						tab.TabName = otherTab.TabName;
					}
					hasTabIDAlready = true;
					break;
				}
			}
			if (!hasTabIDAlready)
			{
				Tabs.Add(otherTab);
			}
		}

		IList<PropertyGroupAttribute> ISubGroupProviderAttribute.GetSubGroupAttributes()
		{
			int count = 0;
			List<PropertyGroupAttribute> result = new List<PropertyGroupAttribute>(Tabs.Count)
			{
				new TabSubGroupAttribute(this, GroupID + "/" + TabId, count++)
			};
			foreach (TabGroupAttribute tab in Tabs)
			{
				if (tab.TabId != TabId)
				{
					result.Add(new TabSubGroupAttribute(tab, GroupID + "/" + tab.TabId, count++));
				}
			}
			return result;
		}

		string ISubGroupProviderAttribute.RepathMemberAttribute(PropertyGroupAttribute attr)
		{
			TabGroupAttribute tabAttr = (TabGroupAttribute)attr;
			return GroupID + "/" + tabAttr.TabId;
		}
	}
}
