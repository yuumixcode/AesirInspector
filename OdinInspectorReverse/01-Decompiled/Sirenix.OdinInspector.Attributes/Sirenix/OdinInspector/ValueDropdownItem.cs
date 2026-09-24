namespace Sirenix.OdinInspector
{
	/// <summary>
	///
	/// </summary>
	public struct ValueDropdownItem : IValueDropdownItem
	{
		/// <summary>
		/// The name of the item.
		/// </summary>
		public string Text;

		/// <summary>
		/// The value of the item.
		/// </summary>
		public object Value;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.ValueDropdownItem`1" /> class.
		/// </summary>
		/// <param name="text">The text to display for the dropdown item.</param>
		/// <param name="value">The value for the dropdown item.</param>
		public ValueDropdownItem(string text, object value)
		{
			Text = text;
			Value = value;
		}

		/// <summary>
		/// The name of this item.
		/// </summary>
		public override string ToString()
		{
			return Text ?? Value?.ToString() ?? "";
		}

		/// <summary>
		/// Gets the text.
		/// </summary>
		string IValueDropdownItem.GetText()
		{
			return Text;
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		object IValueDropdownItem.GetValue()
		{
			return Value;
		}
	}
	/// <summary>
	///
	/// </summary>
	public struct ValueDropdownItem<T> : IValueDropdownItem
	{
		/// <summary>
		/// The name of the item.
		/// </summary>
		public string Text;

		/// <summary>
		/// The value of the item.
		/// </summary>
		public T Value;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.ValueDropdownItem`1" /> class.
		/// </summary>
		/// <param name="text">The text to display for the dropdown item.</param>
		/// <param name="value">The value for the dropdown item.</param>
		public ValueDropdownItem(string text, T value)
		{
			Text = text;
			Value = value;
		}

		/// <summary>
		/// Gets the text.
		/// </summary>
		string IValueDropdownItem.GetText()
		{
			return Text;
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		object IValueDropdownItem.GetValue()
		{
			return Value;
		}

		/// <summary>
		/// The name of this item.
		/// </summary>
		public override string ToString()
		{
			object obj = Text;
			if (obj == null)
			{
				T value = Value;
				obj = value?.ToString() ?? "";
			}
			return (string)obj;
		}
	}
}
