using System;
using Clipboard = Sirenix.Utilities.Editor.Clipboard;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerAttributePopup : EditorWindow
	{
		public class Page
		{
			public bool IsVisible;

			public SirenixAnimationUtility.InterpolatedFloat T;

			private SlideFrom slideFrom;

			public Page(SlideFrom slideFrom)
			{
				this.slideFrom = slideFrom;
			}

			public void Draw(Rect viewport, Action<Rect> draw)
			{
				T.ChangeDestination(IsVisible ? 1f : 0f);
				T.Move(10f);
				Color c = GUI.color;
				c.a = T;
				GUI.BeginClip(viewport);
				EditorGUI.BeginDisabledGroup(Math.Abs((float)T - 1f) > 0.2f);
				GUIHelper.PushColor(c);
				float offsetY = (1f - (float)T) * viewport.height * ((slideFrom == SlideFrom.Bottom) ? 1f : (-1f));
				Rect contentRect = new Rect(0f, offsetY, viewport.width, viewport.height);
				draw?.Invoke(contentRect);
				GUIHelper.PopColor();
				EditorGUI.EndDisabledGroup();
				GUI.EndClip();
			}

			public void Show(bool skipAnimation = false)
			{
				if (skipAnimation)
				{
					T = 1f;
				}
				IsVisible = true;
			}

			public void Hide(bool skipAnimation = false)
			{
				if (skipAnimation)
				{
					T = 0f;
				}
				IsVisible = false;
			}
		}

		public enum SlideFrom
		{
			Top,
			Bottom
		}

		public static DesignerVirtualizedScrollView ScrollView;

		public static bool NeedsScrollViewRebuild;

		private static List<DesignerPopupCategory> categories;

		private static DesignerPopupCategory favoriteCategory;

		private static string searchTerm;

		[HideInInspector]
		public DesignerEditor Editor;

		[HideInInspector]
		public bool IsPinned;

		[HideInInspector]
		public bool IsDragging;

		[HideInInspector]
		public Vector2 DragStartPosition;

		[HideInInspector]
		public bool IsResizing;

		[HideInInspector]
		public DesignerEditorWindow.ResizeEdge ActiveResizeEdge;

		[HideInInspector]
		public Vector2 ResizeStartMouseScreen;

		[HideInInspector]
		public Rect ResizeStartWindowRect;

		[HideInInspector]
		public Page EditPage;

		[HideInInspector]
		public Page AddPage;

		private SearchField searchField;

		public bool FlipItemColors;

		private int selectedRowIndex = -1;

		private static bool searching => !string.IsNullOrEmpty(searchTerm);

		public static DesignerAttributePopup Open(Rect contextRect, DesignerEditor editor)
		{
			UnityEngine.Object[] existingPopups = Resources.FindObjectsOfTypeAll(typeof(DesignerAttributePopup));
			DesignerAttributePopup popup = null;
			for (int i = 0; i < existingPopups.Length; i++)
			{
				DesignerAttributePopup current = (DesignerAttributePopup)existingPopups[i];
				if (current.Editor == editor)
				{
					popup = current;
					break;
				}
			}
			if (popup == null)
			{
				popup = ScriptableObject.CreateInstance<DesignerAttributePopup>();
				popup.hideFlags = HideFlags.HideAndDontSave;
				popup.Editor = editor;
			}
			else
			{
				popup.Focus();
				if (popup.IsPinned)
				{
					return popup;
				}
			}
			editor.RetainIn(popup);
			if (contextRect == Rect.zero)
			{
				UnityShims.Rect.Ctor(out contextRect, GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - new Vector2(2f, 2f), new Vector2(4f, 4f));
			}
			int width = GlobalConfig<OdinVisualDesignerConfig>.Instance.PopupWidth;
			int height = GlobalConfig<OdinVisualDesignerConfig>.Instance.PopupHeight;
			Rect windowRect = new Rect(contextRect.x + contextRect.width - (float)width, contextRect.yMax + 2f, width, height);
			Rect fittedWindowRect = windowRect;
			if (!contextRect.Overlaps(fittedWindowRect))
			{
				popup.position = fittedWindowRect;
			}
			else
			{
				windowRect.y = contextRect.y - (float)height - 2f;
				popup.position = windowRect;
			}
			UpdateAttributeItems();
			popup.EditPage = new Page(SlideFrom.Top);
			popup.AddPage = new Page(SlideFrom.Bottom);
			popup.GotoEditPage(skipAnimation: true);
			EditorWindow_Internal.ShowPopupNoLayout(popup);
			return popup;
		}

		public void OnLostFocus()
		{
			CancelWindowInteraction();
			if (IsPinned)
			{
				return;
			}
			DesignerEditor editor = Editor;
			UnityEditorEventUtility.DelayAction(delegate
			{
				if (editor == null)
				{
					Close();
				}
				else if (!EditorWindow_Internal.IsTransientWindow(EditorWindow.focusedWindow))
				{
					Close();
				}
			});
		}

		private void OnDestroy()
		{
			CancelWindowInteraction();
			Editor?.ReleaseFrom(this);
		}

		public void OnGUI()
		{
			DesignerEditor.IsEditorRendering = true;
			Event e = Event.current;
			Rect rect = base.position.SetPosition(Vector2.zero);
			int resizeId = GUIUtility.GetControlID(FocusType.Passive, rect);
			(bool, DesignerEditorWindow.ResizeEdge, Vector2, Rect) tuple = DesignerEditorWindow.HandleWindowResizing(IsResizing, ActiveResizeEdge, ResizeStartMouseScreen, ResizeStartWindowRect, this, resizeId, rect, DesignerEditorWindow.ResizeEdge.Right | DesignerEditorWindow.ResizeEdge.Bottom, 6f, 260f, 220f);
			IsResizing = tuple.Item1;
			ActiveResizeEdge = tuple.Item2;
			ResizeStartMouseScreen = tuple.Item3;
			ResizeStartWindowRect = tuple.Item4;
			rect = base.position.SetPosition(Vector2.zero);
			EditorGUI.DrawRect(rect, Colors.Editor.Bg);
			SirenixEditorGUI.DrawBorders(rect, 1);
			rect = rect.Padding(1f);
			Rect headerRect = rect.TakeFromTop(26f);
			EditorGUI.DrawRect(headerRect, Colors.AttributePopup.TitleBarBg);
			EditorGUI.DrawRect(headerRect.AlignBottom(1f), SirenixGUIStyles.BorderColor);
			DesignerGUI.DrawHaloBar(headerRect, "DesignerAttributeExampleWindow_Header", Colors.AttributePopup.ButtonHaloBg, Colors.AttributePopup.PinHalo, Colors.AttributePopup.CloseHalo, IsPinned ? 0.6f : 0.15f, 0.15f, IsPinned);
			Rect pinRect = headerRect.TakeFromLeft(headerRect.height);
			Rect closeRect = headerRect.TakeFromRight(headerRect.height);
			bool showOpenDesignerButton = Editor?.Window == null;
			Rect openDesignerRect = Rect.zero;
			if (showOpenDesignerButton)
			{
				openDesignerRect = headerRect.TakeFromRight(headerRect.height);
			}
			if (GUI.Button(pinRect, GUIContent.none, GUIStyle.none))
			{
				IsPinned = !IsPinned;
			}
			if (GUI.Button(closeRect, new GUIContent("", DesignerGUI.Tooltips.CloseWindow), GUIStyle.none))
			{
				Close();
			}
			if (showOpenDesignerButton && GUI.Button(openDesignerRect, GUIContent.none, GUIStyle.none))
			{
				Editor?.OpenWindow();
				if (!IsPinned)
				{
					Close();
				}
			}
			DesignerGUI.DrawIcon(pinRect.Padding(6f), IsPinned ? SdfIconType.PinAngleFill : SdfIconType.PinAngle, Colors.Icons.Default, DesignerGUI.Tooltips.Pin);
			DesignerGUI.DrawIcon(closeRect.Padding(6f), SdfIconType.X, Colors.Icons.Default, "");
			if (showOpenDesignerButton)
			{
				DesignerGUI.DrawIcon(openDesignerRect.Padding(6f), SdfIconType.BoxArrowInUpRight, EditorStyles.label.normal.textColor, DesignerGUI.Tooltips.OpenVisualDesigner);
			}
			Editor?.TypePatch.HandleDiskSynchronization();
			DesignerEditorContext ctx = Editor.Context;
			bool isDisabled = ctx.IsNonDeclMembersLocked && ctx.SelectedNode != null && ctx.SelectedNode.NodeColor != Color.white && ctx.SelectedNode.NodeType != DesignerEditorNodeType.Root;
			if (isDisabled)
			{
				GUIHelper.PushGUIEnabled(enabled: false);
			}
			searchField = searchField ?? new SearchField();
			if (AddPage.IsVisible)
			{
				if (e.type == EventType.KeyDown && searchField.HasFocus() && (e.keyCode == KeyCode.DownArrow || e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.Tab))
				{
					int rowCount = GetRowCount();
					if (rowCount > 0)
					{
						selectedRowIndex = ((e.keyCode == KeyCode.UpArrow || (e.keyCode == KeyCode.Tab && e.shift)) ? (rowCount - 1) : 0);
						GUIHelper.RemoveFocusControl();
						e.Use();
					}
				}
				Color color = GUI.color;
				color.a = AddPage.T;
				GUIHelper.PushColor(color);
				EditorGUI.BeginChangeCheck();
				searchTerm = searchField.Draw(headerRect.Padding(20f, 3f), searchTerm, "Search");
				if (EditorGUI.EndChangeCheck())
				{
					UpdateAttributeItems();
					if (!searchTerm.IsNullOrWhitespace())
					{
						for (int i = 0; i < categories.Count; i++)
						{
							DesignerPopupCategory category = categories[i];
							category.IsExpanded = true;
							categories[i] = category;
						}
						NeedsScrollViewRebuild = true;
					}
				}
				if (searchField.HasFocus())
				{
					selectedRowIndex = -1;
				}
				GUIHelper.PopColor();
			}
			EditPage.Draw(rect, DrawAttributesPage);
			AddPage.Draw(rect, DrawAddAttributePage);
			if (isDisabled)
			{
				GUIHelper.PopGUIEnabled();
			}
			bool exit = false;
			int viewId = GUIUtility.GetControlID(FocusType.Passive, rect);
			if (!IsResizing)
			{
				(IsDragging, DragStartPosition) = DesignerEditorWindow.HandleWindowMovement(IsDragging, DragStartPosition, this, viewId);
			}
			EventType type = e.type;
			if (type == EventType.KeyDown)
			{
				switch (e.keyCode)
				{
				case KeyCode.P:
					IsPinned = !IsPinned;
					e.Use();
					break;
				case KeyCode.Backspace:
					if (AddPage.IsVisible)
					{
						GotoEditPage();
						e.Use();
					}
					break;
				case KeyCode.Escape:
					exit = true;
					e.Use();
					break;
				}
			}
			if (IsDragging)
			{
				EditorGUIUtility.AddCursorRect(base.position.SetPosition(Vector2.zero), MouseCursor.Pan);
			}
			DesignerEditor.IsEditorRendering = false;
			if (exit)
			{
				Close();
			}
			Repaint();
		}

		private void CancelWindowInteraction()
		{
			if (IsDragging || IsResizing)
			{
				IsDragging = false;
				IsResizing = false;
				ActiveResizeEdge = DesignerEditorWindow.ResizeEdge.None;
				GUIUtility.hotControl = 0;
			}
		}

		public void DrawAttributesPage(Rect rect)
		{
			Event e = Event.current;
			Rect addButtonAreaRect = rect.TakeFromBottom(42f);
			Rect addButtonRect = addButtonAreaRect.AlignCenter(180f, 22f);
			if (GUIUtility.hotControl == 0 && GUIUtility.keyboardControl == 0 && (e.keyCode == KeyCode.Space || e.keyCode == KeyCode.Return))
			{
				GotoAddPage();
				searchField.Focus();
			}
			GUILayout.BeginArea(rect);
			Editor?.Context.DrawAttributes(rect);
			GUILayout.EndArea();
			DesignerEditorNodeType? selectedNodeType = Editor?.Context.SelectedNode?.NodeType;
			if (!selectedNodeType.HasValue || (selectedNodeType.Value != DesignerEditorNodeType.Root && selectedNodeType.Value != DesignerEditorNodeType.Member))
			{
				return;
			}
			if (DesignerGUI.DrawHaloButton(addButtonRect, $"{this}", "Add Attribute", DesignerGUI.Tooltips.AddAttribute))
			{
				GotoAddPage();
				searchField.Focus();
			}
			if (!Event.current.OnContextClick(rect) && !Event.current.OnMouseDown(1))
			{
				return;
			}
			GenericMenu genericMenu = new GenericMenu();
			if (Clipboard.CanPaste(typeof(Attribute)) && Editor != null && Editor.Context.Selection.IsValid)
			{
				genericMenu.AddItem(new GUIContent("Paste Attribute"), on: false, delegate
				{
					Editor.Context.PasteAttributeToSelection(Clipboard.Paste<Attribute>());
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Paste Attribute"));
			}
			genericMenu.ShowAsContext();
		}

		public void DrawAddAttributePage(Rect rect)
		{
			Rect btnArea = rect.TakeFromBottom(42f);
			Rect btnRect = btnArea.AlignCenter(180f, 22f);
			HandleAddAttributeKeyboard(rect);
			if (DesignerGUI.DrawHaloButton(btnRect, $"{this}", "Go Back (Esc)", DesignerGUI.Tooltips.ReturnToAttributeList) || (!EditorGUIUtility.editingTextField && Event.current.OnKeyDown(KeyCode.Escape)))
			{
				GotoEditPage();
				searchTerm = "";
				selectedRowIndex = -1;
				GUIHelper.RemoveFocusControl();
				DesignerAttributeExampleWindow.CloseAllDesignerAttributeExampleWindows();
				GUI.changed = true;
			}
			ClampSelection();
			foreach (VisibleItem visibleItem in ScrollView.VisibleItems(rect))
			{
				bool isSelected = selectedRowIndex >= 0 && visibleItem.AbsoluteIndex == selectedRowIndex;
				if (Event.current.type == EventType.MouseDown)
				{
					Rect rect2 = visibleItem.Rect;
					if (rect2.Contains(Event.current.mousePosition))
					{
						selectedRowIndex = visibleItem.AbsoluteIndex;
						GUIHelper.RemoveFocusControl();
						GUI.changed = true;
					}
				}
				TryResolveRow(visibleItem.AbsoluteIndex, out var item, out var categoryIndex, out var isCategoryHeader);
				if (isCategoryHeader)
				{
					DesignerPopupCategory category = categories[categoryIndex];
					if (category.Items.Length <= 0)
					{
						continue;
					}
					bool highlight = isSelected;
					if (Event.current.control)
					{
						if (Event.current.IsHovering(rect))
						{
							highlight = true;
						}
						if (Event.current.OnMouseDown(visibleItem.Rect, 0))
						{
							bool targetState = !category.IsExpanded;
							for (int i = 0; i < categories.Count; i++)
							{
								DesignerPopupCategory c = categories[i];
								c.IsExpanded = targetState;
								categories[i] = c;
							}
							NeedsScrollViewRebuild = true;
							break;
						}
					}
					category.Draw(visibleItem.Rect, visibleItem.AbsoluteIndex, highlight);
					categories[categoryIndex] = category;
				}
				else
				{
					bool isEven = visibleItem.AbsoluteIndex % 2 == 0;
					item.Draw(visibleItem.Rect, this, 0, isEven ^ FlipItemColors, isSelected);
				}
			}
			if (NeedsScrollViewRebuild)
			{
				CreateScrollView();
				NeedsScrollViewRebuild = false;
			}
		}

		private void HandleAddAttributeKeyboard(Rect listRect)
		{
			Event e = Event.current;
			if (e.type != EventType.KeyDown || !AddPage.IsVisible || categories == null)
			{
				return;
			}
			bool isEnter = e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter;
			bool isRight = e.keyCode == KeyCode.RightArrow;
			bool isLeft = e.keyCode == KeyCode.LeftArrow;
			bool isUp = e.keyCode == KeyCode.UpArrow;
			bool isDown = e.keyCode == KeyCode.DownArrow;
			bool isTab = e.keyCode == KeyCode.Tab;
			if (!(isEnter || isRight || isLeft || isUp || isDown || isTab))
			{
				return;
			}
			int rowCount = GetRowCount();
			DesignerPopupItem item;
			int categoryIndex;
			bool isCategoryHeader;
			if (selectedRowIndex < 0)
			{
				if (rowCount > 0)
				{
					if (isDown || (isTab && !e.shift))
					{
						selectedRowIndex = 0;
						GUIHelper.RemoveFocusControl();
						e.Use();
					}
					else if (isUp || (isTab && e.shift))
					{
						selectedRowIndex = rowCount - 1;
						GUIHelper.RemoveFocusControl();
						e.Use();
					}
				}
			}
			else if (rowCount <= 0)
			{
				selectedRowIndex = -1;
				searchField.Focus();
				e.Use();
			}
			else if (isUp || (isTab && e.shift))
			{
				if (selectedRowIndex == 0)
				{
					selectedRowIndex = -1;
					searchField.Focus();
				}
				else
				{
					selectedRowIndex--;
					CenterSelection(listRect);
				}
				e.Use();
			}
			else if (isDown || (isTab && !e.shift))
			{
				if (selectedRowIndex == rowCount - 1)
				{
					selectedRowIndex = -1;
					searchField.Focus();
				}
				else
				{
					selectedRowIndex++;
					CenterSelection(listRect);
				}
				e.Use();
			}
			else if (!TryResolveRow(selectedRowIndex, out item, out categoryIndex, out isCategoryHeader))
			{
				e.Use();
			}
			else if (isCategoryHeader)
			{
				DesignerPopupCategory category = categories[categoryIndex];
				if (isLeft)
				{
					if (category.IsExpanded)
					{
						category.IsExpanded = false;
						categories[categoryIndex] = category;
						CreateScrollView();
						NeedsScrollViewRebuild = false;
					}
					e.Use();
				}
				else if (isRight)
				{
					if (!category.IsExpanded)
					{
						category.IsExpanded = true;
						categories[categoryIndex] = category;
						CreateScrollView();
						NeedsScrollViewRebuild = false;
					}
					e.Use();
				}
				else if (isEnter)
				{
					category.IsExpanded = !category.IsExpanded;
					categories[categoryIndex] = category;
					CreateScrollView();
					NeedsScrollViewRebuild = false;
					e.Use();
				}
				else
				{
					e.Use();
				}
			}
			else if (isLeft)
			{
				OdinVisualDesignerConfig typeConfig = GlobalConfig<OdinVisualDesignerConfig>.Instance;
				Type attributeType = item.AttributeType;
				int oldFavoritesCount = favoriteCategory.Items.Length;
				bool wasInFavorites = selectedRowIndex < oldFavoritesCount;
				if (typeConfig.FavoriteAttributes.Contains(attributeType))
				{
					typeConfig.FavoriteAttributes.Remove(attributeType);
				}
				else
				{
					typeConfig.FavoriteAttributes.Add(attributeType);
				}
				PopulateFavorites();
				CreateScrollView();
				NeedsScrollViewRebuild = false;
				if (!wasInFavorites)
				{
					int relative = selectedRowIndex - oldFavoritesCount;
					selectedRowIndex = relative + favoriteCategory.Items.Length;
				}
				ClampSelection();
				e.Use();
			}
			else if (isRight)
			{
				DesignerAttributeExampleWindow activeWindow = Resources.FindObjectsOfTypeAll<DesignerAttributeExampleWindow>().FirstOrDefault();
				if (activeWindow != null)
				{
					if (activeWindow.AttributeType == item.AttributeType)
					{
						DesignerAttributeExampleWindow.CloseAllDesignerAttributeExampleWindows();
					}
					else
					{
						DesignerAttributeExampleWindow.Open(UnityShims.Rect.Ctor(base.position.position + new Vector2(base.position.width + 10f, 0f), Vector2.zero), item.AttributeType);
					}
				}
				else
				{
					DesignerAttributeExampleWindow.Open(UnityShims.Rect.Ctor(base.position.position + new Vector2(base.position.width + 10f, 0f), Vector2.zero), item.AttributeType);
				}
				Focus();
				e.Use();
			}
			else if (isEnter)
			{
				Editor.Context.AddAttributeToSelection(item.AttributeType);
				GotoEditPage();
				DesignerAttributeExampleWindow.CloseAllDesignerAttributeExampleWindows();
				ClearSearchField();
				e.Use();
			}
			else
			{
				e.Use();
			}
		}

		private int GetRowCount()
		{
			int count = favoriteCategory.Items.Length;
			for (int i = 0; i < categories.Count; i++)
			{
				DesignerPopupCategory category = categories[i];
				if (category.Items.Length > 0)
				{
					count++;
					if (category.IsExpanded)
					{
						count += category.Items.Length;
					}
				}
			}
			return count;
		}

		private void ClampSelection()
		{
			int rowCount = GetRowCount();
			if (rowCount <= 0)
			{
				selectedRowIndex = -1;
			}
			else if (selectedRowIndex >= 0 && selectedRowIndex >= rowCount)
			{
				selectedRowIndex = rowCount - 1;
			}
		}

		public static void CreateCategories()
		{
			categories = new List<DesignerPopupCategory>(8);
			foreach (Assembly assembly in AssemblyUtilities.GetAllAssemblies())
			{
				IEnumerable<OdinVisualDesignerAttributeItem> attributeItems = assembly.GetAttributes<OdinVisualDesignerAttributeItem>(inherit: false);
				foreach (OdinVisualDesignerAttributeItem attributeItem in attributeItems)
				{
					if (!categories.Any((DesignerPopupCategory c) => c.Label == attributeItem.Category))
					{
						DesignerPopupCategory category = new DesignerPopupCategory(attributeItem.Category, isExpanded: false);
						category.EnsureItemsCapacity(24);
						categories.Add(category);
					}
				}
			}
		}

		public static void PopulateAttributes()
		{
			foreach (Assembly assembly in AssemblyUtilities.GetAllAssemblies())
			{
				IEnumerable<OdinVisualDesignerAttributeItem> attributeItems = assembly.GetAttributes<OdinVisualDesignerAttributeItem>(inherit: false);
				foreach (OdinVisualDesignerAttributeItem attributeItem in attributeItems)
				{
					int categoryIndex = categories.FindIndex((DesignerPopupCategory c) => c.Label == attributeItem.Category);
					if (categoryIndex != -1)
					{
						DesignerPopupCategory category = categories[categoryIndex];
						if (!searching || FuzzySearch.Contains(searchTerm.ToLowerInvariant(), attributeItem.Label.ToLowerInvariant()))
						{
							DesignerPopupItem item = new DesignerPopupItem(attributeItem);
							category.Items.Add(ref item);
							categories[categoryIndex] = category;
						}
					}
				}
			}
		}

		public static void PopulateFavorites()
		{
			OdinVisualDesignerConfig typeConfig = GlobalConfig<OdinVisualDesignerConfig>.Instance;
			favoriteCategory = new DesignerPopupCategory("Favorites", isExpanded: true);
			favoriteCategory.EnsureItemsCapacity(typeConfig.FavoriteAttributes.Count);
			foreach (DesignerPopupCategory category in categories)
			{
				for (int i = 0; i < category.Items.Length; i++)
				{
					DesignerPopupItem item = category.Items[i];
					if (typeConfig.FavoriteAttributes.Contains(item.AttributeType))
					{
						favoriteCategory.Items.Add(ref item);
					}
				}
			}
		}

		public static void UpdateAttributeItems()
		{
			CreateCategories();
			PopulateAttributes();
			PopulateFavorites();
			CreateScrollView();
		}

		public void GotoEditPage(bool skipAnimation = false)
		{
			EditPage.Show(skipAnimation);
			AddPage.Hide(skipAnimation);
		}

		public void GotoAddPage(bool skipAnimation = false)
		{
			AddPage.Show(skipAnimation);
			EditPage.Hide(skipAnimation);
			UpdateAttributeItems();
			selectedRowIndex = -1;
		}

		public void ClearSearchField()
		{
			searchTerm = "";
			UpdateAttributeItems();
			selectedRowIndex = -1;
		}

		public static void CreateScrollView()
		{
			if (ScrollView == null)
			{
				ScrollView = new DesignerVirtualizedScrollView(128);
			}
			ScrollView.BeginRectAllocations();
			for (int i = 0; i < favoriteCategory.Items.Length; i++)
			{
				ScrollView.GetRect(24f);
			}
			for (int j = 0; j < categories.Count; j++)
			{
				DesignerPopupCategory category = categories[j];
				if (category.Items.Length <= 0)
				{
					continue;
				}
				ScrollView.GetRect(24f);
				if (category.IsExpanded)
				{
					for (int k = 0; k < category.Items.Length; k++)
					{
						ScrollView.GetRect(24f);
					}
				}
			}
			ScrollView.EndRectAllocations();
			ScrollView.UpdateScrollView = true;
		}

		private static bool TryResolveRow(int index, out DesignerPopupItem popupItem, out int categoryIndex, out bool isCategoryHeader)
		{
			popupItem = default(DesignerPopupItem);
			categoryIndex = -1;
			isCategoryHeader = false;
			if (index < 0)
			{
				return false;
			}
			RefList<DesignerPopupItem> favoriteItems = favoriteCategory.Items;
			if (index < favoriteItems.Length)
			{
				popupItem = favoriteItems[index];
				return true;
			}
			index -= favoriteItems.Length;
			for (int ci = 0; ci < categories.Count; ci++)
			{
				DesignerPopupCategory category = categories[ci];
				if (category.Items.Length <= 0)
				{
					continue;
				}
				if (index == 0)
				{
					categoryIndex = ci;
					isCategoryHeader = true;
					return true;
				}
				index--;
				if (category.IsExpanded)
				{
					RefList<DesignerPopupItem> categoryItems = category.Items;
					if (index < categoryItems.Length)
					{
						popupItem = categoryItems[index];
						categoryIndex = ci;
						return true;
					}
					index -= categoryItems.Length;
				}
			}
			return false;
		}

		private void CenterSelection(Rect listRect)
		{
			if (selectedRowIndex >= 0 && ScrollView != null)
			{
				int h = 24;
				int rowCount = GetRowCount();
				if (rowCount > 0)
				{
					int totalHeight = rowCount * h;
					float maxScroll = Mathf.Max(0f, (float)totalHeight - listRect.height);
					float desired = (float)(selectedRowIndex * h) - (listRect.height - (float)h) * 0.5f;
					desired = Mathf.Clamp(desired, 0f, maxScroll);
					ScrollView.ScrollPosition = desired;
					ScrollView.UpdateScrollView = true;
				}
			}
		}
	}
}
