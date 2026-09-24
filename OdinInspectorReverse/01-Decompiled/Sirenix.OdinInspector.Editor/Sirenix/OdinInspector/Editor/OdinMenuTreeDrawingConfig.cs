using System;
using System.ComponentModel;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// The config used by OdinMenuTree to specify which features of the Menu Tree should be used when drawing.
	/// </summary>
	[Serializable]
	public class OdinMenuTreeDrawingConfig
	{
		private Func<OdinMenuItem, bool> searchFunction;

		[SerializeField]
		private OdinMenuStyle menuItemStyle;

		/// <summary>
		/// The automatic scroll on selection changed. True by default.
		/// </summary>
		public bool AutoScrollOnSelectionChanged = true;

		/// <summary>
		/// Whether to draw the tree in a scrollable view. True by default.
		/// </summary>
		public bool DrawScrollView = true;

		/// <summary>
		/// Whether to handle keyboard navigation after it's done drawing. True by default.
		/// </summary>
		public bool AutoHandleKeyboardNavigation = true;

		/// <summary>
		/// Whether to draw a searchbar above the menu tree. True by default.
		/// </summary>
		public bool DrawSearchToolbar = true;

		/// <summary>
		/// Whether to the menu items expanded state should be cached. True by default.
		/// </summary>
		public bool UseCachedExpandedStates = true;

		/// <summary>
		/// Whether to automatically set focus on the search bar when the tree is drawn for the first time. True by default.
		/// </summary>
		public bool AutoFocusSearchBar = true;

		/// <summary>
		/// Whether to select menu items on mouse down, or on click. False by default.
		/// </summary>
		public bool SelectMenuItemsOnMouseDown;

		/// <summary>
		/// The scroll-view position.
		/// </summary>
		public Vector2 ScrollPos;

		/// <summary>
		/// The search term.
		/// </summary>
		public string SearchTerm = "";

		/// <summary>
		/// The height of the search toolbar.
		/// </summary>
		public int SearchToolbarHeight = 24;

		public bool EXPERIMENTAL_INTERNAL_DrawFlatTreeFastNoLayout;

		/// <summary>
		/// Will only handle layouting when there has been a hierarchical change (such as an item being expanded or collapsed).
		/// </summary>
		/// <remarks>
		/// This expects every item to have the same height specified in the style (<see cref="F:Sirenix.OdinInspector.Editor.OdinMenuStyle.Height" />).
		/// This means no custom layouting is supported with this on.
		/// </remarks>
		public bool EXPERIMENTAL_INTERNAL_SparseFixedLayouting;

		/// <summary>
		/// By default, the MenuTree.Selection is confirmed when menu items are double clicked, 
		/// Set this to false if you don't want that behaviour.
		/// </summary>
		[HideInInspector]
		[Obsolete("Use ConfirmSelectionOnDoubleClick instead.", false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ConfirmSlecectionOnDoubleClick = true;

		/// <summary>
		/// Gets or sets the default menu item style.
		/// </summary>
		public OdinMenuStyle DefaultMenuStyle
		{
			get
			{
				if (menuItemStyle == null)
				{
					menuItemStyle = new OdinMenuStyle();
				}
				return menuItemStyle;
			}
			set
			{
				menuItemStyle = value;
			}
		}

		/// <summary>
		/// Gets or sets the search function. Null by default.
		/// </summary>
		public Func<OdinMenuItem, bool> SearchFunction
		{
			get
			{
				return searchFunction;
			}
			set
			{
				searchFunction = value;
			}
		}

		/// <summary>
		/// By default, the MenuTree.Selection is confirmed when menu items are double clicked, 
		/// Set this to false if you don't want that behaviour.
		/// </summary>
		public bool ConfirmSelectionOnDoubleClick
		{
			get
			{
				return ConfirmSlecectionOnDoubleClick;
			}
			set
			{
				ConfirmSlecectionOnDoubleClick = value;
			}
		}
	}
}
