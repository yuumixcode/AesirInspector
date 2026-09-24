using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// The TableList attirbute drawer.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.TableListAttribute" />
	public class TableListAttributeDrawer : OdinAttributeDrawer<TableListAttribute>, IDisposable
	{
		private enum ColumnType
		{
			Property,
			Index,
			DeleteButton
		}

		private class Column : IResizableColumn
		{
			public string Name;

			public float ColWidth;

			public float MinWidth;

			public bool Preserve;

			public bool Resizable;

			public string NiceName;

			public int NiceNameLabelWidth;

			public ColumnType ColumnType;

			public bool PreferWide;

			float IResizableColumn.ColWidth
			{
				get
				{
					return ColWidth;
				}
				set
				{
					ColWidth = value;
				}
			}

			float IResizableColumn.MinWidth => MinWidth;

			bool IResizableColumn.PreserveWidth => Preserve;

			bool IResizableColumn.Resizable => Resizable;

			public Column(int minWidth, bool preserveWidth, bool resizable, string name, ColumnType colType)
			{
				MinWidth = minWidth;
				ColWidth = minWidth;
				Preserve = preserveWidth;
				Name = name;
				ColumnType = colType;
				Resizable = resizable;
			}
		}

		private static readonly int TableListDrawerId = "id_TableListDrawer".GetHashCode();

		private IOrderedCollectionResolver resolver;

		private LocalPersistentContext<bool> isPagingExpanded;

		private LocalPersistentContext<Vector2> scrollPos;

		private LocalPersistentContext<int> currPage;

		private GUITableRowLayoutGroup table;

		private HashSet<string> seenColumnNames;

		private List<Column> columns;

		private int colOffset;

		private GUIContent indexLabel;

		private bool isReadOnly;

		private int indexLabelWidth;

		private Rect columnHeaderRect;

		private GUIPagingHelper paging;

		private bool drawAsList;

		private bool isFirstFrame = true;

		private MultiCollectionFilter<IOrderedCollectionResolver> filter;

		/// <summary>
		/// Determines whether this instance [can draw attribute property] the specified property.
		/// </summary>
		protected override bool CanDrawAttributeProperty(InspectorProperty property)
		{
			return property.ChildResolver is IOrderedCollectionResolver;
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			drawAsList = false;
			isReadOnly = base.Attribute.IsReadOnly || !base.Property.ValueEntry.IsEditable;
			indexLabelWidth = (int)SirenixGUIStyles.Label.CalcSize(new GUIContent("100")).x + 15;
			indexLabel = new GUIContent();
			colOffset = 0;
			seenColumnNames = new HashSet<string>();
			table = new GUITableRowLayoutGroup();
			table.MinScrollViewHeight = base.Attribute.MinScrollViewHeight;
			table.MaxScrollViewHeight = base.Attribute.MaxScrollViewHeight;
			resolver = base.Property.ChildResolver as IOrderedCollectionResolver;
			scrollPos = this.GetPersistentValue("scrollPos", Vector2.zero);
			currPage = this.GetPersistentValue("currPage", 0);
			isPagingExpanded = this.GetPersistentValue("expanded", defaultValue: false);
			columns = new List<Column>(10);
			paging = new GUIPagingHelper();
			paging.NumberOfItemsPerPage = ((base.Attribute.NumberOfItemsPerPage > 0) ? base.Attribute.NumberOfItemsPerPage : GlobalConfig<GeneralDrawerConfig>.Instance.NumberOfItemsPrPage);
			paging.IsExpanded = isPagingExpanded.Value;
			paging.IsEnabled = GlobalConfig<GeneralDrawerConfig>.Instance.ShowPagingInTables || base.Attribute.ShowPaging;
			paging.CurrentPage = currPage.Value;
			base.Property.ValueEntry.OnChildValueChanged += OnChildValueChanged;
			filter = new MultiCollectionFilter<IOrderedCollectionResolver>(base.Property, base.Property.ChildResolver as IOrderedCollectionResolver);
			if (base.Attribute.AlwaysExpanded)
			{
				base.Property.State.Expanded = true;
			}
			int p = base.Attribute.CellPadding;
			if (p > 0)
			{
				table.CellStyle = new GUIStyle
				{
					padding = new RectOffset(p, p, p, p)
				};
			}
			GUIHelper.RequestRepaint();
			if (base.Attribute.ShowIndexLabels)
			{
				colOffset++;
				columns.Add(new Column(indexLabelWidth, preserveWidth: true, resizable: false, null, ColumnType.Index));
			}
			if (!isReadOnly)
			{
				columns.Add(new Column(22, preserveWidth: true, resizable: false, null, ColumnType.DeleteButton));
			}
		}

		/// <summary>
		/// Draws the property layout.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (drawAsList)
			{
				if (GUILayout.Button("Draw as table"))
				{
					drawAsList = false;
				}
				CallNextDrawer(label);
				return;
			}
			paging.Update(filter.GetCount());
			currPage.Value = paging.CurrentPage;
			isPagingExpanded.Value = paging.IsExpanded;
			Rect rect = SirenixEditorGUI.BeginIndentedVertical(SirenixGUIStyles.PropertyMargin);
			if (!base.Attribute.HideToolbar)
			{
				DrawToolbar(label);
			}
			if (base.Attribute.AlwaysExpanded)
			{
				base.Property.State.Expanded = true;
				DrawColumnHeaders();
				DrawTable();
			}
			else
			{
				if (SirenixEditorGUI.BeginFadeGroup(this, base.Property.State.Expanded) && base.Property.Children.Count > 0)
				{
					DrawColumnHeaders();
					DrawTable();
				}
				SirenixEditorGUI.EndFadeGroup();
			}
			SirenixEditorGUI.EndIndentedVertical();
			if (Event.current.type == EventType.Repaint)
			{
				SirenixEditorGUI.DrawBorders(rect, 1, 1, (!base.Attribute.HideToolbar) ? 1 : 0, 1);
			}
			DropZone(rect);
			if (OdinObjectSelector.IsReadyToClaim(this, TableListDrawerId))
			{
				resolver.QueueAdd(OdinObjectSelector.ClaimMultiple(base.Property.Tree.WeakTargets.Count));
			}
			if (Event.current.type == EventType.Repaint)
			{
				isFirstFrame = false;
			}
		}

		private void OnChildValueChanged(int index)
		{
			IPropertyValueEntry valueEntry = base.Property.Children[index].ValueEntry;
			if (valueEntry == null || !typeof(ScriptableObject).IsAssignableFrom(valueEntry.TypeOfValue))
			{
				return;
			}
			for (int i = 0; i < valueEntry.ValueCount; i++)
			{
				UnityEngine.Object uObj = valueEntry.WeakValues[i] as UnityEngine.Object;
				if ((bool)uObj)
				{
					EditorUtility.SetDirty(uObj);
				}
			}
		}

		private void DropZone(Rect rect)
		{
			if (isReadOnly)
			{
				return;
			}
			EventType eventType = Event.current.type;
			if ((eventType != EventType.DragUpdated && eventType != EventType.DragPerform) || !rect.Contains(Event.current.mousePosition))
			{
				return;
			}
			UnityEngine.Object[] objReferences = null;
			if (DragAndDrop.objectReferences.Any((UnityEngine.Object n) => n != null && resolver.ElementType.IsAssignableFrom(n.GetType())))
			{
				objReferences = DragAndDrop.objectReferences.Where((UnityEngine.Object x) => x != null && resolver.ElementType.IsAssignableFrom(x.GetType())).Reverse().ToArray();
			}
			else if (resolver.ElementType.InheritsFrom(typeof(Component)))
			{
				UnityEngine.Object[] array = (from x in DragAndDrop.objectReferences.OfType<GameObject>()
					select x.GetComponent(resolver.ElementType) into x
					where x != null
					select x).Reverse().ToArray();
				objReferences = array;
			}
			else if (resolver.ElementType.InheritsFrom(typeof(Sprite)) && DragAndDrop.objectReferences.Any((UnityEngine.Object n) => n is Texture2D && AssetDatabase.Contains(n)))
			{
				UnityEngine.Object[] array = (from x in DragAndDrop.objectReferences.OfType<Texture2D>().Select(delegate(Texture2D x)
					{
						string assetPath = AssetDatabase.GetAssetPath(x);
						return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
					})
					where x != null
					select x).Reverse().ToArray();
				objReferences = array;
			}
			if (objReferences == null || objReferences.Length == 0)
			{
				return;
			}
			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			Event.current.Use();
			if (eventType != EventType.DragPerform)
			{
				return;
			}
			DragAndDrop.AcceptDrag();
			UnityEngine.Object[] array2 = objReferences;
			foreach (UnityEngine.Object obj in array2)
			{
				object[] values = new object[base.Property.ParentValues.Count];
				for (int i = 0; i < values.Length; i++)
				{
					values[i] = obj;
				}
				resolver.QueueAdd(values);
			}
		}

		private void AddColumns(int rowIndexFrom, int rowIndexTo)
		{
			if (Event.current.type != EventType.Layout)
			{
				return;
			}
			for (int y = rowIndexFrom; y < rowIndexTo; y++)
			{
				int skip = 0;
				InspectorProperty rowProperty = base.Property.Children[y];
				for (int x = 0; x < rowProperty.Children.Count; x++)
				{
					InspectorProperty colProperty = rowProperty.Children[x];
					if (!seenColumnNames.Add(colProperty.Name))
					{
						continue;
					}
					HideInTablesAttribute hide = GetColumnAttribute<HideInTablesAttribute>(colProperty);
					if (hide != null)
					{
						skip++;
						continue;
					}
					bool preserve = false;
					bool resizable = true;
					bool preferWide = true;
					int width = base.Attribute.DefaultMinColumnWidth;
					TableColumnWidthAttribute colAttr = GetColumnAttribute<TableColumnWidthAttribute>(colProperty);
					if (colAttr != null)
					{
						preserve = !colAttr.Resizable;
						resizable = colAttr.Resizable;
						width = colAttr.Width;
						preferWide = false;
					}
					Column newCol = new Column(width, preserve, resizable, colProperty.Name, ColumnType.Property);
					newCol.NiceName = colProperty.NiceName;
					newCol.NiceNameLabelWidth = (int)SirenixGUIStyles.Label.CalcSize(new GUIContent(newCol.NiceName)).x;
					newCol.PreferWide = preferWide;
					int index = x + colOffset - skip;
					columns.Insert(Math.Min(index, columns.Count), newCol);
					GUIHelper.RequestRepaint();
				}
			}
		}

		private void DrawToolbar(GUIContent label)
		{
			Rect rect = GUILayoutUtility.GetRect(0f, 22f);
			bool isRepaint = Event.current.type == EventType.Repaint;
			if (isRepaint)
			{
				SirenixGUIStyles.ToolbarBackground.Draw(rect, GUIContent.none, 0);
			}
			if (!isReadOnly)
			{
				Rect btnRect = rect.AlignRight(23f);
				rect.xMax = btnRect.xMin;
				if (GUI.Button(btnRect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
				{
					bool allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(base.Property);
					OdinObjectSelector.Show(rect, this, TableListDrawerId, null, resolver.ElementType, resolver.ElementType, allowSceneObjects, !base.Property.ValueEntry.SerializationBackend.SupportsPolymorphism, base.Property);
				}
				SdfIcons.DrawIcon(btnRect.AlignCenter(13f), SdfIconType.Plus);
			}
			if (!isReadOnly)
			{
				Rect btnRect2 = rect.AlignRight(23f);
				rect.xMax = btnRect2.xMin;
				if (GUI.Button(btnRect2, GUIContent.none, SirenixGUIStyles.ToolbarButton))
				{
					drawAsList = !drawAsList;
				}
				SdfIcons.DrawIcon(btnRect2.AlignCenter(13f), SdfIconType.ListOl);
			}
			paging.DrawToolbarPagingButtons(ref rect, base.Property.State.Expanded, showItemCount: true);
			if (label == null)
			{
				label = GUIHelper.TempContent("");
			}
			Rect labelRect = rect;
			labelRect.x += 5f;
			labelRect.y += 3f;
			labelRect.height = 16f;
			if (filter.IsUsed)
			{
				Vector2 labelSize = EditorStyles.label.CalcSize(label);
				Rect filterRect = labelRect.TakeFromRight(labelRect.width - labelSize.x - 20f);
				filterRect.width -= 10f;
				filter.Draw(filterRect);
			}
			if (base.Property.Children.Count > 0)
			{
				GUIHelper.PushHierarchyMode(hierarchyMode: false);
				if (base.Attribute.AlwaysExpanded)
				{
					GUI.Label(labelRect, label);
				}
				else
				{
					base.Property.State.Expanded = SirenixEditorGUI.Foldout(labelRect, base.Property.State.Expanded, label);
				}
				GUIHelper.PushHierarchyMode(hierarchyMode: true);
			}
			else if (isRepaint)
			{
				GUI.Label(labelRect, label);
			}
		}

		private void DrawColumnHeaders()
		{
			if (base.Property.Children.Count == 0)
			{
				return;
			}
			columnHeaderRect = GUILayoutUtility.GetRect(0f, 21f);
			columnHeaderRect.height += 1f;
			columnHeaderRect.y -= 1f;
			if (Event.current.type == EventType.Repaint)
			{
				SirenixEditorGUI.DrawBorders(columnHeaderRect, 1);
				EditorGUI.DrawRect(columnHeaderRect, SirenixGUIStyles.ColumnTitleBg);
			}
			float offset = columnHeaderRect.width - table.ContentRect.width;
			columnHeaderRect.width -= offset;
			GUITableUtilities.ResizeColumns(columnHeaderRect, columns);
			if (Event.current.type != EventType.Repaint)
			{
				return;
			}
			GUITableUtilities.DrawColumnHeaderSeperators(columnHeaderRect, columns, SirenixGUIStyles.BorderColor);
			Rect rect = columnHeaderRect;
			for (int i = 0; i < columns.Count; i++)
			{
				Column col = columns[i];
				if (!(rect.x > columnHeaderRect.xMax))
				{
					rect.width = col.ColWidth;
					rect.xMax = Mathf.Min(columnHeaderRect.xMax, rect.xMax);
					if (col.NiceName != null)
					{
						Rect lblRect = rect;
						GUI.Label(lblRect, col.NiceName, SirenixGUIStyles.LabelCentered);
					}
					rect.x += col.ColWidth;
					continue;
				}
				break;
			}
		}

		private void DrawTable()
		{
			GUIHelper.PushHierarchyMode(hierarchyMode: false);
			table.DrawScrollView = base.Attribute.DrawScrollView && (paging.IsExpanded || !paging.IsEnabled);
			table.ScrollPos = scrollPos.Value;
			table.BeginTable(paging.EndIndex - paging.StartIndex);
			AddColumns(table.RowIndexFrom, table.RowIndexTo);
			DrawListItemBackGrounds();
			float currX = 0f;
			for (int i = 0; i < columns.Count; i++)
			{
				Column col = columns[i];
				int colWidth = (int)col.ColWidth;
				if (isFirstFrame && col.PreferWide)
				{
					colWidth = 200;
				}
				table.BeginColumn((int)currX, colWidth);
				GUIHelper.PushLabelWidth((float)colWidth * 0.3f);
				currX += col.ColWidth;
				for (int j = table.RowIndexFrom; j < table.RowIndexTo; j++)
				{
					table.BeginCell(j);
					DrawCell(col, j);
					table.EndCell(j);
				}
				GUIHelper.PopLabelWidth();
				table.EndColumn();
			}
			DrawRightClickContextMenuAreas();
			table.EndTable();
			scrollPos.Value = table.ScrollPos;
			DrawColumnSeperators();
			GUIHelper.PopHierarchyMode();
			if (columns.Count > 0 && columns[0].ColumnType == ColumnType.Index)
			{
				columns[0].ColWidth = indexLabelWidth;
				columns[0].MinWidth = indexLabelWidth;
			}
		}

		private void DrawColumnSeperators()
		{
			if (Event.current.type == EventType.Repaint)
			{
				Color bcol = SirenixGUIStyles.BorderColor;
				bcol.a *= 0.4f;
				Rect r = table.OuterRect;
				GUITableUtilities.DrawColumnHeaderSeperators(r, columns, bcol);
			}
		}

		private void DrawListItemBackGrounds()
		{
			if (Event.current.type == EventType.Repaint)
			{
				for (int i = table.RowIndexFrom; i < table.RowIndexTo; i++)
				{
					Color col = default(Color);
					Rect rect = table.GetRowRect(i);
					col = ((i % 2 == 0) ? SirenixGUIStyles.ListItemColorEven : SirenixGUIStyles.ListItemColorOdd);
					EditorGUI.DrawRect(rect, col);
				}
			}
		}

		private void DrawRightClickContextMenuAreas()
		{
			for (int i = table.RowIndexFrom; i < table.RowIndexTo; i++)
			{
				Rect rect = table.GetRowRect(i);
				base.Property.Children[i].Update();
				PropertyContextMenuDrawer.AddRightClickArea(base.Property.Children[i], rect);
			}
		}

		private void DrawCell(Column col, int rowIndex)
		{
			rowIndex += paging.StartIndex;
			if (col.ColumnType == ColumnType.Index)
			{
				Rect rect = GUILayoutUtility.GetRect(0f, 16f);
				rect.xMin += 5f;
				rect.width -= 2f;
				if (Event.current.type == EventType.Repaint)
				{
					indexLabel.text = rowIndex.ToString();
					GUI.Label(rect, indexLabel, SirenixGUIStyles.Label);
					int labelWidth = (int)SirenixGUIStyles.Label.CalcSize(indexLabel).x;
					indexLabelWidth = Mathf.Max(indexLabelWidth, labelWidth + 15);
				}
			}
			else if (col.ColumnType == ColumnType.DeleteButton)
			{
				Rect rect2 = GUILayoutUtility.GetRect(20f, 20f).AlignCenter(13f, 13f);
				if (SirenixEditorGUI.SDFIconButton(rect2, SdfIconType.X, IconAlignment.LeftOfText, SirenixGUIStyles.IconButton))
				{
					resolver.QueueRemoveAt(filter.GetCollectionIndex(rowIndex));
					filter.Update();
				}
			}
			else
			{
				if (col.ColumnType != ColumnType.Property)
				{
					throw new NotImplementedException(col.ColumnType.ToString());
				}
				filter[rowIndex].Children[col.Name]?.Draw(null);
			}
		}

		private IEnumerable<InspectorProperty> EnumerateGroupMembers(InspectorProperty groupProperty)
		{
			for (int i = 0; i < groupProperty.Children.Count; i++)
			{
				InspectorPropertyInfo info = groupProperty.Children[i].Info;
				if (info.PropertyType != PropertyType.Group)
				{
					yield return groupProperty.Children[i];
					continue;
				}
				foreach (InspectorProperty item in EnumerateGroupMembers(groupProperty.Children[i]))
				{
					yield return item;
				}
			}
		}

		private T GetColumnAttribute<T>(InspectorProperty col) where T : Attribute
		{
			if (col.Info.PropertyType == PropertyType.Group)
			{
				return (from c in EnumerateGroupMembers(col)
					select c.GetAttribute<T>()).FirstOrDefault((T c) => c != null);
			}
			return col.GetAttribute<T>();
		}

		public void Dispose()
		{
			filter?.Dispose();
		}
	}
}
