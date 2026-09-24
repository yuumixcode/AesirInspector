using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Sirenix.Utilities.Editor;

namespace Sirenix.OdinInspector.Editor
{
	public abstract class OdinPropertyResolver
	{
		private bool hasUpdatedChildCountEver;

		private int lastUpdatedTreeID = -1;

		private int childCount;

		private bool hasChildCountConflict;

		private int maxChildCountSeen;

		private Type resolverForType;

		private static readonly Dictionary<Type, Func<OdinPropertyResolver>> Resolver_EmittedCreator_Cache = new Dictionary<Type, Func<OdinPropertyResolver>>(FastTypeComparer.Instance);

		public bool HasChildCountConflict
		{
			get
			{
				UpdateChildCountIfNeeded();
				return hasChildCountConflict;
			}
			protected set
			{
				hasChildCountConflict = value;
			}
		}

		public int MaxChildCountSeen
		{
			get
			{
				UpdateChildCountIfNeeded();
				return maxChildCountSeen;
			}
			protected set
			{
				maxChildCountSeen = value;
			}
		}

		public virtual Type ResolverForType => resolverForType;

		public InspectorProperty Property { get; private set; }

		public virtual bool IsCollection => this is ICollectionResolver;

		public int ChildCount
		{
			get
			{
				UpdateChildCountIfNeeded();
				return childCount;
			}
		}

		public static OdinPropertyResolver Create(Type resolverType, InspectorProperty property)
		{
			if (resolverType == null)
			{
				throw new ArgumentNullException("resolverType");
			}
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			if (!typeof(OdinPropertyResolver).IsAssignableFrom(resolverType))
			{
				throw new ArgumentException("Type is not a PropertyResolver");
			}
			if (!Resolver_EmittedCreator_Cache.TryGetValue(resolverType, out var creator))
			{
				DynamicMethod builder = new DynamicMethod("OdinPropertyResolver_EmittedCreator_" + Guid.NewGuid(), typeof(OdinPropertyResolver), Type.EmptyTypes);
				ILGenerator il = builder.GetILGenerator();
				il.Emit(OpCodes.Newobj, resolverType.GetConstructor(Type.EmptyTypes));
				il.Emit(OpCodes.Ret);
				creator = (Func<OdinPropertyResolver>)builder.CreateDelegate(typeof(Func<OdinPropertyResolver>));
				Resolver_EmittedCreator_Cache.Add(resolverType, creator);
			}
			OdinPropertyResolver result = creator();
			result.Property = property;
			if (result.Property.ValueEntry != null)
			{
				result.resolverForType = property.ValueEntry.TypeOfValue;
			}
			result.Initialize();
			return result;
		}

		public static T Create<T>(InspectorProperty property) where T : OdinPropertyResolver, new()
		{
			if (property == null)
			{
				throw new ArgumentNullException("property");
			}
			T result = new T
			{
				Property = property
			};
			if (result.Property.ValueEntry != null)
			{
				result.resolverForType = property.ValueEntry.TypeOfValue;
			}
			result.Initialize();
			return result;
		}

		protected virtual void Initialize()
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void UpdateChildCountIfNeeded()
		{
			int treeId = Property.Tree.UpdateID;
			if (lastUpdatedTreeID != treeId || !hasUpdatedChildCountEver)
			{
				lastUpdatedTreeID = treeId;
				hasUpdatedChildCountEver = true;
				childCount = CalculateChildCount();
			}
		}

		public abstract InspectorPropertyInfo GetChildInfo(int childIndex);

		public abstract int ChildNameToIndex(string name);

		public virtual int ChildNameToIndex(ref StringSlice name)
		{
			return ChildNameToIndex(name.ToString());
		}

		protected abstract int CalculateChildCount();

		public virtual bool CanResolveForPropertyFilter(InspectorProperty property)
		{
			return true;
		}

		public void ForceUpdateChildCount()
		{
			if (hasUpdatedChildCountEver)
			{
				lastUpdatedTreeID = Property.Tree.UpdateID;
				childCount = CalculateChildCount();
			}
		}
	}
	public abstract class OdinPropertyResolver<TValue> : OdinPropertyResolver
	{
		private IPropertyValueEntry<TValue> valueEntry;

		public IPropertyValueEntry<TValue> ValueEntry
		{
			get
			{
				if (valueEntry == null)
				{
					valueEntry = base.Property.TryGetTypedValueEntry<TValue>();
				}
				return valueEntry;
			}
		}

		protected virtual bool AllowNullValues => false;

		protected sealed override int CalculateChildCount()
		{
			IPropertyValueEntry<TValue> valueEntry = ValueEntry;
			base.HasChildCountConflict = false;
			int count = int.MaxValue;
			base.MaxChildCountSeen = int.MinValue;
			for (int i = 0; i < valueEntry.ValueCount; i++)
			{
				TValue value = valueEntry.Values[i];
				int indexCount = ((!AllowNullValues) ? ((value != null) ? GetChildCount(value) : 0) : GetChildCount(value));
				if (count != int.MaxValue && count != indexCount)
				{
					base.HasChildCountConflict = true;
				}
				if (indexCount < count)
				{
					count = indexCount;
				}
				if (indexCount > base.MaxChildCountSeen)
				{
					base.MaxChildCountSeen = indexCount;
				}
			}
			return count;
		}

		protected abstract int GetChildCount(TValue value);
	}
	public abstract class OdinPropertyResolver<TValue, TAttribute> : OdinPropertyResolver<TValue> where TAttribute : Attribute
	{
	}
}
