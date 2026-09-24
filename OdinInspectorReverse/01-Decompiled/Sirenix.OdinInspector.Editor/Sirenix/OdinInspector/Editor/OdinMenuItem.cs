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
	/// A menu item that represents one or more objects.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuStyle" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeExtensions" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	public class OdinMenuItem
	{
		private static bool previousMenuItemWasSelected;

		private List<OdinMenuItem> childMenuItems;

		private int flatTreeIndex;

		private Texture iconSelected;

		private Texture icon;

		private Func<Texture> iconGetter;

		private bool hasCustomIconGetter;

		private bool isInitialized;

		private LocalPersistentContext<bool> isToggledContext;

		private OdinMenuTree menuTree;

		private string prevName;

		private string name;

		private bool isVisible = true;

		private OdinMenuItem nextMenuItem;

		private OdinMenuItem nextMenuItemFlat;

		private OdinMenuItem parentMenuItem;

		private OdinMenuItem previousMenuItem;

		private OdinMenuItem previousMenuItemFlat;

		private OdinMenuStyle style;

		private Rect triangleRect;

		private Rect labelRect;

		private bool? nonCachedToggledState;

		private object value;

		internal Rect rect;

		internal bool EXPERIMENTAL_DontAllocateNewRect;

		public bool MenuItemIsBeingRendered;

		public SdfIconType SdfIcon;

		public Color? SdfIconColor;

		/// <summary>
		/// The default toggled state
		/// </summary>
		public bool DefaultToggledState;

		/// <summary>
		/// Occurs right after the menu item is done drawing, and right before mouse input is handles so you can take control of that.
		/// </summary>
		public Action<OdinMenuItem> OnDrawItem;

		/// <summary>
		/// Occurs when the user has right-clicked the menu item.
		/// </summary>
		public Action<OdinMenuItem> OnRightClick;

		private float t = -1f;

		/// <summary>
		/// Gets the child menu items.
		/// </summary>
		/// <value>
		/// The child menu items.
		/// </value>
		public virtual List<OdinMenuItem> ChildMenuItems => childMenuItems;

		/// <summary>
		/// Gets the index location of the menu item.
		/// </summary>
		public int FlatTreeIndex => flatTreeIndex;

		/// <summary>
		/// Gets or sets a value indicating whether the menu item is visible.
		/// Not that setting this to false will not hide its children as well. For that see use Toggled.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("OdinMenuItems no longer has this concept which was previously used for filtering search results. Instead search results are cached to seperate list in order to support sorting.")]
		public virtual bool IsVisible
		{
			get
			{
				return isVisible;
			}
			set
			{
				isVisible = value;
			}
		}

		/// <summary>
		/// Gets or sets the icon that is used when the menu item is not selected.
		/// </summary>
		public Texture Icon
		{
			get
			{
				return icon;
			}
			set
			{
				icon = value;
			}
		}

		/// <summary>
		/// Gets or sets the icon that is used when the menu item is selected.
		/// </summary>
		public Texture IconSelected
		{
			get
			{
				return iconSelected;
			}
			set
			{
				iconSelected = value;
			}
		}

		internal bool HasIconContent
		{
			get
			{
				if (!(icon != null) && !(iconSelected != null) && !hasCustomIconGetter)
				{
					return SdfIcon != SdfIconType.None;
				}
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is selected.
		/// </summary>
		public bool IsSelected => menuTree.Selection.Contains(this);

		/// <summary>
		/// Determines whether this instance is selectable.
		/// </summary>
		public bool IsSelectable { get; set; } = true;

		/// <summary>
		/// Determines whether this instance is enabled.
		/// </summary>
		public bool IsEnabled { get; set; } = true;

		/// <summary>
		/// Gets the menu tree instance.
		/// </summary>
		public OdinMenuTree MenuTree => menuTree;

		/// <summary>
		/// Gets or sets the raw menu item name.
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		/// <summary>
		/// Gets or sets the search string used when searching for menu items.
		/// </summary>
		public string SearchString { get; set; }

		/// <summary>
		/// Gets the next visual menu item.
		/// </summary>
		public OdinMenuItem NextVisualMenuItem
		{
			get
			{
				EnsureInitialized();
				if (MenuTree.DrawInSearchMode)
				{
					return nextMenuItemFlat;
				}
				if (ChildMenuItems.Count > 0 && nextMenuItem != null && !Toggled && _IsVisible())
				{
					return nextMenuItem;
				}
				return GetAllNextMenuItems().FirstOrDefault((OdinMenuItem x) => x._IsVisible());
			}
		}

		/// <summary>
		/// Gets the next selectable visual menu item.
		/// </summary>
		public OdinMenuItem NextSelectableMenuItem
		{
			get
			{
				OdinMenuItem next = NextVisualMenuItem;
				while (next != null && (!next.IsSelectable || !next.IsEnabled))
				{
					next = next.NextVisualMenuItem;
				}
				return next;
			}
		}

		/// <summary>
		/// Gets the parent menu item.
		/// </summary>
		public OdinMenuItem Parent
		{
			get
			{
				EnsureInitialized();
				return parentMenuItem;
			}
		}

		/// <summary>
		/// Gets the previous visual menu item.
		/// </summary>
		public OdinMenuItem PrevVisualMenuItem
		{
			get
			{
				EnsureInitialized();
				if (MenuTree.DrawInSearchMode)
				{
					return previousMenuItemFlat;
				}
				if (ChildMenuItems.Count > 0 && !Toggled && _IsVisible())
				{
					if (previousMenuItem != null)
					{
						if (previousMenuItem.ChildMenuItems.Count == 0 || !previousMenuItem.Toggled)
						{
							return previousMenuItem;
						}
					}
					else if (parentMenuItem != null)
					{
						return parentMenuItem;
					}
				}
				return GetAllPreviousMenuItems().FirstOrDefault((OdinMenuItem x) => x._IsVisible());
			}
		}

		/// <summary>
		/// Gets the previous selectable visual menu item.
		/// </summary>
		public OdinMenuItem PrevSelectableMenuItem
		{
			get
			{
				OdinMenuItem prev = PrevVisualMenuItem;
				while (prev != null && (!prev.IsSelectable || !prev.IsEnabled))
				{
					prev = prev.PrevVisualMenuItem;
				}
				return prev;
			}
		}

		/// <summary>
		/// Gets the drawn rect.
		/// </summary>
		public Rect Rect => rect;

		/// <summary>
		/// Gets the drawn label rect.
		/// </summary>
		public Rect LabelRect => labelRect;

		/// <summary>
		/// Gets or sets the style. If null is specified, then the menu trees DefaultMenuStyle is used.
		/// </summary>
		public OdinMenuStyle Style
		{
			get
			{
				if (style == null)
				{
					style = menuTree.DefaultMenuStyle;
				}
				return style;
			}
			set
			{
				style = value;
			}
		}

		/// <summary>
		/// Gets the first object of the <see cref="P:Sirenix.OdinInspector.Editor.OdinMenuItem.ObjectInstances" />
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value instead.", false)]
		public virtual object ObjectInstance
		{
			get
			{
				if (Value == null)
				{
					return null;
				}
				object val = ((Value is IList { Count: not 0 } objectInstances) ? objectInstances[0] : Value);
				if (val is Func<object> instanceFunc)
				{
					return instanceFunc();
				}
				return val;
			}
		}

		/// <summary>
		/// Gets the object instances the menu item represents
		/// </summary>
		[Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value as IEnumerable instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual IEnumerable<object> ObjectInstances
		{
			get
			{
				if (!(Value is IList { Count: not 0 } objectInstances))
				{
					yield break;
				}
				foreach (object item in objectInstances)
				{
					if (item == null)
					{
						yield return null;
					}
					object instance = item;
					if (instance is Func<object> instanceFunc)
					{
						yield return instanceFunc();
					}
					else
					{
						yield return instance;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the value the menu item represents.
		/// </summary>
		public object Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}

		/// <summary>
		/// Gets a nice menu item name. If the raw name value is null or a dollar sign, then the name is retrieved from the object itself via ToString().
		/// </summary>
		public virtual string SmartName
		{
			get
			{
				object val = value;
				if (Value is Func<object> func)
				{
					val = func();
				}
				if (name == null || name == "$")
				{
					if (val == null)
					{
						return "";
					}
					UnityEngine.Object unityObject = val as UnityEngine.Object;
					if ((bool)unityObject)
					{
						return unityObject.name.SplitPascalCase();
					}
					return val.ToString();
				}
				return name;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" /> is toggled / expanded. This value tries it best to be persistent.
		/// </summary>
		public virtual bool Toggled
		{
			get
			{
				if (childMenuItems.Count == 0)
				{
					return false;
				}
				if (menuTree.Config.UseCachedExpandedStates)
				{
					if (isToggledContext == null)
					{
						isToggledContext = LocalPersistentContext<bool>.Create(PersistentContext.Get("[OdinMenuItem]" + GetFullPath(), DefaultToggledState));
					}
					return isToggledContext.Value;
				}
				if (!nonCachedToggledState.HasValue)
				{
					nonCachedToggledState = DefaultToggledState;
				}
				return nonCachedToggledState.Value;
			}
			set
			{
				bool hasChanged;
				if (menuTree.Config.UseCachedExpandedStates)
				{
					if (isToggledContext == null)
					{
						isToggledContext = LocalPersistentContext<bool>.Create(PersistentContext.Get("[OdinMenuItem]" + GetFullPath(), DefaultToggledState));
					}
					hasChanged = isToggledContext.Value != value;
					isToggledContext.Value = value;
				}
				else
				{
					hasChanged = nonCachedToggledState != value;
					nonCachedToggledState = value;
				}
				if (menuTree.Config.EXPERIMENTAL_INTERNAL_SparseFixedLayouting && hasChanged)
				{
					menuTree.MarkLayoutChanged();
				}
			}
		}

		/// <summary>
		/// Gets or sets the icon getter.
		/// </summary>
		public Func<Texture> IconGetter
		{
			get
			{
				if (iconGetter == null)
				{
					iconGetter = delegate
					{
						if (!IsSelected)
						{
							return Icon;
						}
						return (!IconSelected) ? Icon : IconSelected;
					};
				}
				return iconGetter;
			}
			set
			{
				iconGetter = value;
				hasCustomIconGetter = value != null;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" /> class.
		/// </summary>
		/// <param name="tree">The Odin menu tree instance the menu item belongs to.</param>
		/// <param name="name">The name of the menu item.</param>
		/// <param name="value">The instance the value item represents.</param>
		public OdinMenuItem(OdinMenuTree tree, string name, object value)
		{
			if (tree == null)
			{
				throw new ArgumentNullException("tree");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			menuTree = tree;
			this.name = name;
			SearchString = name;
			Value = value;
			childMenuItems = new List<OdinMenuItem>();
		}

		/// <summary>
		/// Deselects this instance.
		/// </summary>
		public bool Deselect()
		{
			return menuTree.Selection.Remove(this);
		}

		/// <summary>
		/// Selects the specified add to selection.
		/// </summary>
		public void Select(bool addToSelection = false)
		{
			if (!addToSelection)
			{
				menuTree.Selection.Clear();
			}
			menuTree.Selection.Add(this);
		}

		/// <summary>
		/// Gets the child menu items recursive in a DFS.
		/// </summary>
		/// <param name="includeSelf">Whether to include it self in the collection.</param>
		public IEnumerable<OdinMenuItem> GetChildMenuItemsRecursive(bool includeSelf)
		{
			if (includeSelf)
			{
				yield return this;
			}
			foreach (OdinMenuItem item in ChildMenuItems.SelectMany((OdinMenuItem x) => x.GetChildMenuItemsRecursive(includeSelf: true)))
			{
				yield return item;
			}
		}

		/// <summary>
		/// Gets the child menu items recursive in a DFS.
		/// </summary>
		/// <param name="includeSelf">Whether to include it self in the collection.</param>
		/// <param name="includeRoot">Whether to include the root.</param>
		public IEnumerable<OdinMenuItem> GetParentMenuItemsRecursive(bool includeSelf, bool includeRoot = false)
		{
			if (includeSelf || (Parent == null && includeRoot))
			{
				yield return this;
			}
			if (Parent == null)
			{
				yield break;
			}
			foreach (OdinMenuItem item in Parent.GetParentMenuItemsRecursive(includeSelf: true, includeRoot))
			{
				yield return item;
			}
		}

		/// <summary>
		/// Gets the full menu item path.
		/// </summary>
		public string GetFullPath()
		{
			EnsureInitialized();
			OdinMenuItem parent = Parent;
			if (parent == null)
			{
				return SmartName;
			}
			return parent.GetFullPath() + "/" + SmartName;
		}

		/// <summary>
		/// Sets the object instance
		/// </summary>
		[Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value = obj instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetObjectInstance(object obj)
		{
			Value = obj;
		}

		/// <summary>
		/// Sets the object instances
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Instead of having ObjectInstance and ObjectInstances, OdinMenuItems now only holds single value. Use menuItem.Value = obj instead.", false)]
		public void SetObjectInstances(IList objects)
		{
			Value = objects;
		}

		/// <summary>
		/// Draws this menu item followed by all of its child menu items
		/// </summary>
		/// <param name="indentLevel">The indent level.</param>
		public virtual void DrawMenuItems(int indentLevel)
		{
			DrawMenuItem(indentLevel);
			List<OdinMenuItem> children = ChildMenuItems;
			int childCount = children.Count;
			if (childCount == 0)
			{
				return;
			}
			bool isVisible = Toggled;
			if (t < 0f)
			{
				t = (isVisible ? 1 : 0);
			}
			if (OdinMenuTree.CurrentEventType == EventType.Layout)
			{
				t = Mathf.MoveTowards(t, isVisible ? 1 : 0, OdinMenuTree.CurrentEditorTimeHelperDeltaTime * (1f / SirenixEditorGUI.DefaultFadeGroupDuration));
			}
			if (SirenixEditorGUI.BeginFadeGroup(t))
			{
				for (int i = 0; i < childCount; i++)
				{
					children[i].DrawMenuItems(indentLevel + 1);
				}
			}
			SirenixEditorGUI.EndFadeGroup();
		}

		/// <summary>
		/// Draws the menu item with the specified indent level.
		/// </summary>
		public virtual void DrawMenuItem(int indentLevel)
		{
			Rect newRect = (EXPERIMENTAL_DontAllocateNewRect ? rect : GUILayoutUtility.GetRect(0f, Style.Height));
			Event e = OdinMenuTree.CurrentEvent;
			EventType eType = OdinMenuTree.CurrentEventType;
			if (eType == EventType.Layout)
			{
				return;
			}
			if (eType == EventType.Repaint || (eType != EventType.Layout && rect.width == 0f))
			{
				rect = newRect;
			}
			float rectY = rect.y;
			if (rectY > 1000f)
			{
				float visibleRectY = OdinMenuTree.VisibleRect.y;
				if (rectY + rect.height < visibleRectY || rectY > visibleRectY + OdinMenuTree.VisibleRect.height)
				{
					MenuItemIsBeingRendered = false;
					return;
				}
			}
			MenuItemIsBeingRendered = true;
			if (eType == EventType.Repaint)
			{
				float indent = Style.Offset + (float)indentLevel * Style.IndentAmount;
				labelRect = rect.AddXMin(indent);
				bool selected = IsSelected;
				if (selected)
				{
					if (OdinMenuTree.ActiveMenuTree == menuTree)
					{
						if (EditorGUIUtility.isProSkin)
						{
							EditorGUI.DrawRect(rect, Style.SelectedColorDarkSkin);
						}
						else
						{
							EditorGUI.DrawRect(rect, Style.SelectedColorLightSkin);
						}
					}
					else if (EditorGUIUtility.isProSkin)
					{
						EditorGUI.DrawRect(rect, Style.SelectedInactiveColorDarkSkin);
					}
					else
					{
						EditorGUI.DrawRect(rect, Style.SelectedInactiveColorLightSkin);
					}
				}
				if (IsSelectable && IsEnabled && !selected && rect.Contains(e.mousePosition))
				{
					EditorGUI.DrawRect(rect, SirenixGUIStyles.MouseOverBgOverlayColor);
				}
				if (ChildMenuItems.Count > 0 && !MenuTree.DrawInSearchMode && Style.DrawFoldoutTriangle)
				{
					if (Style.AlignTriangleLeft)
					{
						triangleRect = labelRect.AlignLeft(Style.TriangleSize).AlignMiddle(Style.TriangleSize);
						triangleRect.x -= Style.TriangleSize - Style.TrianglePadding;
					}
					else
					{
						triangleRect = rect.AlignRight(Style.TriangleSize).AlignMiddle(Style.TriangleSize);
						triangleRect.x -= Style.TrianglePadding;
					}
					EditorIcon icon = (Toggled ? EditorIcons.TriangleDown : EditorIcons.TriangleRight);
					if (eType == EventType.Repaint)
					{
						if (EditorGUIUtility.isProSkin)
						{
							if (selected || triangleRect.Contains(e.mousePosition))
							{
								GUI.DrawTexture(triangleRect, icon.Highlighted);
							}
							else
							{
								GUI.DrawTexture(triangleRect, icon.Active);
							}
						}
						else if (selected)
						{
							GUI.DrawTexture(triangleRect, icon.Raw);
						}
						else if (triangleRect.Contains(e.mousePosition))
						{
							GUI.DrawTexture(triangleRect, icon.Active);
						}
						else
						{
							GUIHelper.PushColor(new Color(1f, 1f, 1f, 0.7f));
							GUI.DrawTexture(triangleRect, icon.Active);
							GUIHelper.PopColor();
						}
					}
				}
				_ = SdfIcon;
				bool hasIcon;
				Texture texIcon;
				if (SdfIcon != SdfIconType.None)
				{
					hasIcon = true;
					texIcon = null;
				}
				else
				{
					texIcon = IconGetter();
					hasIcon = texIcon != null;
				}
				if (hasIcon)
				{
					if ((bool)texIcon)
					{
						Rect iconRect = labelRect.AlignLeft(Style.IconSize).AlignMiddle(Style.IconSize);
						iconRect.x += Style.IconOffset;
						if (!selected)
						{
							GUIHelper.PushColor(new Color(1f, 1f, 1f, Style.NotSelectedIconAlpha));
						}
						iconRect = iconRect.AlignCenter(Mathf.Min(iconRect.width, texIcon.width), Mathf.Min(iconRect.height, texIcon.height));
						GUI.DrawTexture(iconRect, texIcon, ScaleMode.ScaleToFit);
						if (!selected)
						{
							GUIHelper.PopColor();
						}
					}
					else
					{
						Rect iconRect2 = labelRect.AlignLeft(Style.IconSize - 3f).AlignMiddle(Style.IconSize - 3f);
						iconRect2.x += Style.IconOffset;
						if (selected)
						{
							SdfIcons.DrawIcon(iconRect2, SdfIcon, new Color(1f, 1f, 1f, 1f), style.SelectedColor);
						}
						else
						{
							Color col = SdfIconColor ?? SirenixGUIStyles.HighlightedTextColor;
							col.a = style.NotSelectedIconAlpha;
							SdfIcons.DrawIcon(iconRect2, SdfIcon, col);
						}
					}
					labelRect.xMin += Style.IconSize + Style.IconPadding;
				}
				GUIStyle labelStyle = (selected ? Style.SelectedLabelStyle : Style.DefaultLabelStyle);
				labelRect = labelRect.AlignMiddle(16f).AddY(Style.LabelVerticalOffset);
				GUI.Label(labelRect, SmartName, labelStyle);
				if (Style.Borders)
				{
					float borderPadding = Style.BorderPadding;
					bool draw = true;
					if (selected || previousMenuItemWasSelected)
					{
						borderPadding = 0f;
						if (!EditorGUIUtility.isProSkin)
						{
							draw = false;
						}
					}
					previousMenuItemWasSelected = selected;
					if (draw)
					{
						Rect border = rect;
						border.x += borderPadding;
						border.width -= borderPadding * 2f;
						SirenixEditorGUI.DrawHorizontalLineSeperator(border.x, border.y, border.width, Style.BorderAlpha);
					}
				}
			}
			OnDrawMenuItem(rect, labelRect);
			if (OnDrawItem != null)
			{
				OnDrawItem(this);
			}
			if (!IsEnabled)
			{
				EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.15f));
				if (menuTree.Selection.SelectedValue == this)
				{
					menuTree.Selection.Clear();
				}
			}
			if (!IsSelectable || !IsEnabled)
			{
				if (menuTree.Selection.SupportsMultiSelect)
				{
					foreach (object itemValue in menuTree.Selection.SelectedValues)
					{
						if (Value == itemValue)
						{
							menuTree.Selection.Remove(this);
						}
					}
				}
				if (menuTree.Selection.SelectedValue == Value)
				{
					menuTree.Selection.Clear();
				}
			}
			HandleMouseEvents(rect, triangleRect);
		}

		/// <summary>
		/// Override this to add custom GUI to the menu items.
		/// This is called right after the menu item is done drawing, and right before mouse input is handles so you can take control of that.
		/// </summary>
		protected virtual void OnDrawMenuItem(Rect rect, Rect labelRect)
		{
		}

		internal void UpdateMenuTreeRecursive(bool isRoot = false)
		{
			isInitialized = true;
			OdinMenuItem prev = null;
			foreach (OdinMenuItem child in ChildMenuItems)
			{
				child.parentMenuItem = null;
				child.nextMenuItem = null;
				child.previousMenuItemFlat = null;
				child.nextMenuItemFlat = null;
				child.previousMenuItem = null;
				if (!isRoot)
				{
					child.parentMenuItem = this;
				}
				if (prev != null)
				{
					prev.nextMenuItem = child;
					child.previousMenuItem = prev;
				}
				prev = child;
				child.UpdateMenuTreeRecursive();
			}
		}

		internal void UpdateFlatMenuItemNavigation()
		{
			int i = 0;
			OdinMenuItem prev = null;
			IEnumerable<OdinMenuItem> enumerable;
			if (!menuTree.DrawInSearchMode)
			{
				enumerable = menuTree.EnumerateTree();
			}
			else
			{
				IEnumerable<OdinMenuItem> flatMenuTree = menuTree.FlatMenuTree;
				enumerable = flatMenuTree;
			}
			IEnumerable<OdinMenuItem> query = enumerable;
			foreach (OdinMenuItem item in query)
			{
				item.flatTreeIndex = i++;
				item.nextMenuItemFlat = null;
				item.previousMenuItemFlat = null;
				if (prev != null)
				{
					item.previousMenuItemFlat = prev;
					prev.nextMenuItemFlat = item;
				}
				prev = item;
			}
		}

		internal int CountVisibleRecursively()
		{
			int result = 1;
			if (!Toggled)
			{
				return result;
			}
			foreach (OdinMenuItem childMenuItem in ChildMenuItems)
			{
				result += childMenuItem.CountVisibleRecursively();
			}
			return result;
		}

		internal void AllocateRectRecursivelyForScrollView()
		{
			OdinGUIScrollView scrollView = menuTree.ScrollView;
			scrollView.AllocateRect(menuTree.Config.DefaultMenuStyle.Height, this);
			if (menuTree.DrawInSearchMode || !Toggled)
			{
				return;
			}
			scrollView.Indentation += 1f;
			foreach (OdinMenuItem childMenuItem in ChildMenuItems)
			{
				childMenuItem.AllocateRectRecursivelyForScrollView();
			}
			scrollView.Indentation -= 1f;
		}

		/// <summary>
		/// Handles the mouse events.
		/// </summary>
		/// <param name="rect">The rect.</param>
		/// <param name="triangleRect">The triangle rect.</param>
		protected void HandleMouseEvents(Rect rect, Rect triangleRect)
		{
			if (!IsEnabled || !IsSelectable)
			{
				if (!Event.current.OnMouseDown(this.triangleRect, 0))
				{
					return;
				}
				bool toggle = false;
				if (ChildMenuItems.Any())
				{
					if (UnityShims.Misc.GetEventModifiers(Event.current) == 0)
					{
						toggle = true;
					}
					else if (triangleRect.Contains(Event.current.mousePosition))
					{
						toggle = true;
					}
				}
				if (!toggle || !Event.current.IsMouseOver(this.triangleRect))
				{
					return;
				}
				bool state = !Toggled;
				if (UnityShims.Misc.GetEventModifiers(Event.current) == 4)
				{
					foreach (OdinMenuItem item in GetChildMenuItemsRecursive(includeSelf: true))
					{
						item.Toggled = state;
					}
					return;
				}
				Toggled = state;
				return;
			}
			bool selectMenuItem = false;
			if (MenuTree.Config.SelectMenuItemsOnMouseDown)
			{
				if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
				{
					selectMenuItem = true;
					Event.current.Use();
				}
			}
			else
			{
				selectMenuItem = GUI.Button(rect, GUIContent.none, GUIStyle.none);
			}
			if (!selectMenuItem || !rect.Contains(Event.current.mousePosition))
			{
				return;
			}
			bool hasChildren = ChildMenuItems.Any();
			bool selected = IsSelected;
			if (Event.current.button == 1 && OnRightClick != null)
			{
				OnRightClick(this);
			}
			if (Event.current.button == 0)
			{
				bool toggle2 = false;
				if (hasChildren)
				{
					if (selected && UnityShims.Misc.GetEventModifiers(Event.current) == 0)
					{
						toggle2 = true;
					}
					else if (triangleRect.Contains(Event.current.mousePosition))
					{
						toggle2 = true;
					}
				}
				if (toggle2 && triangleRect.Contains(Event.current.mousePosition))
				{
					bool state2 = !Toggled;
					if (UnityShims.Misc.GetEventModifiers(Event.current) == 4)
					{
						foreach (OdinMenuItem item2 in GetChildMenuItemsRecursive(includeSelf: true))
						{
							item2.Toggled = state2;
						}
					}
					else
					{
						Toggled = state2;
					}
				}
				else if (menuTree.Selection.SupportsMultiSelect && UnityShims.Misc.GetEventModifiers(Event.current) == 1 && menuTree.Selection.Count > 0)
				{
					OdinMenuItem curr = menuTree.Selection.First();
					int maxIterations = Mathf.Abs(curr.FlatTreeIndex - FlatTreeIndex) + 1;
					bool down = curr.FlatTreeIndex < FlatTreeIndex;
					menuTree.Selection.Clear();
					for (int i = 0; i < maxIterations; i++)
					{
						if (curr == null)
						{
							break;
						}
						curr.Select(addToSelection: true);
						if (curr == this)
						{
							break;
						}
						curr = (down ? curr.NextSelectableMenuItem : curr.PrevSelectableMenuItem);
					}
				}
				else
				{
					bool ctrl = UnityShims.Misc.GetEventModifiers(Event.current) == 2;
					if (ctrl && selected && MenuTree.Selection.SupportsMultiSelect)
					{
						Deselect();
					}
					else
					{
						Select(ctrl);
					}
					if (MenuTree.Config.ConfirmSelectionOnDoubleClick && Event.current.clickCount == 2)
					{
						MenuTree.Selection.ConfirmSelection();
					}
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
			}
			GUIHelper.RemoveFocusControl();
			Event.current.Use();
		}

		public void Remove()
		{
			if (this == MenuTree.Root)
			{
				throw new Exception("Cannot remove root menu item!");
			}
			MenuTree.Selection.Remove(this);
			OdinMenuItem parent = Parent ?? MenuTree.RootMenuItem;
			parent.ChildMenuItems.Remove(this);
			MenuTree.MarkDirty();
		}

		internal bool _IsVisible()
		{
			if (menuTree.DrawInSearchMode)
			{
				return menuTree.FlatMenuTree.Contains(this);
			}
			return !ParentMenuItemsBottomUp(includeSelf: false).Any((OdinMenuItem x) => !x.Toggled);
		}

		internal void SetChildMenuItems(List<OdinMenuItem> newChildMenuItems)
		{
			childMenuItems = newChildMenuItems ?? new List<OdinMenuItem>();
		}

		private IEnumerable<OdinMenuItem> GetAllNextMenuItems()
		{
			if (nextMenuItemFlat == null)
			{
				yield break;
			}
			yield return nextMenuItemFlat;
			foreach (OdinMenuItem allNextMenuItem in nextMenuItemFlat.GetAllNextMenuItems())
			{
				yield return allNextMenuItem;
			}
		}

		private IEnumerable<OdinMenuItem> GetAllPreviousMenuItems()
		{
			if (previousMenuItemFlat == null)
			{
				yield break;
			}
			yield return previousMenuItemFlat;
			foreach (OdinMenuItem allPreviousMenuItem in previousMenuItemFlat.GetAllPreviousMenuItems())
			{
				yield return allPreviousMenuItem;
			}
		}

		private IEnumerable<OdinMenuItem> ParentMenuItemsBottomUp(bool includeSelf = true)
		{
			if (parentMenuItem != null)
			{
				foreach (OdinMenuItem item in parentMenuItem.ParentMenuItemsBottomUp())
				{
					yield return item;
				}
			}
			if (includeSelf)
			{
				yield return this;
			}
		}

		private void EnsureInitialized()
		{
			if (!isInitialized)
			{
				menuTree.UpdateMenuTree();
				if (!isInitialized)
				{
					Debug.LogWarning("Could not initialize menu item. Is the menu item not part of a menu tree?");
				}
			}
		}
	}
}
