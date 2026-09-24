using System;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// A helper class to control paging of n number of elements in various situations.
	/// </summary>
	public class GUIPagingHelper
	{
		private Rect prevRect;

		private bool isEnabled = true;

		private int elementCount;

		[SerializeField]
		private int currentPage;

		private int startIndex;

		private int endIndex;

		private int pageCount;

		private int numberOfItemsPrPage;

		private int? nextPageNumber;

		private bool? nextIsExpanded;

		/// <summary>
		/// Disables the paging, and show all elements.
		/// </summary>
		[SerializeField]
		public bool IsExpanded;

		/// <summary>
		/// Gets or sets a value indicating whether this instance is enabled.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
		/// </value>
		public bool IsEnabled
		{
			get
			{
				return isEnabled;
			}
			set
			{
				isEnabled = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is on the frist page.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is on frist page; otherwise, <c>false</c>.
		/// </value>
		public bool IsOnFirstPage => currentPage == 0;

		/// <summary>
		/// Gets a value indicating whether this instance is on the last page.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is on last page; otherwise, <c>false</c>.
		/// </value>
		public bool IsOnLastPage => currentPage == pageCount - 1;

		/// <summary>
		/// Gets or sets the number of items per page.
		/// </summary>
		/// <value>
		/// The number of items pr page.
		/// </value>
		public int NumberOfItemsPerPage
		{
			get
			{
				return numberOfItemsPrPage;
			}
			set
			{
				numberOfItemsPrPage = Mathf.Max(value, 0);
			}
		}

		/// <summary>
		/// Gets or sets the current page.
		/// </summary>
		/// <value>
		/// The current page.
		/// </value>
		public int CurrentPage
		{
			get
			{
				return currentPage;
			}
			set
			{
				currentPage = Mathf.Clamp(value, 0, PageCount - 1);
			}
		}

		/// <summary>
		/// Gets the start index.
		/// </summary>
		/// <value>
		/// The start index.
		/// </value>
		public int StartIndex
		{
			get
			{
				if (IsExpanded)
				{
					return 0;
				}
				return startIndex;
			}
		}

		/// <summary>
		/// Gets the end index.
		/// </summary>
		/// <value>
		/// The end index.
		/// </value>
		public int EndIndex
		{
			get
			{
				if (IsExpanded)
				{
					return elementCount;
				}
				return endIndex;
			}
		}

		/// <summary>
		/// Gets or sets the page count.
		/// </summary>
		/// <value>
		/// The page count.
		/// </value>
		public int PageCount => pageCount;

		/// <summary>
		/// Gets the total number of elements.
		/// Use <see cref="M:Sirenix.Utilities.Editor.GUIPagingHelper.Update(System.Int32)" /> to change the value.
		/// </summary>
		public int ElementCount => elementCount;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.Utilities.Editor.GUIPagingHelper" /> class.
		/// </summary>
		public GUIPagingHelper()
		{
			numberOfItemsPrPage = 1;
		}

		/// <summary>
		/// Updates all values based on <paramref name="elementCount" /> and <see cref="!:NumberOfItemsPrPage" />.
		/// </summary>
		/// <remarks>
		/// Call update right before using <see cref="P:Sirenix.Utilities.Editor.GUIPagingHelper.StartIndex" /> and <see cref="P:Sirenix.Utilities.Editor.GUIPagingHelper.EndIndex" /> in your for loop.
		/// </remarks>
		/// <param name="elementCount">The total number of elements to apply paging for.</param>
		public void Update(int elementCount)
		{
			if (elementCount < 0)
			{
				throw new ArgumentOutOfRangeException("Non-negative number required.");
			}
			this.elementCount = elementCount;
			if (isEnabled)
			{
				pageCount = Mathf.Max(1, Mathf.CeilToInt((float)this.elementCount / (float)numberOfItemsPrPage));
				currentPage = Mathf.Clamp(currentPage, 0, pageCount - 1);
				startIndex = currentPage * numberOfItemsPrPage;
				endIndex = Mathf.Min(this.elementCount, startIndex + numberOfItemsPrPage);
			}
			else
			{
				startIndex = 0;
				endIndex = this.elementCount;
			}
			if (Event.current.type == EventType.Layout)
			{
				if (nextPageNumber.HasValue)
				{
					currentPage = nextPageNumber.Value;
					nextPageNumber = null;
				}
				if (nextIsExpanded.HasValue)
				{
					IsExpanded = nextIsExpanded.Value;
					nextIsExpanded = null;
				}
			}
		}

		/// <summary>
		/// Draws right-aligned toolbar paging buttons.
		/// </summary>
		public void DrawToolbarPagingButtons(ref Rect toolbarRect, bool showPaging, bool showItemCount, int btnWidth = 23)
		{
			if (prevRect.height == 0f)
			{
				if (Event.current.type == EventType.Repaint)
				{
					prevRect = toolbarRect;
				}
				return;
			}
			bool drawPaging = isEnabled && !IsExpanded && showPaging && pageCount > 1;
			bool drawExpand = isEnabled && pageCount > 1;
			bool drawPagingField = drawPaging;
			if (drawExpand)
			{
				Rect btnRect = toolbarRect.AlignRight(btnWidth, clamp: true);
				toolbarRect.xMax = btnRect.xMin;
				if (GUI.Button(btnRect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
				{
					GUIHelper.RemoveFocusControl();
					nextIsExpanded = !IsExpanded;
				}
				(IsExpanded ? EditorIcons.TriangleUp : EditorIcons.TriangleDown).Draw(btnRect, 16f);
			}
			if (drawPaging)
			{
				Rect btnRect2 = toolbarRect.AlignRight(btnWidth, clamp: true);
				if (GUI.Button(btnRect2, GUIContent.none, SirenixGUIStyles.ToolbarButton))
				{
					GUIHelper.RemoveFocusControl();
					if (Event.current.button == 1)
					{
						nextPageNumber = PageCount - 1;
					}
					else
					{
						nextPageNumber = currentPage + 1;
						if (nextPageNumber >= pageCount)
						{
							nextPageNumber = 0;
						}
					}
				}
				EditorIcons.TriangleRight.Draw(btnRect2, 16f);
				toolbarRect.xMax = btnRect2.xMin;
			}
			if (drawPagingField)
			{
				string pageCountLbl = "/ " + PageCount;
				float lblLength = SirenixGUIStyles.Label.CalcSize(new GUIContent(pageCountLbl)).x;
				Rect lblRect = toolbarRect.AlignRight(lblLength + 5f, clamp: true);
				toolbarRect.xMax = lblRect.xMin;
				Rect fldRect = toolbarRect.AlignRight(lblLength, clamp: true);
				toolbarRect.xMax = fldRect.xMin;
				fldRect.xMin += 4f;
				fldRect.y -= 1f;
				GUI.Label(lblRect, pageCountLbl, SirenixGUIStyles.LabelCentered);
				int next = SirenixEditorGUI.SlideRectInt(lblRect, 0, CurrentPage);
				if (next != CurrentPage)
				{
					nextPageNumber = next;
				}
				next = EditorGUI.IntField(fldRect.AlignCenterY(15f), CurrentPage + 1) - 1;
				if (next != CurrentPage)
				{
					nextPageNumber = next;
				}
			}
			if (drawPaging)
			{
				Rect btnRect3 = toolbarRect.AlignRight(btnWidth, clamp: true);
				if (GUI.Button(btnRect3, GUIContent.none, SirenixGUIStyles.ToolbarButton))
				{
					GUIHelper.RemoveFocusControl();
					if (Event.current.button == 1)
					{
						nextPageNumber = 0;
					}
					else
					{
						nextPageNumber = currentPage - 1;
						if (nextPageNumber < 0)
						{
							nextPageNumber = pageCount - 1;
						}
					}
				}
				EditorIcons.TriangleLeft.Draw(btnRect3, 16f);
				toolbarRect.xMax = btnRect3.xMin;
			}
			if (showItemCount && Event.current.type != EventType.Layout)
			{
				GUIContent lbl = new GUIContent((ElementCount == 0) ? "Empty" : (ElementCount + " items"));
				float width = SirenixGUIStyles.LeftAlignedGreyMiniLabel.CalcSize(lbl).x + 5f;
				Rect lblRect2 = toolbarRect.AlignRight(width);
				GUI.Label(lblRect2, lbl, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
				toolbarRect.xMax = lblRect2.xMin;
			}
			if (Event.current.type == EventType.Repaint)
			{
				prevRect = toolbarRect;
			}
		}
	}
}
