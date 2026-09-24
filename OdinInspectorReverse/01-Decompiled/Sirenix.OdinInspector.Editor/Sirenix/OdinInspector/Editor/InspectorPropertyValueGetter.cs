using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Sirenix.OdinInspector.Editor.ValueResolvers;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Helper class to get values from InspectorProperties. This class is deprecated and fully replaced by <see cref="T:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolver" />.
	/// </summary>
	[Obsolete("InspectorPropertyValueGetter is obsolete. Use the replacement ValueResolver<T> instead.", false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class InspectorPropertyValueGetter<TReturnType>
	{
		private ValueResolver<TReturnType> backingResolver;

		public static readonly bool IsValueType = typeof(TReturnType).IsValueType;

		/// <summary>
		/// If any error occurred while looking for members, it will be stored here.
		/// </summary>
		public string ErrorMessage => backingResolver.ErrorMessage;

		/// <summary>
		/// Gets the referenced member information.
		/// </summary>
		[Obsolete("A member is no longer guaranteed.", true)]
		public MemberInfo MemberInfo
		{
			get
			{
				throw new NotSupportedException("How have you even called this?? Just stop!");
			}
		}

		[Obsolete("InspectorPropertyValueGetter is obsolete. Use the replacement ValueResolver<T> instead.", false)]
		public InspectorPropertyValueGetter(InspectorProperty property, string memberName, bool allowInstanceMember = true, bool allowStaticMember = true)
		{
			backingResolver = ValueResolver.Get<TReturnType>(property, memberName);
		}

		/// <summary>
		/// Gets the value.
		/// </summary>
		[Obsolete("InspectorPropertyValueGetter is obsolete. Use the replacement ValueResolver<T> instead.", false)]
		public TReturnType GetValue()
		{
			return backingResolver.GetValue();
		}

		/// <summary>
		/// Gets all values from all targets.
		/// </summary>
		[Obsolete("InspectorPropertyValueGetter is obsolete. Use the replacement ValueResolver<T> instead.", false)]
		public IEnumerable<TReturnType> GetValues()
		{
			int count = backingResolver.Context.Property.Tree.WeakTargets.Count;
			for (int i = 0; i < count; i++)
			{
				yield return backingResolver.GetValue(i);
			}
		}
	}
}
