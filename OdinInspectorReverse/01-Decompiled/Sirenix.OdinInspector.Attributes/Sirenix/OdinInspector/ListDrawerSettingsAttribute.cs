using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Customize the behavior for lists and arrays in the inspector.
	/// </summary>
	/// <example>
	/// <para>This example shows how you can add your own custom add button to a list.</para>
	/// <code>
	/// [ListDrawerSettings(HideAddButton = true, OnTitleBarGUI = "DrawTitleBarGUI")]
	/// public List&lt;MyType&gt; SomeList;
	///
	/// #if UNITY_EDITOR
	/// private void DrawTitleBarGUI()
	/// {
	///     if (SirenixEditorGUI.ToolbarButton(EditorIcons.Plus))
	///     {
	///         this.SomeList.Add(new MyType());
	///     }
	/// }
	/// #endif
	/// </code>
	/// </example>
	/// <remarks>
	/// This attribute is scheduled for refactoring.
	/// </remarks>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
	[Conditional("UNITY_EDITOR")]
	[DontApplyToListElements]
	public sealed class ListDrawerSettingsAttribute : Attribute
	{
		/// <summary>
		/// If true, the add button will not be rendered in the title toolbar. You can use OnTitleBarGUI to implement your own add button.
		/// </summary>
		/// <value>
		///   <c>true</c> if [hide add button]; otherwise, <c>false</c>.
		/// </value>
		public bool HideAddButton;

		/// <summary>
		/// If true, the remove button  will not be rendered on list items. You can use OnBeginListElementGUI and OnEndListElementGUI to implement your own remove button.
		/// </summary>
		/// <value>
		///     <c>true</c> if [hide remove button]; otherwise, <c>false</c>.    
		/// </value>
		public bool HideRemoveButton;

		/// <summary>
		/// Specify the name of a member inside each list element which defines the label being drawn for each list element.
		/// </summary>
		public string ListElementLabelName;

		/// <summary>
		/// Override the default behaviour for adding objects to the list. 
		/// If the referenced member returns the list type element, it will be called once per selected object.
		/// If the referenced method returns void, it will only be called once regardless of how many objects are selected.
		/// </summary>
		public string CustomAddFunction;

		[LabelWidth(200f)]
		public string CustomRemoveIndexFunction;

		[LabelWidth(200f)]
		public string CustomRemoveElementFunction;

		/// <summary>
		/// Calls a method before each list element. The member referenced must have a return type of void, and an index parameter of type int which represents the element index being drawn.
		/// </summary>
		public string OnBeginListElementGUI;

		/// <summary>
		/// Calls a method after each list element. The member referenced must have a return type of void, and an index parameter of type int which represents the element index being drawn.
		/// </summary>
		public string OnEndListElementGUI;

		/// <summary>
		/// If true, object/type pickers will never be shown when the list add button is clicked, and default(T) will always be added instantly instead, where T is the element type of the list.
		/// </summary>
		public bool AlwaysAddDefaultValue;

		/// <summary>
		/// Whether adding a new element should copy the last element. False by default.
		/// </summary>
		public bool AddCopiesLastElement;

		/// <summary> A resolved string with "int index" and "Color defaultColor" parameters that lets you control the color of individual elements. Supports a variety of color formats, including named colors (e.g. "red", "orange", "green", "blue"), hex codes (e.g. "#FF0000" and "#FF0000FF"), and RGBA (e.g. "RGBA(1,1,1,1)") or RGB (e.g. "RGB(1,1,1)"), including Odin attribute expressions (e.g "@this.MyColor"). Here are the available named colors: black, blue, clear, cyan, gray, green, grey, magenta, orange, purple, red, transparent, transparentBlack, transparentWhite, white, yellow, lightblue, lightcyan, lightgray, lightgreen, lightgrey, lightmagenta, lightorange, lightpurple, lightred, lightyellow, darkblue, darkcyan, darkgray, darkgreen, darkgrey, darkmagenta, darkorange, darkpurple, darkred, darkyellow. </summary>
		[ColorResolver]
		public string ElementColor;

		private string onTitleBarGUI;

		private int numberOfItemsPerPage;

		private bool paging;

		private bool draggable;

		private bool isReadOnly;

		private bool showItemCount;

		private bool pagingHasValue;

		private bool draggableHasValue;

		private bool isReadOnlyHasValue;

		private bool showItemCountHasValue;

		private bool numberOfItemsPerPageHasValue;

		private bool showIndexLabels;

		private bool showIndexLabelsHasValue;

		private bool defaultExpandedStateHasValue;

		private bool defaultExpandedState;

		/// <summary>
		/// Whether to show a foldout for the collection or not. If this is set to false, the collection will *always* be expanded.
		/// </summary>
		public bool ShowFoldout = true;

		/// <summary>
		/// Override the default setting specified in the Advanced Odin Preferences window and explicitly tell whether paging should be enabled or not.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "paging", "pagingHasValue" })]
		public bool ShowPaging
		{
			get
			{
				return paging;
			}
			set
			{
				paging = value;
				pagingHasValue = true;
			}
		}

		/// <summary>
		/// Override the default setting specified in the Advanced Odin Preferences window and explicitly tell whether items should be draggable or not.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "draggable", "draggableHasValue" })]
		public bool DraggableItems
		{
			get
			{
				return draggable;
			}
			set
			{
				draggable = value;
				draggableHasValue = true;
			}
		}

		/// <summary>
		/// Override the default setting specified in the Advanced Odin Preferences window and explicitly tells how many items each page should contain.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "numberOfItemsPerPage", "numberOfItemsPerPageHasValue" })]
		public int NumberOfItemsPerPage
		{
			get
			{
				return numberOfItemsPerPage;
			}
			set
			{
				numberOfItemsPerPage = value;
				numberOfItemsPerPageHasValue = true;
			}
		}

		/// <summary>
		/// Mark a list as read-only. This removes all editing capabilities from the list such as Add, Drag and delete,
		/// but without disabling GUI for each element drawn as otherwise would be the case if the <see cref="T:Sirenix.OdinInspector.ReadOnlyAttribute" /> was used.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "isReadOnly", "isReadOnlyHasValue" })]
		public bool IsReadOnly
		{
			get
			{
				return isReadOnly;
			}
			set
			{
				isReadOnly = value;
				isReadOnlyHasValue = true;
			}
		}

		/// <summary>
		/// Override the default setting specified in the Advanced Odin Preferences window and explicitly tell whether or not item count should be shown.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "showItemCount", "showItemCountHasValue" })]
		public bool ShowItemCount
		{
			get
			{
				return showItemCount;
			}
			set
			{
				showItemCount = value;
				showItemCountHasValue = true;
			}
		}

		/// <summary>
		/// <para>Whether to show a foldout for the collection or not. If this is set to false, the collection will *always* be expanded.</para>
		/// <para>
		/// This documentation used to wrongly state that this value would override the default setting specified in the Advanced Odin Preferences 
		/// window and explicitly tell whether or not the list should be expanded or collapsed by default. This value *would* do that, but it would
		/// also simultaneously act as ShowFoldout, leading to weird and unintuitive behaviour.
		/// </para>
		/// </summary>
		[Obsolete("Use ShowFoldout instead, which is what Expanded has always done. If you want to control the default expanded state, use DefaultExpandedState. Expanded has been implemented wrong for a long time.", false)]
		public bool Expanded
		{
			get
			{
				return !ShowFoldout;
			}
			set
			{
				ShowFoldout = !value;
			}
		}

		/// <summary>
		/// Override the default setting specified in the Odin Preferences window and explicitly tell whether or not the list should be expanded or collapsed by default.
		/// Note that this will override the persisted expand state, as this is set *every time* the collection drawer is initialized.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "defaultExpandedState", "defaultExpandedStateHasValue" })]
		public bool DefaultExpandedState
		{
			get
			{
				return defaultExpandedState;
			}
			set
			{
				defaultExpandedStateHasValue = true;
				defaultExpandedState = value;
			}
		}

		/// <summary>
		/// If true, a label is drawn for each element which shows the index of the element.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "showIndexLabels", "showIndexLabelsHasValue" })]
		public bool ShowIndexLabels
		{
			get
			{
				return showIndexLabels;
			}
			set
			{
				showIndexLabels = value;
				showIndexLabelsHasValue = true;
			}
		}

		/// <summary>
		/// Use this to inject custom GUI into the title-bar of the list.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "onTitleBarGUI" })]
		public string OnTitleBarGUI
		{
			get
			{
				return onTitleBarGUI;
			}
			set
			{
				onTitleBarGUI = value;
			}
		}

		/// <summary>
		/// Whether the Paging property is set.
		/// </summary>
		public bool PagingHasValue => pagingHasValue;

		/// <summary>
		/// Whether the ShowItemCount property is set.
		/// </summary>
		public bool ShowItemCountHasValue => showItemCountHasValue;

		/// <summary>
		/// Whether the NumberOfItemsPerPage property is set.
		/// </summary>
		public bool NumberOfItemsPerPageHasValue => numberOfItemsPerPageHasValue;

		/// <summary>
		/// Whether the Draggable property is set.
		/// </summary>
		public bool DraggableHasValue => draggableHasValue;

		/// <summary>
		/// Whether the IsReadOnly property is set.
		/// </summary>
		public bool IsReadOnlyHasValue => isReadOnlyHasValue;

		/// <summary>
		/// Whether the ShowIndexLabels property is set.
		/// </summary>
		public bool ShowIndexLabelsHasValue => showIndexLabelsHasValue;

		/// <summary>
		/// Whether the DefaultExpandedState property is set.
		/// </summary>
		public bool DefaultExpandedStateHasValue => defaultExpandedStateHasValue;
	}
}
