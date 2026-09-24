using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.Utilities
{
	/// <summary>
	/// <para>
	/// GUILayoutOptions is a handy utility that provides cached GUILayoutOpion arrays based on the wanted parameters.
	/// </para>
	/// </summary>
	/// <example>
	/// <para>
	/// Most GUILayout and EditorGUILayout methods takes an optional "params GUILayoutOption[]" parameter.
	/// Each time you call this, an array is allocated generating garbage.
	/// </para>
	/// <code>
	/// // Generates garbage:
	/// GUILayout.Label(label, GUILayout.Label(label, GUILayout.Width(20), GUILayout.ExpandHeight(), GUILayout.MaxWidth(300)));
	///
	/// // Does not generate garbage:
	/// GUILayout.Label(label, GUILayout.Label(label, GUILayoutOptions.Width(20).ExpandHeight().MaxWidth(300)));
	/// </code>
	/// </example>
	public static class GUILayoutOptions
	{
		internal enum GUILayoutOptionType
		{
			Width,
			Height,
			MinWidth,
			MaxHeight,
			MaxWidth,
			MinHeight,
			ExpandHeight,
			ExpandWidth
		}

		/// <summary>
		/// A GUILayoutOptions instance with an implicit operator to be converted to a GUILayoutOption[] array.
		/// </summary>
		/// <seealso cref="T:Sirenix.Utilities.GUILayoutOptions" />
		public sealed class GUILayoutOptionsInstance : IEquatable<GUILayoutOptionsInstance>
		{
			private float value;

			internal GUILayoutOptionsInstance Parent;

			internal GUILayoutOptionType GUILayoutOptionType;

			private GUILayoutOption[] GetCachedOptions()
			{
				if (!GUILayoutOptionsCache.TryGetValue(this, out var value))
				{
					return GUILayoutOptionsCache[Clone()] = CreateOptionsArary();
				}
				return value;
			}

			/// <summary>
			/// Gets or creates the cached GUILayoutOption array based on the layout options specified.
			/// </summary>
			public static implicit operator GUILayoutOption[](GUILayoutOptionsInstance options)
			{
				return options.GetCachedOptions();
			}

			private GUILayoutOption[] CreateOptionsArary()
			{
				List<GUILayoutOption> options = new List<GUILayoutOption>();
				for (GUILayoutOptionsInstance curr = this; curr != null; curr = curr.Parent)
				{
					switch (curr.GUILayoutOptionType)
					{
					case GUILayoutOptionType.Width:
						options.Add(GUILayout.Width(curr.value));
						break;
					case GUILayoutOptionType.Height:
						options.Add(GUILayout.Height(curr.value));
						break;
					case GUILayoutOptionType.MaxHeight:
						options.Add(GUILayout.MaxHeight(curr.value));
						break;
					case GUILayoutOptionType.MaxWidth:
						options.Add(GUILayout.MaxWidth(curr.value));
						break;
					case GUILayoutOptionType.MinHeight:
						options.Add(GUILayout.MinHeight(curr.value));
						break;
					case GUILayoutOptionType.MinWidth:
						options.Add(GUILayout.MinWidth(curr.value));
						break;
					case GUILayoutOptionType.ExpandHeight:
						options.Add(GUILayout.ExpandHeight(curr.value > 0.2f));
						break;
					case GUILayoutOptionType.ExpandWidth:
						options.Add(GUILayout.ExpandWidth(curr.value > 0.2f));
						break;
					}
				}
				return options.ToArray();
			}

			private GUILayoutOptionsInstance Clone()
			{
				GUILayoutOptionsInstance result = null;
				result = new GUILayoutOptionsInstance
				{
					value = value,
					GUILayoutOptionType = GUILayoutOptionType
				};
				GUILayoutOptionsInstance currResult = result;
				GUILayoutOptionsInstance curr = Parent;
				while (curr != null)
				{
					currResult.Parent = new GUILayoutOptionsInstance
					{
						value = curr.value,
						GUILayoutOptionType = curr.GUILayoutOptionType
					};
					curr = curr.Parent;
					currResult = currResult.Parent;
				}
				return result;
			}

			internal GUILayoutOptionsInstance()
			{
			}

			/// <summary>
			/// Option passed to a control to give it an absolute width.
			/// </summary>
			public GUILayoutOptionsInstance Width(float width)
			{
				GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
				instance.SetValue(GUILayoutOptionType.Width, width);
				return instance;
			}

			/// <summary>
			/// Option passed to a control to give it an absolute height.
			/// </summary>
			public GUILayoutOptionsInstance Height(float height)
			{
				GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
				instance.SetValue(GUILayoutOptionType.Height, height);
				return instance;
			}

			/// <summary>
			/// Option passed to a control to specify a maximum height.
			/// </summary>
			public GUILayoutOptionsInstance MaxHeight(float height)
			{
				GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
				instance.SetValue(GUILayoutOptionType.MaxHeight, height);
				return instance;
			}

			/// <summary>
			/// Option passed to a control to specify a maximum width.
			/// </summary>
			public GUILayoutOptionsInstance MaxWidth(float width)
			{
				GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
				instance.SetValue(GUILayoutOptionType.MaxWidth, width);
				return instance;
			}

			/// <summary>
			/// Option passed to a control to specify a minimum height.
			/// </summary>
			public GUILayoutOptionsInstance MinHeight(float height)
			{
				GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
				instance.SetValue(GUILayoutOptionType.MinHeight, height);
				return instance;
			}

			/// <summary>
			/// Option passed to a control to specify a minimum width.
			/// </summary>
			public GUILayoutOptionsInstance MinWidth(float width)
			{
				GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
				instance.SetValue(GUILayoutOptionType.MinWidth, width);
				return instance;
			}

			/// <summary>
			/// Option passed to a control to allow or disallow vertical expansion.
			/// </summary>
			public GUILayoutOptionsInstance ExpandHeight(bool expand = true)
			{
				GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
				instance.SetValue(GUILayoutOptionType.ExpandHeight, expand);
				return instance;
			}

			/// <summary>
			/// Option passed to a control to allow or disallow horizontal expansion.
			/// </summary>
			public GUILayoutOptionsInstance ExpandWidth(bool expand = true)
			{
				GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
				instance.SetValue(GUILayoutOptionType.ExpandWidth, expand);
				return instance;
			}

			internal void SetValue(GUILayoutOptionType type, float value)
			{
				GUILayoutOptionType = type;
				this.value = value;
			}

			internal void SetValue(GUILayoutOptionType type, bool value)
			{
				GUILayoutOptionType = type;
				this.value = (value ? 1 : 0);
			}

			/// <summary>
			/// Determines whether the instance is equals another instance.
			/// </summary>
			public bool Equals(GUILayoutOptionsInstance other)
			{
				GUILayoutOptionsInstance currA = this;
				GUILayoutOptionsInstance currB = other;
				while (currA != null && currB != null)
				{
					if (currA.GUILayoutOptionType != currB.GUILayoutOptionType || currA.value != currB.value)
					{
						return false;
					}
					currA = currA.Parent;
					currB = currB.Parent;
				}
				if (currB != null || currA != null)
				{
					return false;
				}
				return true;
			}

			/// <summary>
			/// Returns a hash code for this instance.
			/// </summary>
			public override int GetHashCode()
			{
				int count = 0;
				int hash = 17;
				for (GUILayoutOptionsInstance curr = this; curr != null; curr = curr.Parent)
				{
					hash = hash * 29 + GUILayoutOptionType.GetHashCode() + value.GetHashCode() * 17 + count++;
				}
				return hash;
			}
		}

		private static int CurrentCacheIndex;

		private static readonly GUILayoutOptionsInstance[] GUILayoutOptionsInstanceCache;

		private static readonly Dictionary<GUILayoutOptionsInstance, GUILayoutOption[]> GUILayoutOptionsCache;

		/// <summary>
		/// An EmptyGUIOption[] array with a length of 0.
		/// </summary>
		public static readonly GUILayoutOption[] EmptyGUIOptions;

		static GUILayoutOptions()
		{
			CurrentCacheIndex = 0;
			GUILayoutOptionsCache = new Dictionary<GUILayoutOptionsInstance, GUILayoutOption[]>();
			EmptyGUIOptions = new GUILayoutOption[0];
			GUILayoutOptionsInstanceCache = new GUILayoutOptionsInstance[30];
			GUILayoutOptionsInstanceCache[0] = new GUILayoutOptionsInstance();
			for (int i = 1; i < 30; i++)
			{
				GUILayoutOptionsInstanceCache[i] = new GUILayoutOptionsInstance();
				GUILayoutOptionsInstanceCache[i].Parent = GUILayoutOptionsInstanceCache[i - 1];
			}
		}

		/// <summary>
		/// Option passed to a control to give it an absolute width.
		/// </summary>
		public static GUILayoutOptionsInstance Width(float width)
		{
			CurrentCacheIndex = 0;
			GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
			instance.SetValue(GUILayoutOptionType.Width, width);
			return instance;
		}

		/// <summary>
		/// Option passed to a control to give it an absolute height.
		/// </summary>
		public static GUILayoutOptionsInstance Height(float height)
		{
			CurrentCacheIndex = 0;
			GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
			instance.SetValue(GUILayoutOptionType.Height, height);
			return instance;
		}

		/// <summary>
		/// Option passed to a control to specify a maximum height.
		/// </summary>
		public static GUILayoutOptionsInstance MaxHeight(float height)
		{
			CurrentCacheIndex = 0;
			GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
			instance.SetValue(GUILayoutOptionType.MaxHeight, height);
			return instance;
		}

		/// <summary>
		/// Option passed to a control to specify a maximum width.
		/// </summary>
		public static GUILayoutOptionsInstance MaxWidth(float width)
		{
			CurrentCacheIndex = 0;
			GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
			instance.SetValue(GUILayoutOptionType.MaxWidth, width);
			return instance;
		}

		/// <summary>
		/// Option passed to a control to specify a minimum width.
		/// </summary>
		public static GUILayoutOptionsInstance MinWidth(float width)
		{
			CurrentCacheIndex = 0;
			GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
			instance.SetValue(GUILayoutOptionType.MinWidth, width);
			return instance;
		}

		/// <summary>
		/// Option passed to a control to specify a minimum height.
		/// </summary>
		public static GUILayoutOptionsInstance MinHeight(float height)
		{
			CurrentCacheIndex = 0;
			GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
			instance.SetValue(GUILayoutOptionType.MinHeight, height);
			return instance;
		}

		/// <summary>
		/// Option passed to a control to allow or disallow vertical expansion.
		/// </summary>
		public static GUILayoutOptionsInstance ExpandHeight(bool expand = true)
		{
			CurrentCacheIndex = 0;
			GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
			instance.SetValue(GUILayoutOptionType.ExpandHeight, expand);
			return instance;
		}

		/// <summary>
		/// Option passed to a control to allow or disallow horizontal expansion.
		/// </summary>
		public static GUILayoutOptionsInstance ExpandWidth(bool expand = true)
		{
			CurrentCacheIndex = 0;
			GUILayoutOptionsInstance instance = GUILayoutOptionsInstanceCache[CurrentCacheIndex++];
			instance.SetValue(GUILayoutOptionType.ExpandWidth, expand);
			return instance;
		}
	}
}
