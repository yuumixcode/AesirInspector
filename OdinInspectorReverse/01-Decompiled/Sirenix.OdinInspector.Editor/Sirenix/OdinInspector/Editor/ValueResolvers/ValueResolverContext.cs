using System;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	/// <summary>
	/// This struct contains all of a ValueResolver's configurations and values it needs to function. For performance and simplicity reasons, this is a single very large struct that lives on a ValueResolver instance and is passed around by ref to anything that needs it.
	/// </summary>
	public struct ValueResolverContext
	{
		/// <summary>
		/// The property that *provides* the context for the value resolution. This is the instance that was passed to the resolver when it was created. Note that this is different from <see cref="P:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolverContext.ContextProperty" />, which is based on this value, but almost always isn't the same InspectorProperty instance.
		/// </summary>
		public InspectorProperty Property;

		/// <summary>
		/// The error message, if a valid value resolution wasn't found, or if creation of the value resolver failed because <see cref="P:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolverContext.ResolvedString" /> was invalid, or if value resolution was run but threw an exception. (In this last case, <see cref="F:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolverContext.ErrorMessageIsDueToException" /> will be true.)
		/// </summary>
		public string ErrorMessage;

		/// <summary>
		/// The named values that are available to the value resolver. Use this field only to get and set named values - once the ValueResolver has been created, new named values will have no effect.
		/// </summary>
		public NamedValues NamedValues;

		/// <summary>
		/// This is the fallback value that the value resolver will return if there is an error or failed resolution for any reason.
		/// </summary>
		public object FallbackValue;

		/// <summary>
		/// Whether there is a fallback value. This boolean exists because then null is also a valid fallback value. This boolean will always be true if an overload is used that takes a fallback value parameter.
		/// </summary>
		public bool HasFallbackValue;

		/// <summary>
		/// This will be true if <see cref="F:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolverContext.ErrorMessage" /> is not null and the message was caused by an exception thrown by code invoked during an actual value resolution.
		/// </summary>
		public bool ErrorMessageIsDueToException;

		/// <summary>
		/// Whether exceptions thrown during value resolution should be logged to the console.
		/// </summary>
		public bool LogExceptions;

		private InspectorProperty propertyUsedForContextProperty;

		private InspectorProperty contextProperty;

		private Type resultType;

		private string resolvedString;

		private bool syncRefParametersWithNamedValues;

		private static readonly ValueResolverFunc<object> PropertyGetter = delegate(ref ValueResolverContext context, int selectionIndex)
		{
			return context.Property;
		};

		private static readonly ValueResolverFunc<object> ValueGetter = delegate(ref ValueResolverContext context, int selectionIndex)
		{
			return context.Property.ValueEntry.WeakValues[selectionIndex];
		};

		private static readonly ValueResolverFunc<object> RootGetter = delegate(ref ValueResolverContext context, int selectionIndex)
		{
			return context.Property.Tree.RootProperty.ValueEntry.WeakValues[selectionIndex];
		};

		/// <summary>
		/// The type of value that the resolver is resolving.
		/// </summary>
		public Type ResultType
		{
			get
			{
				return resultType;
			}
			set
			{
				if (IsResolved)
				{
					throw new InvalidOperationException("ResultType cannot be set after a context has been resolved!");
				}
				resultType = value;
			}
		}

		/// <summary>
		/// The string that is resolved to get a value.
		/// </summary>
		public string ResolvedString
		{
			get
			{
				return resolvedString;
			}
			set
			{
				if (IsResolved)
				{
					throw new InvalidOperationException("ResolvedString cannot be set after a context has been resolved!");
				}
				resolvedString = value;
			}
		}

		/// <summary>
		/// Whether the value resolver should sync ref parameters of invoked methods with named values. If this is true, then if a ref or out parameter value is changed during value resolution, the named value associated with that parameter will also be changed to the same value.
		/// </summary>
		public bool SyncRefParametersWithNamedValues
		{
			get
			{
				return syncRefParametersWithNamedValues;
			}
			set
			{
				if (IsResolved)
				{
					throw new InvalidOperationException("SyncRefParametersWithNamedValues cannot be set after a context has been resolved!");
				}
				syncRefParametersWithNamedValues = value;
			}
		}

		/// <summary>
		/// Whether this context has been resolved.
		/// </summary>
		public bool IsResolved { get; private set; }

		/// <summary>
		/// The type that is the parent of the value resolution, ie, the type that is the context. This is the same as <see cref="P:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolverContext.ContextProperty" />.ValueEntry.TypeOfValue.
		/// </summary>
		public Type ParentType => ContextProperty.ValueEntry.TypeOfValue;

		/// <summary>
		/// The property that *is* the context for the value resolution. This is not the instance that was passed to the resolver when it was created, but this value is based on that instance. This is the property that provides the actual context - for example, if <see cref="F:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolverContext.Property" /> is for a member of a type - or for an element in a collection contained by a member - this value will be the parent property for the type that contains that member. Only if <see cref="F:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolverContext.Property" /> is the tree's root property is <see cref="P:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolverContext.ContextProperty" /> the same as <see cref="F:Sirenix.OdinInspector.Editor.ValueResolvers.ValueResolverContext.Property" />.
		/// </summary>
		public InspectorProperty ContextProperty
		{
			get
			{
				if (contextProperty == null || propertyUsedForContextProperty != Property)
				{
					propertyUsedForContextProperty = Property;
					InspectorProperty nearestValueProperty = Property.ParentValueProperty;
					while (nearestValueProperty != null && nearestValueProperty.ChildResolver is ICollectionResolver)
					{
						nearestValueProperty = nearestValueProperty.ParentValueProperty;
					}
					if (nearestValueProperty == null)
					{
						contextProperty = Property.Tree.RootProperty;
					}
					else
					{
						contextProperty = nearestValueProperty;
					}
				}
				return contextProperty;
			}
		}

		/// <summary>
		/// Gets the parent value which provides the context of the resolver.
		/// </summary>
		/// <param name="selectionIndex">The selection index of the parent value to get.</param>
		public object GetParentValue(int selectionIndex)
		{
			return ContextProperty.ValueEntry.WeakValues[selectionIndex];
		}

		/// <summary>
		/// Sets the parent value which provides the context of the resolver.
		/// </summary>
		/// <param name="selectionIndex">The selection index of the parent value to set.</param>
		/// <param name="value">The value to set.</param>
		public void SetParentValue(int selectionIndex, object value)
		{
			ContextProperty.ValueEntry.WeakValues[selectionIndex] = value;
		}

		/// <summary>
		/// Adds the default named values of "property" and "value" to the context's named values.
		/// This method is usually automatically invoked when a resolver is created, so there
		/// is no need to invoke it manually.
		/// </summary>
		public void AddDefaultContextValues()
		{
			NamedValues.Add("property", typeof(InspectorProperty), PropertyGetter);
			if (Property.ValueEntry != null)
			{
				NamedValues.Add("value", Property.ValueEntry.BaseValueType, ValueGetter);
			}
			NamedValues.Add("root", Property.Tree.RootProperty.ValueEntry.BaseValueType, RootGetter);
		}

		public static ValueResolverContext CreateDefault(InspectorProperty property, Type resultType, string resolvedString, params NamedValue[] namedValues)
		{
			ValueResolverContext result = new ValueResolverContext
			{
				Property = property,
				ResultType = resultType,
				ResolvedString = resolvedString,
				LogExceptions = true
			};
			if (namedValues != null)
			{
				for (int i = 0; i < namedValues.Length; i++)
				{
					result.NamedValues.Add(namedValues[i]);
				}
			}
			result.AddDefaultContextValues();
			return result;
		}

		public static ValueResolverContext CreateDefault<T>(InspectorProperty property, string resolvedString, params NamedValue[] namedValues)
		{
			ValueResolverContext result = new ValueResolverContext
			{
				Property = property,
				ResultType = typeof(T),
				ResolvedString = resolvedString,
				LogExceptions = true
			};
			if (namedValues != null)
			{
				for (int i = 0; i < namedValues.Length; i++)
				{
					result.NamedValues.Add(namedValues[i]);
				}
			}
			result.AddDefaultContextValues();
			return result;
		}

		public static ValueResolverContext CreateDefault<T>(InspectorProperty property, string resolvedString, T fallbackValue, params NamedValue[] namedValues)
		{
			ValueResolverContext result = new ValueResolverContext
			{
				Property = property,
				ResultType = typeof(T),
				ResolvedString = resolvedString,
				FallbackValue = fallbackValue,
				HasFallbackValue = true,
				LogExceptions = true
			};
			if (namedValues != null)
			{
				for (int i = 0; i < namedValues.Length; i++)
				{
					result.NamedValues.Add(namedValues[i]);
				}
			}
			result.AddDefaultContextValues();
			return result;
		}

		public void MarkResolved()
		{
			if (IsResolved)
			{
				throw new InvalidOperationException("This context has already been marked resolved!");
			}
			IsResolved = true;
		}
	}
}
