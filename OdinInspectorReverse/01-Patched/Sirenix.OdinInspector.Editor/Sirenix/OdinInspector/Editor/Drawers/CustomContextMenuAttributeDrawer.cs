using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Adds a generic menu option to properties marked with <see cref="T:Sirenix.OdinInspector.CustomContextMenuAttribute" />.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.CustomContextMenuAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.DisableContextMenuAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	[DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
	public sealed class CustomContextMenuAttributeDrawer : OdinAttributeDrawer<CustomContextMenuAttribute>, IDefinesGenericMenuItems
	{
		private class ContextMenuInfo
		{
			public ValueResolver<string> Name;

			public ActionResolver Action;
		}

		private ContextMenuInfo info;

		private PropertyContext<Dictionary<CustomContextMenuAttribute, ContextMenuInfo>> contextMenuInfos;

		private PropertyContext<bool> populated;

		/// <summary>
		/// Populates the generic menu for the property.
		/// </summary>
		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (populated.Value)
			{
				return;
			}
			populated.Value = true;
			if (contextMenuInfos.Value == null || contextMenuInfos.Value.Count <= 0)
			{
				return;
			}
			if (genericMenu.GetItemCount() > 0)
			{
				genericMenu.AddSeparator("");
			}
			foreach (KeyValuePair<CustomContextMenuAttribute, ContextMenuInfo> item in contextMenuInfos.Value.OrderBy((KeyValuePair<CustomContextMenuAttribute, ContextMenuInfo> n) => n.Key.MenuItem ?? ""))
			{
				ContextMenuInfo info = item.Value;
				if (info.Action == null)
				{
					genericMenu.AddDisabledItem(new GUIContent(item.Key.MenuItem + " (Invalid)"));
					continue;
				}
				string name = info.Name.GetValue();
				genericMenu.AddItem(new GUIContent(name), on: false, delegate
				{
					base.Property.RecordForUndo(name);
					info.Action.DoActionForAllSelectionIndices();
				});
			}
		}

		protected override void Initialize()
		{
			InspectorProperty property = base.Property;
			CustomContextMenuAttribute attribute = base.Attribute;
			contextMenuInfos = property.Context.GetGlobal("CustomContextMenu", (Dictionary<CustomContextMenuAttribute, ContextMenuInfo>)null);
			populated = property.Context.GetGlobal("CustomContextMenu_Populated", defaultValue: false);
			if (contextMenuInfos.Value == null)
			{
				contextMenuInfos.Value = new Dictionary<CustomContextMenuAttribute, ContextMenuInfo>();
			}
			if (!contextMenuInfos.Value.TryGetValue(attribute, out info))
			{
				info = new ContextMenuInfo();
				info.Name = ValueResolver.GetForString(base.Property, attribute.MenuItem);
				info.Action = ActionResolver.Get(base.Property, attribute.Action);
				contextMenuInfos.Value[attribute] = info;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			populated.Value = false;
			info.Name.DrawError();
			info.Action.DrawError();
			CallNextDrawer(label);
		}
	}
}
