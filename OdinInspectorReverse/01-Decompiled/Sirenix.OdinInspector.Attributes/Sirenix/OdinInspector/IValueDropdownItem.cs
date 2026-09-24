namespace Sirenix.OdinInspector
{
	/// <summary>
	///
	/// </summary>
	public interface IValueDropdownItem
	{
		/// <summary>
		/// Gets the label for the dropdown item.
		/// </summary>
		/// <returns>The label text for the item.</returns>
		string GetText();

		/// <summary>
		/// Gets the value of the dropdown item.
		/// </summary>
		/// <returns>The value for the item.</returns>
		object GetValue();
	}
}
