using System;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Used in <see cref="T:Sirenix.OdinInspector.Editor.GenericSelector`1" /> to associate name to a value in the GenericSelector.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct GenericSelectorItem<T>
	{
		/// <summary>
		/// The value.
		/// </summary>
		public T Value;

		/// <summary>
		/// The name.
		/// </summary>
		public string Name;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Sirenix.OdinInspector.Editor.GenericSelectorItem`1" /> struct.
		/// </summary>
		public GenericSelectorItem(string name, T value)
		{
			Name = name;
			Value = value;
		}

		/// <summary>
		/// If the
		/// </summary>
		public string GetNiceName()
		{
			if (Name != null)
			{
				return Name;
			}
			Type t = Value as Type;
			if (t != null)
			{
				return t.GetNiceName();
			}
			if (Value != null)
			{
				T value = Value;
				return value?.ToString() ?? "";
			}
			return "Null";
		}
	}
}
