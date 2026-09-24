using System;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Type containing the necessary components to use C# expressions in fields.
	/// </summary>
	/// <example>
	/// // Creating and using a context with for a static type. 
	/// FieldExpressionContext context = FieldExpressionContext.StaticExpression(typeof(MyStaticType));
	///
	/// SirenixEditorFields.SmartIntField(context, ...);
	/// </example>
	/// <example>
	/// // Creating and using a context with an instanced type.
	/// FieldExpressionContext context = FieldExpressionContext.InstanceContext(myInstance);
	///
	/// SirenixEditorFields.SmartIntField(context, ...);
	/// </example>
	/// <example>
	/// // Creating and using context with an InspectorProperty, for example, in a custom Odin drawer.
	/// FieldExpressionContext context = property.ToFieldExpressionContext();
	///
	/// SirenixEditorFields.SmartIntField(context, ...);
	/// </example>
	public struct FieldExpressionContext
	{
		/// <summary>
		/// Target instance for field expressions.
		/// </summary>
		public readonly object Instance;

		/// <summary>
		/// Target type for expressions.
		/// </summary>
		public readonly Type Type;

		/// <summary>
		/// Indicates if the expressions targets a static type or not.
		/// </summary>
		public bool IsStatic => Instance == null;

		private FieldExpressionContext(object instance, Type type)
		{
			Instance = instance;
			Type = type;
		}

		/// <summary>
		/// Creates an expression context that targets nothing. Expressions are still possible, but no members can be accessed, and only static method can be called.
		/// </summary>
		/// <returns>FieldExpresionContext target targets nothing.</returns>
		public static FieldExpressionContext None()
		{
			return new FieldExpressionContext(null, typeof(object));
		}

		/// <summary>
		/// Creates an expression context that targets the provided instance. Expression can access members of the instance.
		/// </summary>
		/// <param name="instance">The instance for the context to target.</param>
		/// <returns>FieldExpressionContext that targets an instance.</returns>
		/// <exception cref="T:System.ArgumentNullException">Throws if instance is null.</exception>
		public static FieldExpressionContext InstanceContext(object instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException("instance");
			}
			return new FieldExpressionContext(instance, instance.GetType());
		}

		/// <summary>
		/// Creates an expression context that targets the provided type. Only static members can be accessed.
		/// </summary>
		/// <param name="type">The type to target.</param>
		/// <returns>FieldExpressionContext that targets a static type.</returns>
		/// <exception cref="T:System.ArgumentNullException">Throws if type is null.</exception>
		public static FieldExpressionContext StaticContext(Type type)
		{
			if ((object)type == null)
			{
				throw new ArgumentNullException("type");
			}
			return new FieldExpressionContext(null, type);
		}
	}
}
