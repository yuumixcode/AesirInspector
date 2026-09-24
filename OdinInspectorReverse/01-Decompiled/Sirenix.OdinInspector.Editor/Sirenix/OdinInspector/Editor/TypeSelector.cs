using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class TypeSelector : OdinSelector<Type>
	{
		private static Dictionary<AssemblyCategory, List<OdinMenuItem>> cachedAllTypesMenuItems = new Dictionary<AssemblyCategory, List<OdinMenuItem>>();

		private IEnumerable<Type> types;

		private AssemblyCategory assemblyCategories;

		private bool supportsMultiSelect;

		[HideInInspector]
		public bool HideNamespaces;

		[HideInInspector]
		public bool FlattenTree;

		private Type lastType;

		public override string Title => null;

		[Obsolete("AssemblyTypeFlags have been made obsolete, because they cannot be determined accurately for all assemblies. Use AssemblyUtilities.GetAssemblyCategory(assembly) instead.", false)]
		public TypeSelector(AssemblyTypeFlags assemblyFlags, bool supportsMultiSelect)
		{
			types = null;
			this.supportsMultiSelect = supportsMultiSelect;
			assemblyCategories = AssemblyUtilities.LossyBadConvertAssemblyTypeFlagsToCategories(assemblyFlags);
		}

		public TypeSelector(AssemblyCategory assemblyCategories, bool supportsMultiSelect)
		{
			types = null;
			this.supportsMultiSelect = supportsMultiSelect;
			this.assemblyCategories = assemblyCategories;
		}

		public TypeSelector(IEnumerable<Type> types, bool supportsMultiSelect)
		{
			this.types = ((types != null) ? OrderTypes(types) : types);
			this.supportsMultiSelect = supportsMultiSelect;
		}

		private static IEnumerable<Type> OrderTypes(IEnumerable<Type> types)
		{
			return from x in types
				orderby x.Namespace.IsNullOrWhitespace() descending, x.Namespace, x.Name
				select x;
		}

		public override bool IsValidSelection(IEnumerable<Type> collection)
		{
			return collection.Any();
		}

		/// <summary>
		/// Builds the selection tree.
		/// </summary>
		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			tree.Config.UseCachedExpandedStates = false;
			tree.DefaultMenuStyle.NotSelectedIconAlpha = 1f;
			tree.Config.SelectMenuItemsOnMouseDown = true;
			if (types == null)
			{
				if (cachedAllTypesMenuItems.TryGetValue(assemblyCategories, out var items))
				{
					AddRecursive(tree, items, tree.MenuItems);
				}
				else
				{
					IEnumerable<Type> assemblyTypes = OrderTypes(AssemblyUtilities.GetTypes(assemblyCategories).Where(delegate(Type x)
					{
						if (x.Name == null)
						{
							return false;
						}
						string text = x.Name.TrimStart(Array.Empty<char>());
						return text.Length != 0 && char.IsLetter(text[0]);
					}));
					foreach (Type t in assemblyTypes)
					{
						string niceName = t.GetNiceName();
						string path = GetTypeNamePath(t, niceName);
						OdinMenuItem last = tree.AddObjectAtPath(path, t).AddThumbnailIcons().Last();
						last.SearchString = ((niceName == path) ? path : (niceName + "|" + path));
					}
					cachedAllTypesMenuItems[assemblyCategories] = tree.MenuItems;
				}
			}
			else
			{
				foreach (Type t2 in types)
				{
					string niceName2 = t2.GetNiceName();
					string path2 = GetTypeNamePath(t2, niceName2);
					OdinMenuItem last2 = tree.AddObjectAtPath(path2, t2).Last();
					last2.SearchString = ((niceName2 == path2) ? path2 : (niceName2 + "|" + path2));
					if (FlattenTree && t2.Namespace != null && !HideNamespaces)
					{
						last2.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(last2.OnDrawItem, (Action<OdinMenuItem>)delegate(OdinMenuItem x)
						{
							GUI.Label(x.Rect.Padding(10f, 0f).AlignCenterY(16f), t2.Namespace, SirenixGUIStyles.RightAlignedGreyMiniLabel);
						});
					}
				}
				tree.EnumerateTree((OdinMenuItem x) => x.Value != null, includeRootNode: false).AddThumbnailIcons();
			}
			tree.Selection.SupportsMultiSelect = supportsMultiSelect;
			tree.Selection.SelectionChanged += delegate
			{
				lastType = base.SelectionTree.Selection.Select((OdinMenuItem x) => x.Value).OfType<Type>().LastOrDefault() ?? lastType;
			};
		}

		private string GetTypeNamePath(Type t, string niceName)
		{
			string name = niceName;
			if (!FlattenTree && !string.IsNullOrEmpty(t.Namespace) && !HideNamespaces)
			{
				char separator = (FlattenTree ? '.' : '/');
				name = t.Namespace + separator + name;
			}
			return name;
		}

		private static void AddRecursive(OdinMenuTree tree, List<OdinMenuItem> source, List<OdinMenuItem> destination)
		{
			destination.Capacity = source.Count;
			for (int i = 0; i < source.Count; i++)
			{
				OdinMenuItem item = source[i];
				OdinMenuItem clone = new OdinMenuItem(tree, item.Name, item.Value).AddThumbnailIcon(preferAssetPreviewAsIcon: false);
				clone.SearchString = item.SearchString;
				destination.Add(clone);
				if (item.ChildMenuItems.Count > 0)
				{
					AddRecursive(tree, item.ChildMenuItems, clone.ChildMenuItems);
				}
			}
		}

		/// <summary>
		/// 450
		/// </summary>
		protected override float DefaultWindowWidth()
		{
			return 450f;
		}

		[OnInspectorGUI]
		[PropertyOrder(10f)]
		private void ShowTypeInfo()
		{
			string fullTypeName = "";
			string assembly = "";
			string baseType = "";
			int labelHeight = 16;
			Rect rect = GUILayoutUtility.GetRect(0f, labelHeight * 3 + 8).Padding(10f, 4f).AlignTop(labelHeight);
			int labelWidth = 75;
			if (lastType != null)
			{
				fullTypeName = lastType.GetNiceFullName();
				assembly = lastType.Assembly.GetName().Name;
				baseType = ((lastType.BaseType == null) ? "" : lastType.BaseType.GetNiceFullName());
			}
			GUIStyle style = SirenixGUIStyles.LeftAlignedGreyMiniLabel;
			GUI.Label(rect.AlignLeft(labelWidth), "Type Name", style);
			GUI.Label(rect.AlignRight(rect.width - (float)labelWidth), fullTypeName, style);
			rect.y += labelHeight;
			GUI.Label(rect.AlignLeft(labelWidth), "Base Type", style);
			GUI.Label(rect.AlignRight(rect.width - (float)labelWidth), baseType, style);
			rect.y += labelHeight;
			GUI.Label(rect.AlignLeft(labelWidth), "Assembly", style);
			GUI.Label(rect.AlignRight(rect.width - (float)labelWidth), assembly, style);
		}

		/// <summary>
		/// Sets the selected types.
		/// </summary>
		public override void SetSelection(Type selected)
		{
			base.SetSelection(selected);
			base.SelectionTree.Selection.SelectMany((OdinMenuItem x) => x.GetParentMenuItemsRecursive(includeSelf: false)).ForEach(delegate(OdinMenuItem x)
			{
				x.Toggled = true;
			});
		}
	}
}
