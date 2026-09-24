using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Class with utility methods for <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />s and <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />s.
	/// </summary>
	/// <example>
	/// <code>
	/// OdinMenuTree tree = new OdinMenuTree();
	/// tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
	///     .AddThumbnailIcons();
	/// tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");
	/// // etc...
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuStyle" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	public static class OdinMenuTreeExtensions
	{
		private class OdinMenuItemComparer : IComparer<OdinMenuItem>
		{
			public Comparison<OdinMenuItem> CustomComparison;

			public bool PlaceFoldersFirst;

			public bool IgnoreLeadingZeroes;

			public bool IgnoreWhiteSpace;

			public bool IgnoreCase;

			public OdinMenuItemComparer(Comparison<OdinMenuItem> customComparison = null)
			{
				CustomComparison = customComparison;
			}

			public int Compare(OdinMenuItem a, OdinMenuItem b)
			{
				if (CustomComparison != null)
				{
					return CustomComparison(a, b);
				}
				if (PlaceFoldersFirst)
				{
					if (a.ChildMenuItems.Count > 0 && b.ChildMenuItems.Count == 0)
					{
						return -1;
					}
					if (b.ChildMenuItems.Count > 0 && a.ChildMenuItems.Count == 0)
					{
						return 1;
					}
				}
				return StringUtilities.NumberAwareStringCompare(a.SmartName, b.SmartName, IgnoreLeadingZeroes, IgnoreWhiteSpace, IgnoreCase);
			}
		}

		[ShowOdinSerializedPropertiesInInspector]
		private class SerializedValueWrapper
		{
			private object instance;

			[HideLabel]
			[ShowInInspector]
			[HideReferenceObjectPicker]
			public object Instance
			{
				get
				{
					return instance;
				}
				set
				{
				}
			}

			public SerializedValueWrapper(object obj)
			{
				instance = obj;
			}
		}

		private static List<OdinMenuItem> cache = new List<OdinMenuItem>(5);

		/// <summary>
		/// Adds the menu item at the specified menu item path and populates the result list with all menu items created in order to add the menuItem at the specified path.
		/// </summary>
		/// <param name="tree">The tree instance.</param>
		/// <param name="result">The result list.</param>
		/// <param name="path">The menu item path.</param>
		/// <param name="menuItem">The menu item.</param>
		public static void AddMenuItemAtPath(this OdinMenuTree tree, ICollection<OdinMenuItem> result, string path, OdinMenuItem menuItem)
		{
			OdinMenuItem curr = tree.Root;
			if (!string.IsNullOrEmpty(path))
			{
				if (path[0] == '/' || path[path.Length - 1] == '/')
				{
					path = path.Trim();
				}
				int iFrom = 0;
				int iTo = 0;
				do
				{
					iTo = path.IndexOf('/', iFrom);
					string name;
					if (iTo < 0)
					{
						iTo = path.Length - 1;
						name = path.Substring(iFrom, iTo - iFrom + 1);
					}
					else
					{
						name = path.Substring(iFrom, iTo - iFrom);
					}
					List<OdinMenuItem> childs = curr.ChildMenuItems;
					OdinMenuItem child = null;
					for (int i = childs.Count - 1; i >= 0; i--)
					{
						if (childs[i].Name == name)
						{
							child = childs[i];
							break;
						}
					}
					if (child == null)
					{
						child = new OdinMenuItem(tree, name, null);
						curr.ChildMenuItems.Add(child);
					}
					result.Add(child);
					curr = child;
					iFrom = iTo + 1;
				}
				while (iTo != path.Length - 1);
			}
			List<OdinMenuItem> currChilds = curr.ChildMenuItems;
			OdinMenuItem oldItem = null;
			for (int i2 = currChilds.Count - 1; i2 >= 0; i2--)
			{
				if (currChilds[i2].Name == menuItem.Name)
				{
					oldItem = currChilds[i2];
					break;
				}
			}
			if (oldItem != null)
			{
				curr.ChildMenuItems.Remove(oldItem);
				menuItem.ChildMenuItems.AddRange(oldItem.ChildMenuItems);
			}
			curr.ChildMenuItems.Add(menuItem);
			result.Add(menuItem);
		}

		/// <summary>
		/// Adds the menu item at specified menu item path, and returns all menu items created in order to add the menuItem at the specified path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="path">The menu item path.</param>
		/// <param name="menuItem">The menu item.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddMenuItemAtPath(this OdinMenuTree tree, string path, OdinMenuItem menuItem)
		{
			cache.Clear();
			tree.AddMenuItemAtPath(cache, path, menuItem);
			return cache;
		}

		/// <summary>
		/// Gets the menu item at the specified path, returns null non was found.
		/// </summary>
		public static OdinMenuItem GetMenuItem(this OdinMenuTree tree, string menuPath)
		{
			OdinMenuItem curr = tree.Root;
			if (!string.IsNullOrEmpty(menuPath))
			{
				menuPath = menuPath.Trim(new char[1] { '/' }) + "/";
				int iFrom = 0;
				int iTo = 0;
				do
				{
					iTo = menuPath.IndexOf('/', iFrom);
					string name = menuPath.Substring(iFrom, iTo - iFrom);
					OdinMenuItem child = curr.ChildMenuItems.FirstOrDefault((OdinMenuItem x) => x.Name == name) ?? curr.ChildMenuItems.FirstOrDefault((OdinMenuItem x) => x.SmartName == name);
					if (child == null)
					{
						return null;
					}
					curr = child;
					iFrom = iTo + 1;
				}
				while (iTo != menuPath.Length - 1);
			}
			return curr;
		}

		/// <summary>
		/// Adds all asset instances from the specified path and type into a single <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" /> at the specified menu item path, and returns all menu items created in order to add the menuItem at the specified path.. 
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuPath">The menu item path.</param>
		/// <param name="assetFolderPath">The asset folder path.</param>
		/// <param name="type">The type of objects.</param>
		/// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddAllAssetsAtPathCombined(this OdinMenuTree tree, string menuPath, string assetFolderPath, Type type, bool includeSubDirectories = false)
		{
			assetFolderPath = (assetFolderPath ?? "").TrimEnd(new char[1] { '/' }) + "/";
			string assetFolderPathLower = assetFolderPath.ToLower();
			if (!assetFolderPathLower.StartsWith("assets/") && !assetFolderPathLower.StartsWith("packages/"))
			{
				assetFolderPath = "Assets/" + assetFolderPath;
			}
			assetFolderPath = assetFolderPath.TrimEnd(new char[1] { '/' }) + "/";
			List<Func<object>> assets = (from x in AssetDatabase.GetAllAssetPaths()
				where includeSubDirectories ? x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase) : (string.Compare(PathUtilities.GetDirectoryName(x).Trim(new char[1] { '/' }), assetFolderPath.Trim(new char[1] { '/' }), ignoreCase: true) == 0)
				select x).Select((Func<string, Func<object>>)delegate(string x)
			{
				UnityEngine.Object tmp = null;
				return delegate
				{
					if (tmp == null)
					{
						tmp = AssetDatabase.LoadAssetAtPath(x, type);
					}
					return tmp;
				};
			}).ToList();
			SplitMenuPath(menuPath, out var path, out var menu);
			return tree.AddMenuItemAtPath(path, new OdinMenuItem(tree, menu, assets));
		}

		/// <summary>
		/// Adds all assets at the specified path. Each asset found gets its own menu item inside the specified menu item path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuPath">The menu item path.</param>
		/// <param name="assetFolderPath">The asset folder path.</param>
		/// <param name="type">The type.</param>
		/// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
		/// <param name="flattenSubDirectories">If true, sub-directories in the assetFolderPath will no longer get its own sub-menu item at the specified menu item path.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddAllAssetsAtPath(this OdinMenuTree tree, string menuPath, string assetFolderPath, Type type, bool includeSubDirectories = false, bool flattenSubDirectories = false)
		{
			assetFolderPath = (assetFolderPath ?? "").TrimEnd(new char[1] { '/' }) + "/";
			string assetFolderPathLower = assetFolderPath.ToLower();
			if (!assetFolderPathLower.StartsWith("assets/") && !assetFolderPathLower.StartsWith("packages/"))
			{
				assetFolderPath = "Assets/" + assetFolderPath;
			}
			assetFolderPath = assetFolderPath.TrimEnd(new char[1] { '/' }) + "/";
			IEnumerable<string> assets = from x in AssetDatabase.GetAllAssetPaths()
				where includeSubDirectories ? x.StartsWith(assetFolderPath, StringComparison.InvariantCultureIgnoreCase) : (string.Compare(PathUtilities.GetDirectoryName(x).Trim(new char[1] { '/' }), assetFolderPath.Trim(new char[1] { '/' }), ignoreCase: true) == 0)
				select x;
			menuPath = menuPath ?? "";
			menuPath = menuPath.TrimStart(new char[1] { '/' });
			HashSet<OdinMenuItem> result = new HashSet<OdinMenuItem>();
			foreach (string assetPath in assets)
			{
				UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, type);
				if (obj == null)
				{
					continue;
				}
				string name = Path.GetFileNameWithoutExtension(assetPath);
				string path = menuPath;
				if (!flattenSubDirectories)
				{
					string subPath = PathUtilities.GetDirectoryName(assetPath).TrimEnd(new char[1] { '/' }) + "/";
					subPath = subPath.Substring(assetFolderPath.Length);
					if (subPath.Length != 0)
					{
						path = path.Trim(new char[1] { '/' }) + "/" + subPath;
					}
				}
				path = path.Trim(new char[1] { '/' }) + "/" + name;
				SplitMenuPath(path, out path, out var menu);
				tree.AddMenuItemAtPath(result, path, new OdinMenuItem(tree, menu, obj));
			}
			return result;
		}

		/// <summary>
		/// Adds all assets at the specified path. Each asset found gets its own menu item inside the specified menu item path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuPath">The menu item path.</param>
		/// <param name="assetFolderPath">The asset folder path.</param>
		/// <param name="includeSubDirectories">Whether to search for assets in subdirectories as well.</param>
		/// <param name="flattenSubDirectories">If true, sub-directories in the assetFolderPath will no longer get its own sub-menu item at the specified menu item path.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddAllAssetsAtPath(this OdinMenuTree tree, string menuPath, string assetFolderPath, bool includeSubDirectories = false, bool flattenSubDirectories = false)
		{
			return tree.AddAllAssetsAtPath(menuPath, assetFolderPath, typeof(UnityEngine.Object), includeSubDirectories, flattenSubDirectories);
		}

		/// <summary>
		/// Adds the asset at the specified menu item path and returns all menu items created in order to end up at the specified menu path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuItemPath">The menu item path.</param>
		/// <param name="assetPath">The asset path.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddAssetAtPath(this OdinMenuTree tree, string menuItemPath, string assetPath)
		{
			return tree.AddAssetAtPath(menuItemPath, assetPath, typeof(UnityEngine.Object));
		}

		/// <summary>
		/// Adds the asset at the specified menu item path and returns all menu items created in order to end up at the specified menu path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuItemPath">The menu item path.</param>
		/// <param name="assetPath">The asset path.</param>
		/// <param name="type">The type.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddAssetAtPath(this OdinMenuTree tree, string menuItemPath, string assetPath, Type type)
		{
			string assetPathLower = assetPath.ToLower();
			if (!assetPathLower.StartsWith("assets/") && !assetPathLower.StartsWith("packages/"))
			{
				assetPath = "Assets/" + assetPath;
			}
			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, type);
			SplitMenuPath(menuItemPath, out menuItemPath, out var name);
			return tree.AddMenuItemAtPath(menuItemPath, new OdinMenuItem(tree, name, obj));
		}

		/// <summary>
		/// Sorts the entire tree of menu items recursively by name with respects to numbers.
		/// </summary>
		public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this OdinMenuTree tree, bool placeFoldersFirst = true)
		{
			IEnumerable<OdinMenuItem> result = tree.EnumerateTree(includeRootNode: true).SortMenuItemsByName(placeFoldersFirst);
			tree.MarkDirty();
			return result;
		}

		public static void AssignIconToEmptyItems(this OdinMenuTree tree, SdfIconType icon, Color? iconColor = null)
		{
			for (int i = 0; i < tree.MenuItems.Count; i++)
			{
				AssignIconToEmptyItemRecursively(tree.MenuItems[i], icon, iconColor);
			}
		}

		private static void AssignIconToEmptyItemRecursively(OdinMenuItem item, SdfIconType icon, Color? iconColor)
		{
			if (item.Value == null)
			{
				item.SdfIcon = icon;
				item.SdfIconColor = iconColor;
			}
			for (int i = 0; i < item.ChildMenuItems.Count; i++)
			{
				AssignIconToEmptyItemRecursively(item.ChildMenuItems[i], icon, iconColor);
			}
		}

		public static void AssignIconToItemsWithNone(this OdinMenuTree tree, SdfIconType icon, Color? iconColor = null)
		{
			for (int i = 0; i < tree.MenuItems.Count; i++)
			{
				AssignIconToItemWithNoneRecursive(tree.MenuItems[i], icon, iconColor);
			}
		}

		private static void AssignIconToItemWithNoneRecursive(OdinMenuItem item, SdfIconType icon, Color? iconColor)
		{
			if (item.SdfIcon == SdfIconType.None && item.Icon == null)
			{
				item.SdfIcon = icon;
				item.SdfIconColor = iconColor;
			}
			for (int i = 0; i < item.ChildMenuItems.Count; i++)
			{
				AssignIconToItemWithNoneRecursive(item.ChildMenuItems[i], icon, iconColor);
			}
		}

		public static void CollapseEmptyItems(this OdinMenuTree tree)
		{
			for (int i = 0; i < tree.MenuItems.Count; i++)
			{
				OdinMenuItem item = tree.MenuItems[i];
				if (item.ChildMenuItems.Count <= 1 || item.Value != null)
				{
					OdinMenuItem newItem = GetItemWithChildren(item);
					if (newItem != null)
					{
						newItem.Name = newItem.GetFullPath();
						tree.MenuItems[i] = newItem;
					}
				}
			}
		}

		private static OdinMenuItem GetItemWithChildren(OdinMenuItem item)
		{
			if (item.ChildMenuItems.Count > 1 || item.Value != null)
			{
				if (item.Value != null && item.Parent != null)
				{
					return item.Parent;
				}
				return item;
			}
			OdinMenuItem result = null;
			for (int i = 0; i < item.ChildMenuItems.Count; i++)
			{
				result = GetItemWithChildren(item.ChildMenuItems[i]);
			}
			return result;
		}

		/// <summary>
		/// Sorts the collection of menu items recursively by name with respects to numbers. This is a stable sort, meaning that equivalently ordered items will remain in the same order as they start.
		/// </summary>
		public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this IEnumerable<OdinMenuItem> menuItems, bool placeFoldersFirst = true, bool ignoreLeadingZeroes = true, bool ignoreWhiteSpace = true, bool ignoreCase = false)
		{
			OdinMenuItemComparer comparer = new OdinMenuItemComparer();
			comparer.PlaceFoldersFirst = placeFoldersFirst;
			comparer.IgnoreLeadingZeroes = ignoreLeadingZeroes;
			comparer.IgnoreWhiteSpace = ignoreWhiteSpace;
			comparer.IgnoreCase = ignoreCase;
			return SortMenuItemsByName(menuItems, comparer);
		}

		/// <summary>
		/// Sorts the collection of menu items recursively using a given custom comparison. This is a stable sort, meaning that equivalently ordered items will remain in the same order as they start.
		/// </summary>
		public static IEnumerable<OdinMenuItem> SortMenuItemsByName(this IEnumerable<OdinMenuItem> menuItems, Comparison<OdinMenuItem> comparison)
		{
			if (comparison == null)
			{
				throw new ArgumentNullException("comparison");
			}
			OdinMenuItemComparer comparer = new OdinMenuItemComparer(comparison);
			return SortMenuItemsByName(menuItems, comparer);
		}

		private static IEnumerable<OdinMenuItem> SortMenuItemsByName(IEnumerable<OdinMenuItem> menuItems, IComparer<OdinMenuItem> comparer)
		{
			OdinMenuItem first = null;
			foreach (OdinMenuItem menuItem in menuItems)
			{
				if (first == null)
				{
					first = menuItem;
				}
				List<OdinMenuItem> newChildMenuItems = menuItem.ChildMenuItems.OrderBy((OdinMenuItem item) => item, comparer).ToList();
				menuItem.SetChildMenuItems(newChildMenuItems);
			}
			if (first != null && first.MenuTree != null)
			{
				first.MenuTree.MarkDirty();
			}
			return menuItems;
		}

		/// <summary>
		/// Adds the specified object at the specified menu item path and returns all menu items created in order to end up at the specified menu path.
		/// </summary>
		/// <param name="tree">The tree.</param>
		/// <param name="menuPath">The menu path.</param>
		/// <param name="instance">The object instance.</param>
		/// <param name="forceShowOdinSerializedMembers">Set this to true if you want Odin serialzied members such as dictionaries and generics to be shown as well.</param>
		/// <returns>Returns all menu items created in order to add the menu item at the specified menu item path.</returns>
		public static IEnumerable<OdinMenuItem> AddObjectAtPath(this OdinMenuTree tree, string menuPath, object instance, bool forceShowOdinSerializedMembers = false)
		{
			SplitMenuPath(menuPath, out menuPath, out var name);
			if (forceShowOdinSerializedMembers && !(instance as UnityEngine.Object))
			{
				return tree.AddMenuItemAtPath(menuPath, new OdinMenuItem(tree, name, new SerializedValueWrapper(instance)));
			}
			return tree.AddMenuItemAtPath(menuPath, new OdinMenuItem(tree, name, instance));
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection with the specified ObjectInstanceType.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons<T>(this IEnumerable<OdinMenuItem> menuItems, Func<T, Texture> getIcon)
		{
			foreach (OdinMenuItem item in menuItems)
			{
				if (item.Value != null && item.Value is T)
				{
					OdinMenuItem localItem = item;
					localItem.IconGetter = () => getIcon((T)localItem.Value);
				}
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection with the specified ObjectInstanceType.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons<T>(this IEnumerable<OdinMenuItem> menuItems, Func<T, Sprite> getIcon)
		{
			foreach (OdinMenuItem item in menuItems)
			{
				if (item.Value != null && item.Value is T)
				{
					OdinMenuItem localItem = item;
					localItem.IconGetter = () => AssetPreview.GetAssetPreview(getIcon((T)localItem.Value));
				}
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Func<OdinMenuItem, Texture> getIcon)
		{
			foreach (OdinMenuItem item in menuItems)
			{
				OdinMenuItem localItem = item;
				localItem.IconGetter = () => getIcon(localItem);
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Func<OdinMenuItem, Sprite> getIcon)
		{
			foreach (OdinMenuItem item in menuItems)
			{
				OdinMenuItem localItem = item;
				localItem.IconGetter = () => AssetPreview.GetAssetPreview(getIcon(localItem));
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to the last menu item in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Sprite icon)
		{
			menuItems.AddIcon(AssetPreview.GetAssetPreview(icon));
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to the last menu item in the collection.
		/// </summary>
		public static OdinMenuItem AddIcon(this OdinMenuItem menuItem, SdfIconType icon)
		{
			menuItem.SdfIcon = icon;
			return menuItem;
		}

		/// <summary>
		/// Assigns the specified icon to the last menu item in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, EditorIcon icon)
		{
			menuItems.AddIcon(icon.Highlighted, icon.Raw);
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to the last menu item in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Texture icon)
		{
			OdinMenuItem last = menuItems.LastOrDefault();
			if (last != null)
			{
				last.Icon = icon;
				last.IconSelected = icon;
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to the last menu item in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcon(this IEnumerable<OdinMenuItem> menuItems, Texture icon, Texture iconSelected)
		{
			OdinMenuItem last = menuItems.LastOrDefault();
			if (last != null)
			{
				last.Icon = icon;
				last.IconSelected = iconSelected;
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, EditorIcon icon)
		{
			foreach (OdinMenuItem item in menuItems)
			{
				item.Icon = icon.Highlighted;
				item.IconSelected = icon.Raw;
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Texture icon)
		{
			foreach (OdinMenuItem item in menuItems)
			{
				item.Icon = icon;
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the specified icon to all menu items in the collection.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddIcons(this IEnumerable<OdinMenuItem> menuItems, Texture icon, Texture iconSelected)
		{
			foreach (OdinMenuItem item in menuItems)
			{
				item.Icon = icon;
				item.IconSelected = iconSelected;
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the asset mini thumbnail as an icon to all menu items in the collection. If the menu items object is null then a Unity folder icon is assigned.
		/// </summary>
		public static IEnumerable<OdinMenuItem> AddThumbnailIcons(this IEnumerable<OdinMenuItem> menuItems, bool preferAssetPreviewAsIcon = false)
		{
			foreach (OdinMenuItem item in menuItems)
			{
				item.AddThumbnailIcon(preferAssetPreviewAsIcon);
			}
			return menuItems;
		}

		/// <summary>
		/// Assigns the asset mini thumbnail as an icon to all menu items in the collection. If the menu items object is null then a Unity folder icon is assigned.
		/// </summary>
		public static OdinMenuItem AddThumbnailIcon(this OdinMenuItem item, bool preferAssetPreviewAsIcon)
		{
			object instance = item.Value;
			UnityEngine.Object unityObject = instance as UnityEngine.Object;
			if ((bool)unityObject)
			{
				if (preferAssetPreviewAsIcon)
				{
					item.IconGetter = () => GUIHelper.GetAssetThumbnail(unityObject, unityObject.GetType(), preferAssetPreviewAsIcon);
				}
				else
				{
					item.Icon = GUIHelper.GetAssetThumbnail(unityObject, unityObject.GetType(), preferAssetPreviewAsIcon);
				}
				return item;
			}
			Type type = instance as Type;
			if (type != null)
			{
				if (preferAssetPreviewAsIcon)
				{
					item.IconGetter = () => GUIHelper.GetAssetThumbnail(null, type, preferAssetPreviewAsIcon);
				}
				else
				{
					item.Icon = GUIHelper.GetAssetThumbnail(null, type, preferAssetPreviewAsIcon);
				}
				return item;
			}
			if (instance is string assetPath && assetPath != null)
			{
				if (File.Exists(assetPath))
				{
					item.Icon = InternalEditorUtility.GetIconForFile(assetPath);
				}
				else if (Directory.Exists(assetPath))
				{
					item.Icon = EditorIcons.UnityFolderIcon;
				}
			}
			return item;
		}

		internal static void SplitMenuPath(string menuPath, out string path, out string name)
		{
			menuPath = menuPath.Trim(new char[1] { '/' });
			int i = menuPath.LastIndexOf('/');
			if (i == -1)
			{
				path = "";
				name = menuPath;
			}
			else
			{
				path = menuPath.Substring(0, i);
				name = menuPath.Substring(i + 1);
			}
		}

		private static bool ReplaceDollarSignWithAssetName(ref string menuItem, string name)
		{
			if (menuItem == null)
			{
				return false;
			}
			if (menuItem == "$")
			{
				menuItem = name;
			}
			if (menuItem.StartsWith("$/"))
			{
				menuItem = name + menuItem.Substring(2);
			}
			if (menuItem.EndsWith("/$"))
			{
				menuItem = menuItem.Substring(0, menuItem.Length - 1) + name;
			}
			if (menuItem.Contains("/$/"))
			{
				menuItem = menuItem.Replace("/$/", "/" + name + "/");
				return true;
			}
			return false;
		}
	}
}
