using System;

namespace Sirenix.OdinInspector.Editor.ActionResolvers
{
	/// <summary>
	/// This struct contains all of an ActionResolver's configurations and values it needs to function. For performance and simplicity reasons, this is a single very large struct that lives on an ActionResolver instance and is passed around by ref to anything that needs it.
	/// </summary>
	public struct ActionResolverContext
	{
		/// <summary>
		/// The property that *provides* the context for the action resolution. This is the instance that was passed to the resolver when it was created. Note that this is different from <see cref="P:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolverContext.ContextProperty" />, which is based on this value, but almost always isn't the same InspectorProperty instance.
		/// </summary>
		public InspectorProperty Property;

		/// <summary>
		/// The error message, if a valid action resolution wasn't found, or if creation of the action resolver failed because <see cref="P:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolverContext.ResolvedString" /> was invalid, or if the action was executed but threw an exception. (In this last case, <see cref="F:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolverContext.ErrorMessageIsDueToException" /> will be true.)
		/// </summary>
		public string ErrorMessage;

		/// <summary>
		/// The named values that are available to the action resolver. Use this field only to get and set named values - once the ValueResolver has been created, new named values will have no effect.
		/// </summary>
		public NamedValues NamedValues;

		/// <summary>
		/// This will be true if <see cref="F:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolverContext.ErrorMessage" /> is not null and the message was caused by an exception thrown by code invoked during execution of the resolved action.
		/// </summary>
		public bool ErrorMessageIsDueToException;

		/// <summary>
		/// Whether exceptions thrown during action execution should be logged to the console.
		/// </summary>
		public bool LogExceptions;

		private InspectorProperty propertyUsedForContextProperty;

		private InspectorProperty contextProperty;

		private bool syncRefParametersWithNamedValues;

		private string resolvedString;

		private static readonly NamedValueGetter PropertyGetter = delegate(ref ActionResolverContext context, int selectionIndex)
		{
			return context.Property;
		};

		private static readonly NamedValueGetter ValueGetter = delegate(ref ActionResolverContext context, int selectionIndex)
		{
			return context.Property.ValueEntry.WeakValues[selectionIndex];
		};

		private static readonly NamedValueGetter RootGetter = delegate(ref ActionResolverContext context, int selectionIndex)
		{
			return context.Property.Tree.RootProperty.ValueEntry.WeakValues[selectionIndex];
		};

		/// <summary>
		/// The string that is resolved to perform an action.
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
		/// Whether the action resolver should sync ref parameters of invoked methods with named values. If this is true, then if a ref or out parameter value is changed during action execution, the named value associated with that parameter will also be changed to the same value.
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
		/// The type that is the parent of the action resolution, ie, the type that is the context. This is the same as <see cref="P:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolverContext.ContextProperty" />.ValueEntry.TypeOfValue.
		/// </summary>
		public Type ParentType => ContextProperty.ValueEntry.TypeOfValue;

		/// <summary>
		/// The property that *is* the context for the action resolution. This is not the instance that was passed to the resolver when it was created, but this value is based on that instance. This is the property that provides the actual context - for example, if <see cref="F:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolverContext.Property" /> is for a member of a type - or for an element in a collection contained by a member - this value will be the parent property for the type that contains that member. Only if <see cref="F:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolverContext.Property" /> is the tree's root property is <see cref="P:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolverContext.ContextProperty" /> the same as <see cref="F:Sirenix.OdinInspector.Editor.ActionResolvers.ActionResolverContext.Property" />.
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

		public void MarkResolved()
		{
			if (IsResolved)
			{
				throw new InvalidOperationException("This context has already been marked resolved!");
			}
			IsResolved = true;
		}

		public static ActionResolverContext CreateDefault(InspectorProperty property, string resolvedString, params NamedValue[] namedValues)
		{
			ActionResolverContext result = new ActionResolverContext
			{
				Property = property,
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
	}
}
