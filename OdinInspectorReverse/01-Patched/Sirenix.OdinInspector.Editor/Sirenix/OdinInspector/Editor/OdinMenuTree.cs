using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// OdinMenuTree provides a tree of <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />s, and helps with selection, inserting menu items into the tree, and can handle keyboard navigation for you.
	/// </summary>
	/// <example>
	/// <code>
	/// OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
	/// {
	///     { "Home",                           this,                           EditorIcons.House       },
	///     { "Odin Settings",                  null,                           SdfIconType.GearFill    },
	///     { "Odin Settings/Color Palettes",   ColorPaletteManager.Instance,   EditorIcons.EyeDropper  },
	///     { "Odin Settings/AOT Generation",   AOTGenerationConfig.Instance,   EditorIcons.SmartPhone  },
	///     { "Camera current",                 Camera.current                                          },
	///     { "Some Class",                     this.someData                                           }
	/// };
	///
	/// tree.AddAllAssetsAtPath("Some Menu Item", "Some Asset Path", typeof(ScriptableObject), true)
	///     .AddThumbnailIcons();
	///
	/// tree.AddAssetAtPath("Some Second Menu Item", "SomeAssetPath/SomeAssetFile.asset");
	///
	/// var customMenuItem = new OdinMenuItem(tree, "Menu Style", tree.DefaultMenuStyle);
	/// tree.MenuItems.Insert(2, customMenuItem);
	///
	/// tree.Add("Menu/Items/Are/Created/As/Needed", new GUIContent());
	/// tree.Add("Menu/Items/Are/Created", new GUIContent("And can be overridden"));
	/// </code>
	/// OdinMenuTrees are typically used with <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />s but is made to work perfectly fine on its own for other use cases.
	/// OdinMenuItems can be inherited and and customized to fit your needs.
	/// <code>
	/// // Draw stuff
	/// someTree.DrawMenuTree();
	/// // Draw stuff
	/// someTree.HandleKeybaordMenuNavigation();
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuStyle" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeExtensions" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	public class OdinMenuTree : IEnumerable
	{
		private static bool preventAutoFocus;

		/// <summary>
		/// Gets the currently active menu tree.
		/// </summary>
		public static OdinMenuTree ActiveMenuTree;

		private static HashSet<OdinMenuItem> cachedHashList = new HashSet<OdinMenuItem>();

		private readonly OdinMenuItem root;

		private readonly OdinMenuTreeSelection selection;

		private OdinMenuTreeDrawingConfig defaultConfig;

		private bool regainSearchFieldFocus;

		private bool hadSearchFieldFocus;

		private Rect outerScrollViewRect;

		private int hideScrollbarsWhileContentIsExpanding;

		private Rect innerScrollViewRect;

		private bool isFirstFrame = true;

		private int forceRegainFocusCounter;

		private bool requestRepaint;

		private GUIFrameCounter frameCounter = new GUIFrameCounter();

		private bool hasRepaintedCurrentSearchResult = true;

		private bool scollToCenter;

		private OdinMenuItem scrollToWhenReady;

		private bool isDirty;

		private bool updateSearchResults;

		private bool regainFocusWhenWindowFocus;

		private bool currWindowHasFocus;

		private bool wasMouseOverMenuTree;

		private SearchField searchField = new SearchField();

		internal OdinGUIScrollView ScrollView;

		private bool layoutRequiresUpdate = true;

		private int visibleMenuItemCount;

		internal static Rect VisibleRect;

		internal static Event CurrentEvent;

		internal static EventType CurrentEventType;

		public List<OdinMenuItem> FlatMenuTree = new List<OdinMenuItem>();

		private bool isFirstGuiFrame;

		internal static float CurrentEditorTimeHelperDeltaTime;

		internal OdinMenuItem Root => root;

		/// <summary>
		/// Gets the selection.
		/// </summary>
		public OdinMenuTreeSelection Selection => selection;

		/// <summary>
		/// Gets the root menu items.
		/// </summary>
		public List<OdinMenuItem> MenuItems => root.ChildMenuItems;

		/// <summary>
		/// Gets the root menu item.
		/// </summary>
		public OdinMenuItem RootMenuItem => root;

		/// <summary>
		/// If true, all indent levels will be ignored, and all menu items with IsVisible == true will be drawn.
		/// </summary>
		public bool DrawInSearchMode { get; private set; }

		/// <summary>
		/// Gets or sets the default menu item style from Config.DefaultStyle.
		/// </summary>
		public OdinMenuStyle DefaultMenuStyle
		{
			get
			{
				return Config.DefaultMenuStyle;
			}
			set
			{
				Config.DefaultMenuStyle = value;
			}
		}

		/// <summary>
		/// Gets or sets the default drawing configuration.
		/// </summary>
		public OdinMenuTreeDrawingConfig Config
		{
			get
			{
				defaultConfig = defaultConfig ?? new OdinMenuTreeDrawingConfig
				{
					DrawScrollView = true,
					DrawSearchToolbar = false,
					AutoHandleKeyboardNavigation = false
				};
				return defaultConfig;
			}
			set
			{
				defaultConfig = value;
			}
		}

		/// <summary>
		/// Adds a menu item with the specified object instance at the the specified path.
		/// </summary>
		/// <returns>Returns all menu items created in order to add the menu item at the specified path.</returns>
		public IEnumerable<OdinMenuItem> Add(string path, object instance)
		{
			return this.AddObjectAtPath(path, instance);
		}

		/// <summary>
		/// Adds a menu item with the specified object instance and icon at the the specified path.
		/// </summary>
		/// <returns>Returns all menu items created in order to add the menu item at the specified path.</returns>
		public IEnumerable<OdinMenuItem> Add(string path, object instance, Texture icon)
		{
			return this.AddObjectAtPath(path, instance).AddIcon(icon);
		}

		/// <summary>
		/// Adds a menu item with the specified object instance and icon at the the specified path.
		/// </summary>
		/// <returns>Returns all menu items created in order to add the menu item at the specified path.</returns>
		public IEnumerable<OdinMenuItem> Add(string path, object instance, SdfIconType icon)
		{
			IEnumerable<OdinMenuItem> addedMenuItems = this.AddObjectAtPath(path, instance);
			addedMenuItems.LastOrDefault()?.AddIcon(icon);
			return addedMenuItems;
		}

		/// <summary>
		/// Adds a menu item with the specified object instance and icon at the the specified path.
		/// </summary>
		/// <returns>Returns all menu items created in order to add the menu item at the specified path.</returns>
		public IEnumerable<OdinMenuItem> Add(string path, object instance, Sprite sprite)
		{
			return this.AddObjectAtPath(path, instance).AddIcon(AssetPreview.GetAssetPreview(sprite));
		}

		/// <summary>
		/// Adds a menu item with the specified object instance and icon at the the specified path.
		/// </summary>
		/// <returns>Returns all menu items created in order to add the menu item at the specified path.</returns>
		public IEnumerable<OdinMenuItem> Add(string path, object instance, EditorIcon icon)
		{
			return this.AddObjectAtPath(path, instance).AddIcon(icon);
		}

		/// <summary>
		/// Adds a collection of objects to the menu tree and returns all menu items created in random order.
		/// </summary>
		public IEnumerable<OdinMenuItem> AddRange<T>(IEnumerable<T> collection, Func<T, string> getPath)
		{
			if (collection == null)
			{
				return Enumerable.Empty<OdinMenuItem>();
			}
			cachedHashList.Clear();
			foreach (T item in collection)
			{
				cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item));
			}
			return cachedHashList;
		}

		/// <summary>
		/// Adds a collection of objects to the menu tree and returns all menu items created in random order.
		/// </summary>
		public IEnumerable<OdinMenuItem> AddRange<T>(IEnumerable<T> collection, Func<T, string> getPath, Func<T, Texture> getIcon)
		{
			if (collection == null)
			{
				return Enumerable.Empty<OdinMenuItem>();
			}
			cachedHashList.Clear();
			foreach (T item in collection)
			{
				if (getIcon != null)
				{
					cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item).AddIcon(getIcon(item)));
				}
				else
				{
					cachedHashList.AddRange(this.AddObjectAtPath(getPath(item), item));
				}
			}
			return cachedHashList;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" /> class.
		/// </summary>
		public OdinMenuTree()
			: this(supportsMultiSelect: false, new OdinMenuStyle())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" /> class.
		/// </summary>
		/// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
		public OdinMenuTree(bool supportsMultiSelect)
			: this(supportsMultiSelect, new OdinMenuStyle())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" /> class.
		/// </summary>
		/// <param name="supportsMultiSelect">if set to <c>true</c> [supports multi select].</param>
		/// <param name="defaultMenuStyle">The default menu item style.</param>
		public OdinMenuTree(bool supportsMultiSelect, OdinMenuStyle defaultMenuStyle)
		{
			DefaultMenuStyle = defaultMenuStyle;
			selection = new OdinMenuTreeSelection(supportsMultiSelect);
			root = new OdinMenuItem(this, "root", null);
			SetupAutoScroll();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" /> class.
		/// </summary>
		public OdinMenuTree(bool supportsMultiSelect, OdinMenuTreeDrawingConfig config)
		{
			Config = config;
			selection = new OdinMenuTreeSelection(supportsMultiSelect);
			root = new OdinMenuItem(this, "root", null);
			SetupAutoScroll();
		}

		/// <summary>
		/// Sets the focus to the <see cref="F:Sirenix.OdinInspector.Editor.OdinMenuTree.searchField" />.
		/// </summary>
		public void FocusSearchField()
		{
			searchField?.Focus();
		}

		private void SetupAutoScroll()
		{
			selection.SelectionChanged += delegate(SelectionChangedType x)
			{
				if (Config.AutoScrollOnSelectionChanged && x == SelectionChangedType.ItemAdded)
				{
					requestRepaint = true;
					GUIHelper.RequestRepaint();
					if (isFirstFrame)
					{
						ScrollToMenuItem(selection.LastOrDefault(), centerMenuItem: true);
					}
					else
					{
						ScrollToMenuItem(selection.LastOrDefault());
					}
				}
			};
		}

		/// <summary>
		/// Scrolls to the specified menu item.
		/// </summary>
		public void ScrollToMenuItem(OdinMenuItem menuItem, bool centerMenuItem = false)
		{
			if (menuItem == null)
			{
				return;
			}
			scollToCenter = centerMenuItem;
			scrollToWhenReady = menuItem;
			if (!menuItem._IsVisible())
			{
				foreach (OdinMenuItem item in menuItem.GetParentMenuItemsRecursive(includeSelf: false))
				{
					item.Toggled = true;
				}
				return;
			}
			foreach (OdinMenuItem item2 in menuItem.GetParentMenuItemsRecursive(includeSelf: false))
			{
				item2.Toggled = true;
			}
			if (outerScrollViewRect.height != 0f && !(menuItem.Rect.height <= 0.01f) && Event.current != null && Event.current.type == EventType.Repaint)
			{
				OdinMenuTreeDrawingConfig config = Config;
				Rect rect = menuItem.Rect;
				float a;
				float b;
				if (centerMenuItem)
				{
					Rect r = outerScrollViewRect.AlignCenterY(rect.height);
					a = rect.yMin - (innerScrollViewRect.y + config.ScrollPos.y - r.y);
					b = rect.yMax - r.height + innerScrollViewRect.y - (config.ScrollPos.y + r.y);
				}
				else
				{
					Rect viewRect = outerScrollViewRect;
					viewRect.y = 0f;
					a = rect.yMin - (innerScrollViewRect.y + config.ScrollPos.y) - 1f;
					b = rect.yMax - outerScrollViewRect.height + innerScrollViewRect.y - config.ScrollPos.y;
					a -= rect.height;
					b += rect.height;
				}
				if (a < 0f)
				{
					config.ScrollPos.y += a;
				}
				if (b > 0f)
				{
					config.ScrollPos.y += b;
				}
				if (frameCounter.FrameCount > 6)
				{
					scrollToWhenReady = null;
				}
				else
				{
					GUIHelper.RequestRepaint();
				}
			}
		}

		/// <summary>
		/// Enumerates the tree with a DFS.
		/// </summary>
		/// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
		public IEnumerable<OdinMenuItem> EnumerateTree(bool includeRootNode = false)
		{
			return root.GetChildMenuItemsRecursive(includeRootNode);
		}

		/// <summary>
		/// Enumerates the tree with a DFS.
		/// </summary>
		/// <param name="predicate">The predicate.</param>
		/// <param name="includeRootNode">if set to <c>true</c> then the invisible root menu item is included.</param>
		public IEnumerable<OdinMenuItem> EnumerateTree(Func<OdinMenuItem, bool> predicate, bool includeRootNode)
		{
			return root.GetChildMenuItemsRecursive(includeRootNode).Where(predicate);
		}

		/// <summary>
		/// Enumerates the tree with a DFS.
		/// </summary>
		public void EnumerateTree(Action<OdinMenuItem> action)
		{
			root.GetChildMenuItemsRecursive(includeSelf: false).ForEach(action);
		}

		/// <summary>
		/// Draws the menu tree recursively.
		/// </summary>
		public void DrawMenuTree()
		{
			CurrentEditorTimeHelperDeltaTime = GUITimeHelper.LayoutDeltaTime;
			frameCounter.Update();
			OdinMenuTreeDrawingConfig config = Config;
			if (requestRepaint)
			{
				GUIHelper.RequestRepaint();
				requestRepaint = false;
			}
			if (config.DrawSearchToolbar)
			{
				DrawSearchToolbar();
			}
			if (Event.current.delta != default(Vector2))
			{
				bool isMouseOverMenuTree = outerScrollViewRect.Contains(Event.current.mousePosition);
				if (wasMouseOverMenuTree || isMouseOverMenuTree)
				{
					GUIHelper.RequestRepaint();
				}
				wasMouseOverMenuTree = isMouseOverMenuTree;
			}
			Rect outerRect = EditorGUILayout.BeginVertical();
			HandleActiveMenuTreeState(outerRect);
			if (config.DrawScrollView)
			{
				if (Event.current.type == EventType.Repaint)
				{
					outerScrollViewRect = outerRect;
				}
				if (hideScrollbarsWhileContentIsExpanding > 0)
				{
					config.ScrollPos = EditorGUILayout.BeginScrollView(config.ScrollPos, GUIStyle.none, GUIStyle.none, GUILayoutOptions.ExpandHeight(expand: false));
				}
				else
				{
					config.ScrollPos = EditorGUILayout.BeginScrollView(config.ScrollPos, GUILayoutOptions.ExpandHeight(expand: false));
				}
				Rect size = EditorGUILayout.BeginVertical();
				if (innerScrollViewRect.height == 0f || Event.current.type == EventType.Repaint)
				{
					float chancedSizeDiff = Mathf.Abs(innerScrollViewRect.height - size.height);
					float boxDiff = Mathf.Abs(innerScrollViewRect.height - outerScrollViewRect.height);
					if (!(innerScrollViewRect.height - 40f > outerScrollViewRect.height) && chancedSizeDiff > 0f)
					{
						hideScrollbarsWhileContentIsExpanding = 5;
						GUIHelper.RequestRepaint();
					}
					else if (Mathf.Abs(boxDiff) < 1f)
					{
						hideScrollbarsWhileContentIsExpanding = 5;
					}
					else
					{
						hideScrollbarsWhileContentIsExpanding--;
						if (hideScrollbarsWhileContentIsExpanding < 0)
						{
							hideScrollbarsWhileContentIsExpanding = 0;
						}
						else
						{
							GUIHelper.RequestRepaint();
						}
					}
					innerScrollViewRect = size;
				}
				GUILayout.Space(-1f);
			}
			if (isDirty && Event.current.type == EventType.Layout)
			{
				UpdateMenuTree();
				isDirty = false;
			}
			VisibleRect = GUIClipInfo.VisibleRect.Expand(300f);
			CurrentEvent = Event.current;
			CurrentEventType = CurrentEvent.type;
			List<OdinMenuItem> tree = (DrawInSearchMode ? FlatMenuTree : MenuItems);
			int count = tree.Count;
			if (config.EXPERIMENTAL_INTERNAL_SparseFixedLayouting)
			{
				if (ScrollView == null)
				{
					ScrollView = new OdinGUIScrollView((MenuItems.Count > 0) ? MenuItems.Count : 16);
				}
				if (layoutRequiresUpdate)
				{
					visibleMenuItemCount = 0;
					if (DrawInSearchMode)
					{
						visibleMenuItemCount = tree.Count;
					}
					else
					{
						foreach (OdinMenuItem item in tree)
						{
							visibleMenuItemCount += item.CountVisibleRecursively();
						}
					}
				}
				int height = visibleMenuItemCount * Config.DefaultMenuStyle.Height;
				Rect rect = GUILayoutUtility.GetRect(0f, height);
				if (layoutRequiresUpdate)
				{
					ScrollView.SetBounds(rect);
					ScrollView.BeginAllocations();
					foreach (OdinMenuItem item2 in tree)
					{
						item2.AllocateRectRecursivelyForScrollView();
					}
					ScrollView.EndAllocations();
					layoutRequiresUpdate = false;
				}
				else
				{
					ScrollView.SetBoundsForCurrentAllocations(rect);
				}
				ScrollView.Position = Config.ScrollPos;
				OdinGUIScrollView.VisibleItems visibleItems = ScrollView.GetVisibleItems();
				for (int i = 0; i < visibleItems.Length; i++)
				{
					OdinMenuItem item3 = visibleItems.GetAssociatedData<OdinMenuItem>(i);
					item3.EXPERIMENTAL_DontAllocateNewRect = true;
					item3.rect = visibleItems.GetRect(i);
					item3.DrawMenuItem((int)visibleItems.GetIndentation(i));
				}
			}
			else if (config.EXPERIMENTAL_INTERNAL_DrawFlatTreeFastNoLayout)
			{
				int itemHeight = DefaultMenuStyle.Height;
				int height2 = count * itemHeight;
				Rect rect2 = GUILayoutUtility.GetRect(0f, height2);
				rect2.height = itemHeight;
				for (int j = 0; j < count; j++)
				{
					OdinMenuItem item4 = tree[j];
					item4.EXPERIMENTAL_DontAllocateNewRect = true;
					item4.rect = rect2;
					item4.DrawMenuItem(0);
					rect2.y += itemHeight;
				}
			}
			else if (DrawInSearchMode)
			{
				for (int k = 0; k < count; k++)
				{
					tree[k].DrawMenuItem(0);
				}
			}
			else
			{
				for (int l = 0; l < count; l++)
				{
					tree[l].DrawMenuItems(0);
				}
			}
			if (config.DrawScrollView)
			{
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndScrollView();
			}
			EditorGUILayout.EndVertical();
			if (config.AutoHandleKeyboardNavigation)
			{
				HandleKeyboardMenuNavigation();
			}
			if (scrollToWhenReady != null)
			{
				ScrollToMenuItem(scrollToWhenReady, scollToCenter);
			}
			if (Event.current.type == EventType.Repaint)
			{
				isFirstFrame = false;
			}
		}

		private void HandleActiveMenuTreeState(Rect outerRect)
		{
			if (Event.current.type == EventType.Repaint)
			{
				if (currWindowHasFocus != GUIHelper.CurrentWindowHasFocus)
				{
					currWindowHasFocus = GUIHelper.CurrentWindowHasFocus;
					if (currWindowHasFocus && regainFocusWhenWindowFocus)
					{
						if (!preventAutoFocus)
						{
							ActiveMenuTree = this;
						}
						regainFocusWhenWindowFocus = false;
					}
				}
				if (!currWindowHasFocus && ActiveMenuTree == this)
				{
					ActiveMenuTree = null;
				}
				if (currWindowHasFocus)
				{
					regainFocusWhenWindowFocus = ActiveMenuTree == this;
				}
				if (currWindowHasFocus && ActiveMenuTree == null)
				{
					ActiveMenuTree = this;
				}
			}
			MenuTreeActivationZone(outerRect);
		}

		internal void MenuTreeActivationZone(Rect rect)
		{
			if (ActiveMenuTree != this && Event.current.rawType == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && GUIHelper.CurrentWindowHasFocus)
			{
				regainSearchFieldFocus = true;
				preventAutoFocus = true;
				ActiveMenuTree = this;
				UnityEditorEventUtility.EditorApplication_delayCall += delegate
				{
					preventAutoFocus = false;
				};
				GUIHelper.RequestRepaint();
			}
		}

		/// <summary>
		/// Marks the dirty. This will cause a tree.UpdateTree() in the beginning of the next Layout frame.
		/// </summary>
		public void MarkDirty()
		{
			isDirty = true;
			updateSearchResults = true;
		}

		/// <summary>
		/// Indicates that the layout has changed and needs to be recomputed.
		/// This is used when <see cref="F:Sirenix.OdinInspector.Editor.OdinMenuTreeDrawingConfig.EXPERIMENTAL_INTERNAL_SparseFixedLayouting" /> is enabled.
		/// </summary>
		public void MarkLayoutChanged()
		{
			layoutRequiresUpdate = true;
		}

		/// <summary>
		/// Draws the search toolbar.
		/// </summary>
		public void DrawSearchToolbar(GUIStyle toolbarStyle = null)
		{
			OdinMenuTreeDrawingConfig config = Config;
			Rect searchFieldRect = GUILayoutUtility.GetRect(0f, config.SearchToolbarHeight, GUILayoutOptions.ExpandWidth());
			if (Event.current.type == EventType.Repaint)
			{
				(toolbarStyle ?? SirenixGUIStyles.ToolbarBackground).Draw(searchFieldRect, GUIContent.none, 0);
			}
			searchFieldRect = searchFieldRect.Padding(4f);
			searchFieldRect.yMax += 1f;
			EditorGUI.BeginChangeCheck();
			config.SearchTerm = DrawSearchField(searchFieldRect, config.SearchTerm, config.AutoFocusSearchBar);
			if ((EditorGUI.EndChangeCheck() || updateSearchResults) && hasRepaintedCurrentSearchResult)
			{
				layoutRequiresUpdate = true;
				updateSearchResults = false;
				hasRepaintedCurrentSearchResult = false;
				if (!string.IsNullOrEmpty(config.SearchTerm))
				{
					if (!DrawInSearchMode)
					{
						config.ScrollPos = default(Vector2);
					}
					DrawInSearchMode = true;
					if (config.SearchFunction != null)
					{
						FlatMenuTree.Clear();
						foreach (OdinMenuItem item in EnumerateTree())
						{
							if (config.SearchFunction(item))
							{
								FlatMenuTree.Add(item);
							}
						}
					}
					else
					{
						FlatMenuTree.Clear();
						FlatMenuTree.AddRange(from x in (from x in EnumerateTree()
								where x.Value != null
								select x).Select(delegate(OdinMenuItem x)
							{
								int score;
								bool include = FuzzySearch.Contains(Config.SearchTerm, x.SearchString, out score);
								return new
								{
									score = score,
									item = x,
									include = include
								};
							})
							where x.include
							orderby x.score descending
							select x.item);
					}
					root.UpdateFlatMenuItemNavigation();
				}
				else
				{
					DrawInSearchMode = false;
					FlatMenuTree.Clear();
					OdinMenuItem last = selection.LastOrDefault();
					UpdateMenuTree();
					Selection.SelectMany((OdinMenuItem x) => x.GetParentMenuItemsRecursive(includeSelf: false)).ForEach(delegate(OdinMenuItem x)
					{
						x.Toggled = true;
					});
					if (last != null)
					{
						ScrollToMenuItem(last);
					}
					root.UpdateFlatMenuItemNavigation();
				}
			}
			if (Event.current.type == EventType.Repaint)
			{
				hasRepaintedCurrentSearchResult = true;
			}
		}

		private string DrawSearchField(Rect rect, string searchTerm, bool autoFocus)
		{
			bool hasFocus = searchField.HasFocus();
			if (hadSearchFieldFocus != hasFocus)
			{
				ActiveMenuTree = this;
				hadSearchFieldFocus = hasFocus;
			}
			bool ignore = hasFocus && (Event.current.keyCode == KeyCode.DownArrow || Event.current.keyCode == KeyCode.UpArrow || Event.current.keyCode == KeyCode.LeftArrow || Event.current.keyCode == KeyCode.RightArrow || Event.current.keyCode == KeyCode.Return);
			if (ignore)
			{
				GUIHelper.PushEventType(Event.current.type);
			}
			if ((isFirstFrame || regainSearchFieldFocus) && autoFocus && ActiveMenuTree == this)
			{
				searchField.Focus();
			}
			isFirstFrame = false;
			rect.y -= 1f;
			searchTerm = searchField.Draw(rect, searchTerm);
			if (regainSearchFieldFocus && Event.current.type == EventType.Layout)
			{
				regainSearchFieldFocus = false;
			}
			if (ignore)
			{
				GUIHelper.PopEventType();
				if (ActiveMenuTree == this)
				{
					regainSearchFieldFocus = true;
				}
			}
			if (forceRegainFocusCounter < 20)
			{
				if (autoFocus && forceRegainFocusCounter < 4 && ActiveMenuTree == this)
				{
					regainSearchFieldFocus = true;
				}
				GUIHelper.RequestRepaint();
				GUIHelper.SafeHandleUtilityRepaint();
				if (Event.current.type == EventType.Repaint)
				{
					forceRegainFocusCounter++;
				}
			}
			return searchTerm;
		}

		/// <summary>
		/// Updates the menu tree. This method is usually called automatically when needed.
		/// </summary>
		public void UpdateMenuTree()
		{
			root.UpdateMenuTreeRecursive(isRoot: true);
			root.UpdateFlatMenuItemNavigation();
		}

		/// <summary>
		/// Handles the keyboard menu navigation. Call this at the end of your GUI scope, to prevent the menu tree from stealing input events from other text fields.
		/// </summary>
		/// <returns>Returns true, if anything was changed via the keyboard.</returns>
		[Obsolete("Use HandleKeyboardMenuNavigation instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool HandleKeybaordMenuNavigation()
		{
			return HandleKeyboardMenuNavigation();
		}

		/// <summary>
		/// Handles the keyboard menu navigation. Call this at the end of your GUI scope, to prevent the menu tree from stealing input events from other text fields.
		/// </summary>
		/// <returns>Returns true, if anything was changed via the keyboard.</returns>
		public bool HandleKeyboardMenuNavigation()
		{
			if (Event.current.type != EventType.KeyDown)
			{
				return false;
			}
			if (ActiveMenuTree != this)
			{
				return false;
			}
			GUIHelper.RequestRepaint();
			KeyCode keycode = Event.current.keyCode;
			if (Selection.Count == 0 || !Selection.Any((OdinMenuItem x) => x._IsVisible() && x.IsEnabled && x.IsSelectable))
			{
				IEnumerable<OdinMenuItem> enumerable;
				if (!DrawInSearchMode)
				{
					enumerable = from x in EnumerateTree()
						where x._IsVisible() && x.IsEnabled && x.IsSelectable
						select x;
				}
				else
				{
					IEnumerable<OdinMenuItem> flatMenuTree = FlatMenuTree;
					enumerable = flatMenuTree;
				}
				IEnumerable<OdinMenuItem> query = enumerable;
				OdinMenuItem next = null;
				switch (keycode)
				{
				case KeyCode.DownArrow:
					next = query.FirstOrDefault();
					break;
				case KeyCode.UpArrow:
					next = query.LastOrDefault();
					break;
				case KeyCode.LeftAlt:
					next = query.FirstOrDefault();
					break;
				case KeyCode.RightAlt:
					next = query.FirstOrDefault();
					break;
				}
				if (next != null)
				{
					next.Select();
					Event.current.Use();
					return true;
				}
			}
			else
			{
				if (keycode == KeyCode.LeftArrow && !DrawInSearchMode)
				{
					bool goUp = true;
					foreach (OdinMenuItem curr in Selection.ToList())
					{
						if (curr.Toggled && curr.ChildMenuItems.Any())
						{
							goUp = false;
							curr.Toggled = false;
						}
						if ((UnityShims.Misc.GetEventModifiers(Event.current) & 4) == 0)
						{
							continue;
						}
						goUp = false;
						foreach (OdinMenuItem item in curr.GetChildMenuItemsRecursive(includeSelf: false))
						{
							item.Toggled = curr.Toggled;
						}
					}
					if (goUp)
					{
						keycode = KeyCode.UpArrow;
					}
					Event.current.Use();
				}
				if (keycode == KeyCode.RightArrow && !DrawInSearchMode)
				{
					bool goDown = true;
					foreach (OdinMenuItem curr2 in Selection.ToList())
					{
						if (!curr2.Toggled && curr2.ChildMenuItems.Any())
						{
							curr2.Toggled = true;
							goDown = false;
						}
						if ((UnityShims.Misc.GetEventModifiers(Event.current) & 4) == 0)
						{
							continue;
						}
						goDown = false;
						foreach (OdinMenuItem item2 in curr2.GetChildMenuItemsRecursive(includeSelf: false))
						{
							item2.Toggled = curr2.Toggled;
						}
					}
					if (goDown)
					{
						keycode = KeyCode.DownArrow;
					}
					Event.current.Use();
				}
				if (keycode == KeyCode.UpArrow)
				{
					if ((UnityShims.Misc.GetEventModifiers(Event.current) & 1) != 0)
					{
						OdinMenuItem last = Selection.Last();
						OdinMenuItem prev = last.PrevSelectableMenuItem;
						if (prev != null)
						{
							if (prev.IsSelected)
							{
								last.Deselect();
							}
							else
							{
								prev.Select(addToSelection: true);
							}
							Event.current.Use();
							return true;
						}
					}
					else
					{
						OdinMenuItem prev2 = Selection.Last().PrevSelectableMenuItem;
						if (prev2 != null)
						{
							prev2.Select();
							Event.current.Use();
							return true;
						}
					}
				}
				if (keycode == KeyCode.DownArrow)
				{
					if ((UnityShims.Misc.GetEventModifiers(Event.current) & 1) != 0)
					{
						OdinMenuItem last2 = Selection.Last();
						OdinMenuItem next2 = last2.NextSelectableMenuItem;
						if (next2 != null)
						{
							if (next2.IsSelected)
							{
								last2.Deselect();
							}
							else
							{
								next2.Select(addToSelection: true);
							}
							Event.current.Use();
							return true;
						}
					}
					else
					{
						OdinMenuItem next3 = Selection.Last().NextSelectableMenuItem;
						if (next3 != null)
						{
							next3.Select();
							Event.current.Use();
							return true;
						}
					}
				}
				if (keycode == KeyCode.Return)
				{
					Selection.ConfirmSelection();
					Event.current.Use();
					return true;
				}
			}
			return false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return MenuItems.GetEnumerator();
		}
	}
}
