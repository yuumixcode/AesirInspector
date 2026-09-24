using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all properties grouped together with the <see cref="T:Sirenix.OdinInspector.TabGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.TabGroupAttribute" />
	public class TabGroupAttributeDrawer : OdinGroupDrawer<TabGroupAttribute>, IOnSelfStateChangedNotification
	{
		private class TabInfo
		{
			public string TabId;

			public SdfIconType Icon;

			public ValueResolver<Color?> TextColor;

			public List<InspectorProperty> InspectorProperties;

			public ValueResolver<string> Title;

			public string NiceName;
		}

		public const string CurrentTabIndexKey = "CurrentTabIndex";

		public const string CurrentTabNameKey = "CurrentTabName";

		public const string TabCountKey = "TabCount";

		private bool isChangingTabName;

		private GUITabGroup tabGroup;

		private List<TabInfo> tabs;

		private bool initialized;

		protected override void Initialize()
		{
			this.tabGroup = new GUITabGroup();
			this.tabGroup.AnimationSpeed = 1f / SirenixEditorGUI.TabPageSlideAnimationDuration;
			this.tabGroup.TabLayouting = base.Attribute.TabLayouting;
			tabs = new List<TabInfo>();
			for (int j = 0; j < base.Property.Children.Count; j++)
			{
				InspectorProperty child = base.Property.Children[j];
				if (child.Info.PropertyType == PropertyType.Group)
				{
					TabGroupAttribute.TabSubGroupAttribute tabAttr = child.GetAttribute<TabGroupAttribute.TabSubGroupAttribute>();
					if (tabAttr != null)
					{
						string tabId = tabAttr.GroupName;
						string tabName = tabAttr.Name ?? child.Name.TrimStart(new char[1] { '#' });
						TabInfo tab = new TabInfo
						{
							TabId = tabId,
							Icon = tabAttr.Icon,
							Title = ValueResolver.GetForString(base.Property, tabName),
							NiceName = child.NiceName,
							InspectorProperties = child.Children.ToList(),
							TextColor = ValueResolver.Get<Color?>(base.Property, tabAttr.TextColor)
						};
						GUITabPage tabGroup = this.tabGroup.RegisterTab(tab.TabId);
						tabGroup.Icon = tabAttr.Icon;
						tabGroup.TextColor = tab.TextColor.GetValue();
						tabs.Add(tab);
					}
				}
			}
			base.Property.State.Create("CurrentTabIndex", persistent: true, 0);
			base.Property.State.Create("TabCount", persistent: false, tabs.Count);
			int currentIndex = GetClampedCurrentIndex();
			if (currentIndex >= 0 && currentIndex < tabs.Count)
			{
				TabInfo currentTab = tabs[currentIndex];
				GUITabPage selectedTabGroup = this.tabGroup.RegisterTab(currentTab.TabId);
				this.tabGroup.SetCurrentPage(selectedTabGroup);
				isChangingTabName = true;
				base.Property.State.Create("CurrentTabName", persistent: false, currentTab.TabId);
				isChangingTabName = false;
			}
			initialized = true;
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			foreach (TabInfo item in tabs)
			{
				item.TextColor.DrawError();
			}
			InspectorProperty property = base.Property;
			TabGroupAttribute attribute = base.Attribute;
			if (attribute.HideTabGroupIfTabGroupOnlyHasOneTab && tabs.Count <= 1)
			{
				for (int i = 0; i < tabs.Count; i++)
				{
					int pageCount = tabs[i].InspectorProperties.Count;
					for (int j = 0; j < pageCount; j++)
					{
						InspectorProperty child = tabs[i].InspectorProperties[j];
						child.Update();
						child.Draw(child.Label);
					}
				}
				return;
			}
			tabGroup.AnimationSpeed = 1f / SirenixEditorGUI.TabPageSlideAnimationDuration;
			tabGroup.FixedHeight = attribute.UseFixedHeight;
			GetClampedCurrentIndex();
			SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyPadding);
			tabGroup.BeginGroup(drawToolbar: true, attribute.Paddingless ? GUIStyle.none : null);
			property.State.Set("TabCount", tabs.Count);
			for (int k = 0; k < tabs.Count; k++)
			{
				TabInfo tab = tabs[k];
				GUITabPage page = tabGroup.RegisterTab(tabs[k].TabId);
				page.Title = tab.Title.GetValue();
				page.TextColor = tab.TextColor.GetValue();
				if (string.IsNullOrEmpty(page.Title))
				{
					page.Tooltip = tab.NiceName;
				}
				else
				{
					page.Tooltip = null;
				}
				if (tabGroup.NextPage == null && tabGroup.CurrentPage == page)
				{
					property.State.Set("CurrentTabIndex", k);
				}
				if (page.BeginPage())
				{
					int pageCount2 = tabs[k].InspectorProperties.Count;
					for (int l = 0; l < pageCount2; l++)
					{
						InspectorProperty child2 = tabs[k].InspectorProperties[l];
						child2.Update();
						child2.Draw(child2.Label);
					}
				}
				page.EndPage();
			}
			tabGroup.EndGroup();
			SirenixEditorGUI.EndIndentedVertical();
		}

		private int GetClampedCurrentIndex()
		{
			int currentIndex = base.Property.State.Get<int>("CurrentTabIndex");
			if (currentIndex < 0)
			{
				currentIndex = 0;
				base.Property.State.Set("CurrentTabIndex", currentIndex);
			}
			else if (currentIndex >= tabs.Count)
			{
				currentIndex = tabs.Count - 1;
				base.Property.State.Set("CurrentTabIndex", currentIndex);
			}
			return currentIndex;
		}

		public void OnSelfStateChanged(string state)
		{
			if (!initialized)
			{
				return;
			}
			if (state == "CurrentTabIndex")
			{
				int index = GetClampedCurrentIndex();
				TabInfo tab = tabs[index];
				isChangingTabName = true;
				base.Property.State.Set("CurrentTabName", tab.TabId);
				isChangingTabName = false;
				tabGroup.GoToPage(tabs[index].TabId);
			}
			else
			{
				if (!(state == "CurrentTabName") || isChangingTabName)
				{
					return;
				}
				string name = base.Property.State.Get<string>("CurrentTabName");
				int index2 = -1;
				for (int i = 0; i < tabs.Count; i++)
				{
					if (tabs[i].TabId == name)
					{
						index2 = i;
						break;
					}
				}
				if (index2 == -1)
				{
					Debug.LogError("There is no tab named '" + name + "' in the tab group '" + base.Property.NiceName + "'!");
					index2 = base.Property.State.Get<int>("CurrentTabIndex");
					isChangingTabName = true;
					base.Property.State.Set("CurrentTabName", tabs[index2].TabId);
					isChangingTabName = false;
				}
				else
				{
					base.Property.State.Set("CurrentTabIndex", index2);
				}
			}
		}
	}
}
