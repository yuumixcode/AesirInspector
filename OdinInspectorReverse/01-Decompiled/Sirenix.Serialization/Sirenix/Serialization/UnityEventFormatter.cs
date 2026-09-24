using UnityEngine.Events;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Custom generic formatter for the <see cref="T:UnityEngine.Events.UnityEvent`1" />, <see cref="T:UnityEngine.Events.UnityEvent`2" />, <see cref="T:UnityEngine.Events.UnityEvent`3" /> and <see cref="T:UnityEngine.Events.UnityEvent`4" /> types.
	/// </summary>
	/// <typeparam name="T">The type of UnityEvent that this formatter can serialize and deserialize.</typeparam>
	/// <seealso cref="!:ReflectionFormatter&lt;UnityEngine.Events.UnityEvent&gt;" />
	public class UnityEventFormatter<T> : ReflectionFormatter<T> where T : UnityEventBase, new()
	{
		/// <summary>
		/// Get an uninitialized object of type <see cref="!:T" />.
		/// </summary>
		/// <returns>
		/// An uninitialized object of type <see cref="!:T" />.
		/// </returns>
		protected override T GetUninitializedObject()
		{
			return new T();
		}
	}
}
