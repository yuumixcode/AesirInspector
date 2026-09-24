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
	/// OdinSelectors is an abstract base class that combines OdinMenuTrees and OdinEditorWindows to help making feature-rich selectors and popup selectors.
	/// </summary>
	/// <example>
	/// <code>
	/// public class MySelector : OdinSelector&lt;SomeType&gt;
	/// {
	///     private readonly List&lt;SomeType&gt; source;
	///     private readonly bool supportsMultiSelect;
	///
	///     public MySelector(List&lt;SomeType&gt; source, bool supportsMultiSelect)
	///     {
	///         this.source = source;
	///         this.supportsMultiSelect = supportsMultiSelect;
	///     }
	///
	///     protected override void BuildSelectionTree(OdinMenuTree tree)
	///     {
	///         tree.Config.DrawSearchToolbar = true;
	///         tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
	///
	///         tree.Add("Defaults/None", null);
	///         tree.Add("Defaults/A", new SomeType());
	///         tree.Add("Defaults/B", new SomeType());
	///
	///         tree.AddRange(this.source, x =&gt; x.Path, x =&gt; x.SomeTexture);
	///     }
	///
	///     [OnInspectorGUI]
	///     private void DrawInfoAboutSelectedItem()
	///     {
	///         SomeType selected = this.GetCurrentSelection().FirstOrDefault();
	///
	///         if (selected != null)
	///         {
	///             GUILayout.Label("Name: " + selected.Name);
	///             GUILayout.Label("Data: " + selected.Data);
	///         }
	///     }
	/// }
	/// </code>
	/// Usage:
	/// <code>
	/// void OnGUI()
	/// {
	///     if (GUILayout.Button("Open My Selector"))
	///     {
	///         List&lt;SomeType&gt; source = this.GetListOfThingsToSelectFrom();
	///         MySelector selector = new MySelector(source, false);
	///
	///         selector.SetSelection(this.someValue);
	///
	///         selector.SelectionCancelled += () =&gt; { };  // Occurs when the popup window is closed, and no slection was confirmed.
	///         selector.SelectionChanged += col =&gt; { };
	///         selector.SelectionConfirmed += col =&gt; this.someValue = col.FirstOrDefault();
	///
	///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
	///     }
	/// }
	///
	/// // All Odin Selectors can be rendered anywhere with Odin.
	/// [ShowInInspector]
	/// MySelector inlineSelector;
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.EnumSelector`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.TypeSelector" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.TypeSelectorV2" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinEditorWindow" />
	public abstract class OdinSelector<T> : ISelectionChangeListener
	{
		private static EditorWindow selectorFieldWindow;

		private static IEnumerable<T> selectedValues;

		private static bool selectionWasConfirmed;

		private static bool selectionWasChanged;

		private static GUIStyle titleStyle;

		private OdinEditorWindow popupWindowInstance;

		private OdinMenuTree selectionTree;

		/// <summary>
		/// If true, a confirm selection button will be drawn in the title-bar.
		/// </summary>
		[HideInInspector]
		public bool DrawConfirmSelectionButton;

		[SerializeField]
		[HideInInspector]
		private OdinMenuTreeDrawingConfig config = new OdinMenuTreeDrawingConfig
		{
			SearchToolbarHeight = 22,
			AutoScrollOnSelectionChanged = true,
			DefaultMenuStyle = new OdinMenuStyle
			{
				Height = 22
			}
		};

		private static bool wasKeyboard;

		private static int prevKeyboardId;

		private static GUIContent tmpValueLabel;

		/// <summary>
		/// Gets the selection menu tree.
		/// </summary>
		public OdinMenuTree SelectionTree
		{
			get
			{
				if (selectionTree == null)
				{
					selectionTree = new OdinMenuTree(supportsMultiSelect: true);
					selectionTree.Config = config;
					OdinMenuTree.ActiveMenuTree = selectionTree;
					BuildSelectionTree(selectionTree);
					selectionTree.Selection.SelectionConfirmed += delegate
					{
						if (this.SelectionConfirmed != null)
						{
							IEnumerable<T> currentSelection = GetCurrentSelection();
							if (IsValidSelection(currentSelection))
							{
								this.SelectionConfirmed(currentSelection);
							}
						}
					};
					selectionTree.Selection.SelectionChanged += delegate
					{
						TriggerSelectionChanged();
					};
				}
				return selectionTree;
			}
		}

		/// <summary>
		/// Gets the title. No title will be drawn if the string is null or empty.
		/// </summary>
		public virtual string Title => null;

		/// <summary>
		/// Occurs when the window is closed, and no slection was confirmed.
		/// </summary>
		public event Action SelectionCancelled;

		/// <summary>
		/// Occurs when the menuTrees selection is changed and IsValidSelection returns true.
		/// </summary>
		public event Action<IEnumerable<T>> SelectionChanged;

		/// <summary>
		/// Occurs when the menuTrees selection is confirmed and IsValidSelection returns true.
		/// </summary>
		public event Action<IEnumerable<T>> SelectionConfirmed;

		/// <summary>
		/// Enables the single click to select.
		/// </summary>
		public void EnableSingleClickToSelect()
		{
			SelectionTree.Config.SelectMenuItemsOnMouseDown = true;
			SelectionTree.EnumerateTree(delegate(OdinMenuItem x)
			{
				x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Remove(x.OnDrawItem, new Action<OdinMenuItem>(EnableSingleClickToSelect));
				x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Remove(x.OnDrawItem, new Action<OdinMenuItem>(EnableSingleClickToSelect));
				x.OnDrawItem = (Action<OdinMenuItem>)Delegate.Combine(x.OnDrawItem, new Action<OdinMenuItem>(EnableSingleClickToSelect));
			});
		}

		private void EnableSingleClickToSelect(OdinMenuItem obj)
		{
			EventType t = Event.current.type;
			if (t != EventType.Layout && obj.Rect.Contains(Event.current.mousePosition))
			{
				GUIHelper.RequestRepaint();
				if (Event.current.type == EventType.MouseDrag && obj is T && IsValidSelection(Enumerable.Repeat((T)obj.Value, 1)))
				{
					obj.Select();
				}
				if (t == EventType.MouseUp && obj.ChildMenuItems.Count == 0)
				{
					obj.MenuTree.Selection.ConfirmSelection();
					Event.current.Use();
				}
			}
		}

		/// <summary>
		/// Gets the current selection from the menu tree whether it's valid or not.
		/// </summary>
		public virtual IEnumerable<T> GetCurrentSelection()
		{
			return SelectionTree.Selection.Select((OdinMenuItem x) => x.Value).OfType<T>();
		}

		/// <summary>
		/// Determines whether the specified collection is a valid collection.
		/// If false, the SlectionChanged and SelectionConfirm events will not be called.
		/// By default, this returns true if the collection contains one or more items.
		/// </summary>
		public virtual bool IsValidSelection(IEnumerable<T> collection)
		{
			return true;
		}

		/// <summary>
		/// Sets the selection.
		/// </summary>
		public virtual void SetSelection(IEnumerable<T> selection)
		{
			SelectionTree.Selection.Clear();
			if (selection == null)
			{
				return;
			}
			foreach (T item in selection)
			{
				SetSelection(item);
			}
		}

		/// <summary>
		/// Sets the selection.
		/// </summary>
		public virtual void SetSelection(T selected)
		{
			if (selected != null)
			{
				List<OdinMenuItem> items = (from x in SelectionTree.EnumerateTree()
					where x.Value is T
					where EqualityComparer<T>.Default.Equals((T)x.Value, selected)
					select x).ToList();
				items.ForEach(delegate(OdinMenuItem x)
				{
					x.Select(addToSelection: true);
				});
			}
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified rect position.
		/// The width of the popup is determined by DefaultWindowWidth, and the height is automatically calculated.
		/// </summary>
		public OdinEditorWindow ShowInPopup()
		{
			EditorWindow prevSelectedWindow = EditorWindow.focusedWindow;
			float width = DefaultWindowWidth();
			OdinEditorWindow window = ((width != 0f) ? OdinEditorWindow.InspectObjectInDropDown(this, width) : OdinEditorWindow.InspectObjectInDropDown(this));
			SetupWindow(window, prevSelectedWindow);
			return window;
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified rect position.
		/// </summary>
		public OdinEditorWindow ShowInPopup(Rect btnRect)
		{
			return ShowInPopup(btnRect, btnRect.width);
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified rect position.
		/// </summary>
		public OdinEditorWindow ShowInPopup(Rect btnRect, float windowWidth)
		{
			EditorWindow prevSelectedWindow = EditorWindow.focusedWindow;
			OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, btnRect, windowWidth);
			SetupWindow(window, prevSelectedWindow);
			return window;
		}

		/// <summary>
		/// The mouse position is used as the position for the window.
		/// Opens up the selector instance in a popup at the specified position.
		/// </summary>
		public OdinEditorWindow ShowInPopup(float windowWidth)
		{
			EditorWindow prevSelectedWindow = EditorWindow.focusedWindow;
			OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, windowWidth);
			SetupWindow(window, prevSelectedWindow);
			return window;
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified position.
		/// </summary>
		public OdinEditorWindow ShowInPopup(Vector2 position, float windowWidth)
		{
			EditorWindow prevSelectedWindow = EditorWindow.focusedWindow;
			OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, position, windowWidth);
			SetupWindow(window, prevSelectedWindow);
			return window;
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified rect position.
		/// </summary>
		public OdinEditorWindow ShowInPopup(Rect btnRect, Vector2 windowSize)
		{
			EditorWindow prevSelectedWindow = EditorWindow.focusedWindow;
			OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, btnRect, windowSize);
			SetupWindow(window, prevSelectedWindow);
			return window;
		}

		/// <summary>
		/// Opens up the selector instance in a popup at the specified position.
		/// The width of the popup is determined by DefaultWindowWidth, and the height is automatically calculated.
		/// </summary>
		public OdinEditorWindow ShowInPopup(Vector2 position)
		{
			EditorWindow prevSelectedWindow = EditorWindow.focusedWindow;
			float width = DefaultWindowWidth();
			OdinEditorWindow window = ((width != 0f) ? OdinEditorWindow.InspectObjectInDropDown(this, position, width) : OdinEditorWindow.InspectObjectInDropDown(this, position));
			SetupWindow(window, prevSelectedWindow);
			return window;
		}

		/// <summary>
		/// Opens up the selector instance in a popup with the specified width and height.
		/// The mouse position is used as the position for the window.
		/// </summary>
		public OdinEditorWindow ShowInPopup(float width, float height)
		{
			EditorWindow prevSelectedWindow = EditorWindow.focusedWindow;
			OdinEditorWindow window = OdinEditorWindow.InspectObjectInDropDown(this, width, height);
			SetupWindow(window, prevSelectedWindow);
			return window;
		}

		/// <summary>
		/// Builds the selection tree.
		/// </summary>
		protected abstract void BuildSelectionTree(OdinMenuTree tree);

		/// <summary>
		/// When ShowInPopup is called, without a specifed window width, this methods gets called.
		/// Here you can calculate and give a good default width for the popup. 
		/// The default implementation returns 0, which will let the popup window determain the width itself. This is usually a fixed value.
		/// </summary>
		protected virtual float DefaultWindowWidth()
		{
			return 0f;
		}

		/// <summary>
		/// Triggers the selection changed event, but only if the current selection is valid.
		/// </summary>
		protected void TriggerSelectionChanged()
		{
			if (this.SelectionChanged != null)
			{
				IEnumerable<T> selected = GetCurrentSelection();
				if (IsValidSelection(selected))
				{
					this.SelectionChanged(selected);
				}
			}
		}

		/// <summary>
		/// Draw the selecotr manually.
		/// </summary>
		public void OnInspectorGUI()
		{
			DrawSelectionTree();
		}

		/// <summary>
		/// Draws the selection tree. This gets drawn using the OnInspectorGUI attribute.
		/// </summary>
		[PropertyOrder(-1f)]
		[OnInspectorGUI]
		protected virtual void DrawSelectionTree()
		{
			Rect rect = EditorGUILayout.BeginVertical();
			EditorGUI.DrawRect(rect, SirenixGUIStyles.DarkEditorBackground);
			GUILayout.Space(1f);
			DrawToolbar();
			bool prev = SelectionTree.Config.DrawSearchToolbar;
			SelectionTree.Config.DrawSearchToolbar = false;
			try
			{
				if (SelectionTree.MenuItems.Count == 0)
				{
					GUILayout.BeginVertical(SirenixGUIStyles.ContentPadding);
					SirenixEditorGUI.InfoMessageBox("There are no possible values to select.");
					GUILayout.EndVertical();
				}
				SelectionTree.DrawMenuTree();
			}
			finally
			{
				SelectionTree.Config.DrawSearchToolbar = prev;
			}
			SirenixEditorGUI.DrawBorders(rect, 1);
			EditorGUILayout.EndVertical();
		}

		protected virtual void DrawToolbar()
		{
			bool drawTitle = !string.IsNullOrEmpty(Title);
			bool drawSearchToolbar = SelectionTree.Config.DrawSearchToolbar;
			bool drawButton = DrawConfirmSelectionButton;
			if (drawTitle || drawSearchToolbar || drawButton)
			{
				SirenixEditorGUI.BeginHorizontalToolbar(SelectionTree.Config.SearchToolbarHeight);
				DrawToolbarTitle();
				DrawToolbarSearch();
				EditorGUI.DrawRect(GUILayoutUtility.GetLastRect().AlignLeft(1f), SirenixGUIStyles.BorderColor);
				DrawToolbarConfirmButton();
				SirenixEditorGUI.EndHorizontalToolbar();
			}
		}

		protected void DrawToolbarTitle()
		{
			if (!string.IsNullOrEmpty(Title))
			{
				if (titleStyle == null)
				{
					titleStyle = new GUIStyle(SirenixGUIStyles.LeftAlignedCenteredLabel)
					{
						padding = new RectOffset(10, 10, 0, 0)
					};
				}
				Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(Title), titleStyle, GUILayoutOptions.ExpandWidth(expand: false).Height(SelectionTree.Config.SearchToolbarHeight));
				if (Event.current.type == EventType.Repaint)
				{
					labelRect.y -= 2f;
					GUI.Label(labelRect.AlignCenterY(16f), Title, titleStyle);
				}
			}
		}

		protected void DrawToolbarSearch()
		{
			if (SelectionTree.Config.DrawSearchToolbar)
			{
				SelectionTree.DrawSearchToolbar(GUIStyle.none);
			}
			else
			{
				GUILayout.FlexibleSpace();
			}
		}

		protected void DrawToolbarConfirmButton()
		{
			if (DrawConfirmSelectionButton && SirenixEditorGUI.ToolbarButton(new GUIContent(EditorIcons.TestPassed)))
			{
				SelectionTree.Selection.ConfirmSelection();
			}
		}

		protected void RebuildMenuTree()
		{
			selectionTree = null;
		}

		private void SetupWindow(OdinEditorWindow window, EditorWindow prevSelectedWindow)
		{
			int prevFocusId = GUIUtility.hotControl;
			int prevKeyboardFocus = GUIUtility.keyboardControl;
			popupWindowInstance = window;
			SelectionChangeListener.Listeners.SubscribeListener(this);
			window.WindowPadding = default(Vector4);
			bool wasConfirmed = false;
			SelectionConfirmed += delegate
			{
				bool ctrl = Event.current != null && UnityShims.Misc.GetEventModifiers(Event.current) != 2;
				UnityEditorEventUtility.DelayAction(delegate
				{
					if (IsValidSelection(GetCurrentSelection()))
					{
						wasConfirmed = true;
						if (ctrl)
						{
							window.Close();
							if ((bool)prevSelectedWindow)
							{
								prevSelectedWindow.Focus();
							}
						}
					}
				});
			};
			window.OnBeginGUI += delegate
			{
				if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
				{
					if ((bool)prevSelectedWindow)
					{
						prevSelectedWindow.Focus();
					}
					else
					{
						UnityEditorEventUtility.DelayAction(window.Close);
					}
					Event.current.Use();
				}
			};
			window.OnClose += delegate
			{
				if (!wasConfirmed && this.SelectionCancelled != null)
				{
					this.SelectionCancelled();
				}
				GUIUtility.hotControl = prevFocusId;
				GUIUtility.keyboardControl = prevKeyboardFocus;
				EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
				{
					SelectionChangeListener.Listeners.DesubscribeListener(this);
				});
			};
		}

		internal static bool DrawSelectorButton<TSelector>(Rect buttonRect, string label, SdfIconType valueIcon, GUIStyle style, int id, bool returnValuesOnSelectionChange, out Action<TSelector> bindSelector, out Func<IEnumerable<T>> resultGetter) where TSelector : OdinSelector<T>
		{
			return DrawSelectorButton(buttonRect, new GUIContent(label), valueIcon, style, id, returnValuesOnSelectionChange, out bindSelector, out resultGetter);
		}

		internal static bool DrawSelectorButton<TSelector>(Rect buttonRect, GUIContent label, SdfIconType valueIcon, GUIStyle style, int id, bool returnValuesOnSelectionChange, out Action<TSelector> bindSelector, out Func<IEnumerable<T>> resultGetter) where TSelector : OdinSelector<T>
		{
			bool wasPressed = false;
			bindSelector = null;
			resultGetter = null;
			if (Event.current.type == EventType.Repaint)
			{
				bool showIsDown = GUIUtility.hotControl == id || GUIHelper.focusedControlId == id;
				style = style ?? EditorStyles.popup;
				Vector2 s = EditorGUIUtility.GetIconSize();
				EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
				if (valueIcon != SdfIconType.None)
				{
					style.Draw(buttonRect, GUIContent.none, showIsDown, showIsDown, on: false, GUIUtility.keyboardControl == id);
					SdfIcons.DrawIcon(buttonRect.TakeFromLeft(buttonRect.height * 1.2f).Padding(2f, 0f, 3f, 3f), valueIcon);
					SirenixGUIStyles.Label.Draw(buttonRect, label, 0);
				}
				else
				{
					style.Draw(buttonRect, label, showIsDown, showIsDown, on: false, GUIUtility.keyboardControl == id);
				}
				EditorGUIUtility.SetIconSize(s);
			}
			bool openPopup = false;
			if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown && GUIUtility.keyboardControl == id)
			{
				GUIUtility.hotControl = id;
				wasKeyboard = true;
			}
			else if (GUIUtility.hotControl == id && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp && GUIUtility.keyboardControl == id)
			{
				openPopup = true;
				wasKeyboard = true;
			}
			else if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && buttonRect.Contains(Event.current.mousePosition))
			{
				GUIUtility.hotControl = id;
				wasKeyboard = false;
			}
			else if (GUIUtility.hotControl == id && Event.current.type == EventType.MouseUp && Event.current.button == 0 && buttonRect.Contains(Event.current.mousePosition))
			{
				openPopup = true;
				wasKeyboard = false;
			}
			if (openPopup)
			{
				prevKeyboardId = GUIUtility.keyboardControl;
				selectedValues = null;
				selectionWasConfirmed = false;
				selectionWasChanged = false;
				GUIHelper.focusedControlId = id;
				selectorFieldWindow = EditorWindow.focusedWindow;
				GUIUtility.hotControl = id;
				if (wasKeyboard)
				{
					GUIUtility.keyboardControl = id;
				}
				bindSelector = delegate(TSelector selector)
				{
					selector.SelectionChanged += delegate(IEnumerable<T> x)
					{
						selectedValues = x;
						selectionWasChanged = true;
						GUIHelper.confirmedPopupControlId = id;
					};
					selector.SelectionConfirmed += delegate(IEnumerable<T> x)
					{
						selectionWasConfirmed = true;
						selectedValues = x;
						GUIHelper.confirmedPopupControlId = id;
					};
					OdinEditorWindow odinEditorWindow = selector.popupWindowInstance;
					if (odinEditorWindow != null)
					{
						odinEditorWindow.OnClose += delegate
						{
							GUIHelper.focusedControlId = -1;
							GUIHelper.confirmedPopupControlId = id;
						};
					}
				};
				wasPressed = true;
				Event.current.Use();
			}
			if (Event.current.type == EventType.Repaint && selectorFieldWindow == GUIHelper.CurrentWindow && id == GUIHelper.confirmedPopupControlId)
			{
				if (wasKeyboard)
				{
					GUIUtility.keyboardControl = prevKeyboardId;
				}
				else
				{
					GUIUtility.keyboardControl = 0;
				}
				if (GUIHelper.focusedControlId == -1)
				{
					GUIHelper.confirmedPopupControlId = 0;
				}
				if (selectionWasConfirmed)
				{
					GUIHelper.confirmedPopupControlId = 0;
					GUIHelper.focusedControlId = -1;
					GUI.changed = true;
					selectionWasConfirmed = false;
					GUIHelper.RequestRepaint();
					resultGetter = () => selectedValues ?? Enumerable.Empty<T>();
				}
				else if (selectionWasChanged)
				{
					selectionWasChanged = false;
					GUIHelper.RequestRepaint();
					if (returnValuesOnSelectionChange)
					{
						resultGetter = () => selectedValues ?? Enumerable.Empty<T>();
					}
				}
			}
			return wasPressed;
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(Rect rect, string btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null)
		{
			return DrawSelectorDropdown(rect, new GUIContent(btnLabel), createSelector, style);
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(Rect rect, string btnLabel, Func<Rect, OdinSelector<T>> createSelector, bool returnValuesOnSelectionChange, GUIStyle style = null)
		{
			return DrawSelectorDropdown(rect, new GUIContent(btnLabel), createSelector, returnValuesOnSelectionChange, style);
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(Rect rect, GUIContent btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null)
		{
			return DrawSelectorDropdown(rect, btnLabel, createSelector, returnValuesOnSelectionChange: true, style);
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(Rect rect, GUIContent btnLabel, Func<Rect, OdinSelector<T>> createSelector, bool returnValuesOnSelectionChange, GUIStyle style = null, SdfIconType valueIcon = SdfIconType.None)
		{
			tmpValueLabel = tmpValueLabel ?? new GUIContent();
			int id = GUIUtility.GetControlID(FocusType.Keyboard);
			tmpValueLabel.image = btnLabel.image;
			tmpValueLabel.text = (EditorGUI.showMixedValue ? "—" : btnLabel.text);
			tmpValueLabel.tooltip = btnLabel.tooltip;
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
			{
				GUIUtility.keyboardControl = id;
			}
			style = style ?? EditorStyles.popup;
			if (DrawSelectorButton(rect, tmpValueLabel, valueIcon, style, id, returnValuesOnSelectionChange, out Action<OdinSelector<T>> bindSelector, out Func<IEnumerable<T>> getResult))
			{
				OdinSelector<T> selector = createSelector(rect);
				bindSelector(selector);
				if (Application.platform == RuntimePlatform.LinuxEditor)
				{
					GUIHelper.ExitGUI(removeFocusControl: true);
				}
			}
			return getResult?.Invoke();
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(GUIContent label, string btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null, params GUILayoutOption[] options)
		{
			return DrawSelectorDropdown(label, new GUIContent(btnLabel), createSelector, style, options);
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(GUIContent label, string btnLabel, Func<Rect, OdinSelector<T>> createSelector, bool returnValuesOnSelectionChange, GUIStyle style = null, params GUILayoutOption[] options)
		{
			return DrawSelectorDropdown(label, new GUIContent(btnLabel), createSelector, returnValuesOnSelectionChange, style, options);
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(GUIContent label, GUIContent btnLabel, Func<Rect, OdinSelector<T>> createSelector, GUIStyle style = null, params GUILayoutOption[] options)
		{
			return DrawSelectorDropdown(label, btnLabel, createSelector, returnValuesOnSelectionChange: true, style, options);
		}

		/// <summary>
		/// Draws dropwdown field, that creates and binds the selector to the dropdown field.
		/// </summary>
		public static IEnumerable<T> DrawSelectorDropdown(GUIContent label, GUIContent btnLabel, Func<Rect, OdinSelector<T>> createSelector, bool returnValuesOnSelectionChange, GUIStyle style = null, params GUILayoutOption[] options)
		{
			tmpValueLabel = tmpValueLabel ?? new GUIContent();
			tmpValueLabel.image = btnLabel.image;
			tmpValueLabel.text = (EditorGUI.showMixedValue ? "—" : btnLabel.text);
			tmpValueLabel.tooltip = btnLabel.tooltip;
			SirenixEditorGUI.GetFeatureRichControlRect(label, out var id, out var _, out var rect, options);
			style = style ?? EditorStyles.popup;
			if (DrawSelectorButton(rect, tmpValueLabel, SdfIconType.None, style, id, returnValuesOnSelectionChange, out Action<OdinSelector<T>> bindSelector, out Func<IEnumerable<T>> getResult))
			{
				OdinSelector<T> selector = createSelector(rect);
				bindSelector(selector);
				if (Application.platform == RuntimePlatform.LinuxEditor)
				{
					GUIHelper.ExitGUI(removeFocusControl: false);
				}
			}
			return getResult?.Invoke();
		}

		void ISelectionChangeListener.OnSelectionChanged()
		{
			popupWindowInstance.Close();
		}
	}
}
