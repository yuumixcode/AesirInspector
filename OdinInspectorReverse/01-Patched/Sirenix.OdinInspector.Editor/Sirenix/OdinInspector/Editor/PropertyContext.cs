using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>A contextual value attached to an <see cref="T:Sirenix.OdinInspector.Editor.InspectorProperty" />, mapped to a key, contained in a <see cref="T:Sirenix.OdinInspector.Editor.PropertyContextContainer" />.</para>
	/// </summary>
	public sealed class PropertyContext<T>
	{
		/// <summary>
		/// The contained value.
		/// </summary>
		public T Value;

		private PropertyContext()
		{
		}

		/// <summary>
		/// Creates a new PropertyContext.
		/// </summary>
		public static PropertyContext<T> Create()
		{
			return new PropertyContext<T>();
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="T:Sirenix.OdinInspector.Editor.PropertyContext`1" /> to <see cref="!:T" />.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns>
		/// The result of the conversion.
		/// </returns>
		public static explicit operator T(PropertyContext<T> context)
		{
			if (context == null)
			{
				return default(T);
			}
			return context.Value;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String" /> that represents this instance, of the format "<see cref="T:Sirenix.OdinInspector.Editor.PropertyContext`1" />: Value.ToString()".
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			string niceName = GetType().GetNiceName();
			T value = Value;
			return niceName + ": " + value;
		}
	}
}
