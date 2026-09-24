namespace Sirenix.OdinInspector.Editor
{
	public interface IResizableColumn
	{
		/// <summary>
		/// Gets or sets the width of the col.
		/// </summary>
		float ColWidth { get; set; }

		/// <summary>
		/// Gets or sets the minimum width.
		/// </summary>
		float MinWidth { get; }

		/// <summary>
		/// Gets a value indicating whether the width should be preserved when the table itself gets resiszed.
		/// </summary>
		bool PreserveWidth { get; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="T:Sirenix.OdinInspector.Editor.IResizableColumn" /> is resizable.
		/// </summary>
		bool Resizable { get; }
	}
}
