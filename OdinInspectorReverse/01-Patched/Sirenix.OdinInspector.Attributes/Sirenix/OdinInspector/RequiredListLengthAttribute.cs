using System;

namespace Sirenix.OdinInspector
{
	public sealed class RequiredListLengthAttribute : Attribute
	{
		private PrefabKind prefabKind;

		private bool prefabKindIsSet;

		private int minLength;

		private int maxLength;

		private bool minLengthIsSet;

		private bool maxLengthIsSet;

		/// <summary>
		/// A C# expression for getting the minimum length of the collection, for example "@this.otherList.Count".
		/// If set, MinLength will be the fallback in case nothing in case MinLengthGetter returns null.
		/// </summary>
		public string MinLengthGetter;

		/// <summary>
		/// A C# expression for getting the maximum length of the collection, for example "@this.otherList.Count".
		/// If set, MaxLength will be the fallback in case nothing in case MaxLengthGetter returns null.
		/// </summary>
		public string MaxLengthGetter;

		/// <summary>
		/// The minimum length of the collection. If not set, there is no minimum length restriction.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "minLength", "minLengthIsSet" })]
		public int MinLength
		{
			get
			{
				return minLength;
			}
			set
			{
				minLength = value;
				minLengthIsSet = true;
			}
		}

		/// <summary>
		/// The maximum length of the collection. If not set, there is no maximum length restriction.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "maxLength", "maxLengthIsSet" })]
		public int MaxLength
		{
			get
			{
				return maxLength;
			}
			set
			{
				maxLength = value;
				maxLengthIsSet = true;
			}
		}

		public bool MinLengthIsSet => minLengthIsSet;

		public bool MaxLengthIsSet => maxLengthIsSet;

		public bool PrefabKindIsSet => prefabKindIsSet;

		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "prefabKind", "prefabKindIsSet" })]
		public PrefabKind PrefabKind
		{
			get
			{
				return prefabKind;
			}
			set
			{
				prefabKind = value;
				prefabKindIsSet = true;
			}
		}

		/// <summary>
		/// Limits the collection to be contain the specified number of elements.
		/// </summary>
		public RequiredListLengthAttribute()
		{
		}

		/// <summary>
		/// Limits the collection to be contain the specified number of elements.
		/// </summary>
		/// <param name="fixedLength">The minimum and maximum length of the collection.</param>
		public RequiredListLengthAttribute(int fixedLength)
		{
			MinLength = fixedLength;
			MaxLength = fixedLength;
		}

		/// <summary>
		/// Limits the collection to be contain the specified number of elements.
		/// </summary>
		/// <param name="minLength">The minimum length of the collection.</param>
		/// <param name="maxLength">The maximum length of the collection.</param>
		public RequiredListLengthAttribute(int minLength, int maxLength)
		{
			MinLength = minLength;
			MaxLength = maxLength;
		}

		/// <summary>
		/// Limits the collection to be contain the specified number of elements.
		/// </summary>
		/// <param name="minLength">The minimum length of the collection.</param>
		/// <param name="maxLengthGetter">A C# expression for getting the maximum length of the collection, for example "@this.otherList.Count". If set, MaxLength will be the fallback in case nothing in case MaxLengthGetter returns null.</param>
		public RequiredListLengthAttribute(int minLength, string maxLengthGetter)
		{
			MinLength = minLength;
			MaxLengthGetter = maxLengthGetter;
		}

		/// <summary>
		/// Limits the collection to be contain the specified number of elements.
		/// </summary>
		/// <param name="fixedLengthGetter">The minimum and maximum length of the collection.</param>
		public RequiredListLengthAttribute(string fixedLengthGetter)
		{
			MinLengthGetter = fixedLengthGetter;
			MaxLengthGetter = fixedLengthGetter;
		}

		/// <summary>
		/// Limits the collection to be contain the specified number of elements.
		/// </summary>
		/// <param name="minLengthGetter">A C# expression for getting the minimum length of the collection, for example "@this.otherList.Count". If set, MinLength will be the fallback in case nothing in case MinLengthGetter returns null.</param>
		/// <param name="maxLengthGetter">A C# expression for getting the maximum length of the collection, for example "@this.otherList.Count". If set, MaxLength will be the fallback in case nothing in case MaxLengthGetter returns null.</param>
		public RequiredListLengthAttribute(string minLengthGetter, string maxLengthGetter)
		{
			MinLengthGetter = minLengthGetter;
			MaxLengthGetter = maxLengthGetter;
		}

		/// <summary>
		/// Limits the collection to be contain the specified number of elements.
		/// </summary>
		/// <param name="minLengthGetter">A C# expression for getting the minimum length of the collection, for example "@this.otherList.Count". If set, MinLength will be the fallback in case nothing in case MinLengthGetter returns null.</param>
		/// <param name="maxLength">The maximum length of the collection.</param>
		public RequiredListLengthAttribute(string minLengthGetter, int maxLength)
		{
			MinLengthGetter = minLengthGetter;
			MaxLength = maxLength;
		}
	}
}
