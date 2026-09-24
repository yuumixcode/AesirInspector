using System;
using System.Diagnostics;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// The TableColumnWidth attribute is used to further customize the width of a column in tables drawn using the <see cref="T:Sirenix.OdinInspector.TableListAttribute" />.
	/// </summary>
	/// <example>
	/// <code>
	/// [TableList]
	/// public List&lt;SomeType&gt; TableList = new List&lt;SomeType&gt;();
	///
	/// [Serializable]
	/// public class SomeType
	/// {
	///     [LabelWidth(30)]
	///     [TableColumnWidth(130, false)]
	///     [VerticalGroup("Combined")]
	///     public string A;
	///
	///     [LabelWidth(30)]
	///     [VerticalGroup("Combined")]
	///     public string B;
	///
	///     [Multiline(2), Space(3)]
	///     public string fields;
	/// }
	/// </code>
	/// </example>
	/// <seealso cref="T:Sirenix.OdinInspector.TableListAttribute" />
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	[Conditional("UNITY_EDITOR")]
	public class TableColumnWidthAttribute : Attribute
	{
		/// <summary>
		/// The width of the column.
		/// </summary>
		public int Width;

		/// <summary>
		/// Whether the column should be resizable. True by default.
		/// </summary>
		public bool Resizable = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.TableColumnWidthAttribute" /> class.
		/// </summary>
		/// <param name="width">The width of the column in pixels.</param>
		/// <param name="resizable">If <c>true</c> then the column can be resized in the inspector.</param>
		public TableColumnWidthAttribute(int width, bool resizable = true)
		{
			Width = width;
			Resizable = resizable;
		}
	}
}
