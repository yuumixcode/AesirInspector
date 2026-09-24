using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Draws an editor window with a menu tree.
	/// </summary>
	/// <example>
	/// <code>
	/// public class OdinMenuEditorWindowExample : OdinMenuEditorWindow
	/// {
	///     [SerializeField, HideLabel]
	///     private SomeData someData = new SomeData();
	///
	///     protected override OdinMenuTree BuildMenuTree()
	///     {
	///         OdinMenuTree tree = new OdinMenuTree(supportsMultiSelect: true)
	///         {
	///             { "Home",                           this,                           EditorIcons.House       }, // draws the someDataField in this case.
	///             { "Odin Settings",                  null,                           SdfIconType.GearFill    },
	///             { "Odin Settings/Color Palettes",   ColorPaletteManager.Instance,   EditorIcons.EyeDropper  },
	///             { "Odin Settings/AOT Generation",   AOTGenerationConfig.Instance,   EditorIcons.SmartPhone  },
	///             { "Camera current",                 Camera.current                                          },
	///             { "Some Class",                     this.someData                                           }
	///         };
	///
	///         tree.AddAllAssetsAtPath("More Odin Settings", SirenixAssetPaths.OdinEditorConfigsPath, typeof(ScriptableObject), true)
	///             .AddThumbnailIcons();
	///
	///         tree.AddAssetAtPath("Odin Getting Started", SirenixAssetPaths.SirenixPluginPath + "Getting Started With Odin.asset");
	///
	///         var customMenuItem = new OdinMenuItem(tree, "Menu Style", tree.DefaultMenuStyle);
	///         tree.MenuItems.Insert(2, customMenuItem);
	///
	///         tree.Add("Menu/Items/Are/Created/As/Needed", new GUIContent());
	///         tree.Add("Menu/Items/Are/Created", new GUIContent("And can be overridden"));
	///
	///         // As you can see, Odin provides a few ways to quickly add editors / objects to your menu tree.
	///         // The API also gives you full control over the selection, etc..
	///         // Make sure to check out the API Documentation for OdinMenuEditorWindow, OdinMenuTree and OdinMenuItem for more information on what you can do!
	///
	///         return tree;
	///     }
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuStyle" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeExtensions" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	public abstract class OdinMenuEditorWindow : OdinEditorWindow
	{
		protected Color MenuBackgroundColor = Color.clear;

		[NonSerialized]
		private bool isDirty;

		[HideInInspector]
		[SerializeField]
		private OdinMenuTreeDrawingConfig menuTreeConfig;

		[HideInInspector]
		[SerializeField]
		private float menuWidth = 180f;

		[NonSerialized]
		private OdinMenuTree menuTree;

		[NonSerialized]
		private object trySelectObject;

		[HideInInspector]
		[SerializeField]
		private List<string> selectedItems = new List<string>();

		[SerializeField]
		[HideInInspector]
		private bool resizableMenuWidth = true;

		private OdinMenuTreeDrawingConfig MenuTreeConfig
		{
			get
			{
				menuTreeConfig = menuTreeConfig ?? new OdinMenuTreeDrawingConfig
				{
					DrawScrollView = true,
					DrawSearchToolbar = false,
					AutoHandleKeyboardNavigation = false
				};
				return menuTreeConfig;
			}
		}

		/// <summary>
		/// Gets or sets the width of the menu.
		/// </summary>
		public virtual float MenuWidth
		{
			get
			{
				return menuWidth;
			}
			set
			{
				menuWidth = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the menu is resizable.
		/// </summary>
		public virtual bool ResizableMenuWidth
		{
			get
			{
				return resizableMenuWidth;
			}
			set
			{
				resizableMenuWidth = value;
			}
		}

		/// <summary>
		/// Gets the menu tree.
		/// </summary>
		public OdinMenuTree MenuTree => menuTree;

		/// <summary>
		/// Gets or sets a value indicating whether to draw the menu search bar.
		/// </summary>
		public bool DrawMenuSearchBar
		{
			get
			{
				return MenuTreeConfig.DrawSearchToolbar;
			}
			set
			{
				MenuTreeConfig.DrawSearchToolbar = value;
			}
		}

		/// <summary>
		/// Gets or sets the custom search function.
		/// </summary>
		public Func<OdinMenuItem, bool> CustomSearchFunction
		{
			get
			{
				return MenuTreeConfig.SearchFunction;
			}
			set
			{
				MenuTreeConfig.SearchFunction = value;
			}
		}

		private void ProjectWindowChanged()
		{
			isDirty = true;
		}

		/// <summary>
		/// Called when the window is destroyed. Remember to call base.OnDestroy();
		/// </summary>
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (UnityEditorEventUtility.HasOnProjectChanged)
			{
				UnityEditorEventUtility.OnProjectChanged -= ProjectWindowChanged;
				UnityEditorEventUtility.OnProjectChanged -= ProjectWindowChanged;
			}
			else
			{
				EditorApplication.projectWindowChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.projectWindowChanged, new EditorApplication.CallbackFunction(ProjectWindowChanged));
				EditorApplication.projectWindowChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.projectWindowChanged, new EditorApplication.CallbackFunction(ProjectWindowChanged));
			}
		}

		/// <summary>
		/// Builds the menu tree.
		/// </summary>
		protected abstract OdinMenuTree BuildMenuTree();

		/// <summary>
		/// Forces the menu tree rebuild.
		/// </summary>
		public void ForceMenuTreeRebuild()
		{
			menuTree = BuildMenuTree();
			if (selectedItems.Count == 0 && menuTree.Selection.Count == 0)
			{
				OdinMenuItem firstMenu = menuTree.EnumerateTree().FirstOrDefault((OdinMenuItem x) => x.Value != null);
				if (firstMenu != null)
				{
					firstMenu.GetParentMenuItemsRecursive(includeSelf: false).ForEach(delegate(OdinMenuItem x)
					{
						x.Toggled = true;
					});
					firstMenu.Select();
				}
			}
			else if (menuTree.Selection.Count == 0 && selectedItems.Count > 0)
			{
				foreach (OdinMenuItem item in menuTree.EnumerateTree())
				{
					if (selectedItems.Contains(item.GetFullPath()))
					{
						item.Select(addToSelection: true);
					}
				}
			}
			menuTree.Selection.SelectionChanged += OnSelectionChanged;
		}

		private void OnSelectionChanged(SelectionChangedType type)
		{
			Repaint();
			GUIHelper.RemoveFocusControl();
			selectedItems = menuTree.Selection.Select((OdinMenuItem x) => x.GetFullPath()).ToList();
			EditorUtility.SetDirty(this);
		}

		/// <summary>
		/// Tries to select the menu item with the specified object.
		/// </summary>
		public void TrySelectMenuItemWithObject(object obj)
		{
			trySelectObject = obj;
		}

		/// <summary>
		/// Draws the menu tree selection.
		/// </summary>
		protected override IEnumerable<object> GetTargets()
		{
			if (menuTree == null)
			{
				yield break;
			}
			for (int i = 0; i < menuTree.Selection.Count; i++)
			{
				OdinMenuItem item = menuTree.Selection[i];
				if (item != null)
				{
					object val = item.Value;
					if (val is Func<object> func)
					{
						val = func();
					}
					if (val != null)
					{
						yield return val;
					}
				}
			}
		}

		/// <summary>
		/// Draws the Odin Editor Window.
		/// </summary>
		protected override void OnImGUI()
		{
			if (Event.current.type == EventType.Layout)
			{
				bool setActive = menuTree == null;
				if (menuTree == null || isDirty)
				{
					ForceMenuTreeRebuild();
					if (setActive)
					{
						OdinMenuTree.ActiveMenuTree = menuTree;
					}
					if (UnityEditorEventUtility.HasOnProjectChanged)
					{
						UnityEditorEventUtility.OnProjectChanged -= ProjectWindowChanged;
						UnityEditorEventUtility.OnProjectChanged += ProjectWindowChanged;
					}
					else
					{
						EditorApplication.projectWindowChanged = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.projectWindowChanged, new EditorApplication.CallbackFunction(ProjectWindowChanged));
						EditorApplication.projectWindowChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.projectWindowChanged, new EditorApplication.CallbackFunction(ProjectWindowChanged));
					}
					isDirty = false;
				}
				if (trySelectObject != null && menuTree != null)
				{
					OdinMenuItem menuItem = menuTree.EnumerateTree().FirstOrDefault((OdinMenuItem x) => x.Value == trySelectObject);
					if (menuItem != null)
					{
						menuTree.Selection.Clear();
						menuItem.Select();
						trySelectObject = null;
					}
				}
			}
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical(GUILayoutOptions.Width(MenuWidth).ExpandHeight());
			Rect rect = GUIHelper.GetCurrentLayoutRect();
			if (menuTree != null)
			{
				menuTree.MenuTreeActivationZone(rect);
			}
			EditorGUI.DrawRect(rect, (MenuBackgroundColor == Color.clear) ? SirenixGUIStyles.MenuBackgroundColor : MenuBackgroundColor);
			Rect menuBorderRect = rect;
			menuBorderRect.xMin = rect.xMax - 4f;
			menuBorderRect.xMax += 4f;
			if (ResizableMenuWidth)
			{
				EditorGUIUtility.AddCursorRect(menuBorderRect, MouseCursor.ResizeHorizontal);
				MenuWidth += SirenixEditorGUI.SlideRect(menuBorderRect).x;
			}
			DrawMenu();
			GUILayout.EndVertical();
			GUILayout.BeginVertical(GUILayoutOptions.ExpandHeight());
			Rect rect2 = GUIHelper.GetCurrentLayoutRect();
			EditorGUI.DrawRect(rect2, SirenixGUIStyles.DarkEditorBackground);
			EditorGUI.DrawRect(menuBorderRect.AlignCenter(1f), SirenixGUIStyles.BorderColor);
			base.OnImGUI();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			if (menuTree != null)
			{
				menuTree.HandleKeyboardMenuNavigation();
			}
			this.RepaintIfRequested();
		}

		/// <summary>
		/// The method that draws the menu.
		/// </summary>
		protected virtual void DrawMenu()
		{
			if (menuTree != null)
			{
				menuTree.DrawMenuTree();
			}
		}
	}
}
