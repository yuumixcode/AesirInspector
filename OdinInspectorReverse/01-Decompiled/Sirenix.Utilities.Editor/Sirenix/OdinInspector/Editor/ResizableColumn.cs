namespace Sirenix.OdinInspector.Editor
{
	public class ResizableColumn : IResizableColumn
	{
		public float ColWidth;

		public float MinWidth;

		public bool PreserveWidth;

		public bool Resizable = true;

		bool IResizableColumn.Resizable => Resizable;

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

		bool IResizableColumn.PreserveWidth => PreserveWidth;

		public static ResizableColumn FixedColumn(float width)
		{
			return new ResizableColumn
			{
				ColWidth = width,
				PreserveWidth = true,
				MinWidth = width,
				Resizable = false
			};
		}

		public static ResizableColumn FlexibleColumn(float width = 0f, float minWidth = 0f)
		{
			return new ResizableColumn
			{
				ColWidth = width,
				PreserveWidth = true,
				MinWidth = minWidth,
				Resizable = true
			};
		}

		public static ResizableColumn DynamicColumn(float width = 0f, float minWidth = 0f)
		{
			return new ResizableColumn
			{
				ColWidth = width,
				PreserveWidth = false,
				MinWidth = minWidth,
				Resizable = true
			};
		}
	}
}
