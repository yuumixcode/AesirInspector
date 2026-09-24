using System;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[HideReferenceObjectPicker]
	[InlineProperty]
	public abstract class EditorPref<T>
	{
		private bool initialized;

		private T defaultValue;

		private T value;

		[HideInInspector]
		public readonly string Key;

		private static Func<T, T, bool> comparer;

		[ShowInInspector]
		[HideLabel]
		public T Value
		{
			get
			{
				EnsureInitialized();
				return value;
			}
			set
			{
				EnsureInitialized();
				if (!Equals(this.value, value))
				{
					this.value = value;
					SetValue(Key, value);
				}
			}
		}

		public EditorPref(string key, T defaultValue)
		{
			Key = key;
			this.defaultValue = defaultValue;
		}

		protected virtual bool Equals(T a, T b)
		{
			if (comparer == null)
			{
				comparer = TypeExtensions.GetEqualityComparerDelegate<T>();
			}
			return comparer(a, b);
		}

		protected abstract T GetValue(string key, T defaultValue);

		protected abstract void SetValue(string key, T value);

		private void EnsureInitialized()
		{
			if (!initialized)
			{
				value = GetValue(Key, defaultValue);
				initialized = true;
			}
		}

		public static implicit operator T(EditorPref<T> editorPref)
		{
			return editorPref.Value;
		}
	}
}
