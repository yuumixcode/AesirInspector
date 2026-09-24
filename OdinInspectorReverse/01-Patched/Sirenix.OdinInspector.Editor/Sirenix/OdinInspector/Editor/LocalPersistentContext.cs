using System;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Helper class that provides a local copy of a <see cref="T:Sirenix.OdinInspector.Editor.GlobalPersistentContext`1" />.
	/// When the local value is changed, it also changed the global value, but the global value does not change the local value.
	/// </summary>
	/// <typeparam name="T">The type of the context value.</typeparam>
	public sealed class LocalPersistentContext<T> : ILocalPersistentContext
	{
		private GlobalPersistentContext<T> context;

		private T localValue;

		private static readonly Func<T, T, bool> Comparer = PropertyValueEntry<T>.EqualityComparer;

		private static Type TypeOf_T = typeof(T);

		/// <summary>
		/// The value of the context.
		/// Changing this value, also changes the global context value, but the global value does not change the local value.
		/// </summary>
		public T Value
		{
			get
			{
				return localValue;
			}
			set
			{
				if (!Comparer(localValue, value))
				{
					context.Value = value;
					localValue = value;
				}
			}
		}

		Type ILocalPersistentContext.Type => TypeOf_T;

		object ILocalPersistentContext.WeakValue
		{
			get
			{
				return Value;
			}
			set
			{
				Value = (T)value;
			}
		}

		private LocalPersistentContext(GlobalPersistentContext<T> global)
		{
			if (global == null)
			{
				throw new ArgumentNullException("global");
			}
			context = global;
			localValue = context.Value;
		}

		/// <summary>
		/// Creates a local context object for the provided global context.
		/// </summary>
		/// <param name="global">The global context object.</param>
		public static LocalPersistentContext<T> Create(GlobalPersistentContext<T> global)
		{
			return new LocalPersistentContext<T>(global);
		}

		/// <summary>
		/// Updates the local value to the current global value.
		/// </summary>
		public void UpdateLocalValue()
		{
			localValue = context.Value;
		}
	}
}
