using System.Collections.Generic;

namespace Sirenix.OdinInspector
{
	/// <summary>
	/// Use this with <see cref="T:Sirenix.OdinInspector.ValueDropdownAttribute" /> to specify custom names for values.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	public class ValueDropdownList<T> : List<ValueDropdownItem<T>>
	{
		/// <summary>
		/// Adds the specified value with a custom name.
		/// </summary>
		/// <param name="text">The name of the item.</param>
		/// <param name="value">The value.</param>
		public void Add(string text, T value)
		{
			Add(new ValueDropdownItem<T>(text, value));
		}

		/// <summary>
		/// Adds the specified value.
		/// </summary>
		/// <param name="value">The value.</param>
		public void Add(T value)
		{
			Add(new ValueDropdownItem<T>(value.ToString(), value));
		}
	}
}
