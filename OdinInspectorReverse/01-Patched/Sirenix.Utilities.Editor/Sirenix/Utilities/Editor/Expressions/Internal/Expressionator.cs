using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.Utilities.Editor.Expressions.Internal
{
	public class Expressionator
	{
		[SerializeField]
		private object value;

		[SerializeField]
		public readonly Dictionary<string, object> Context = new Dictionary<string, object>();

		public object Value
		{
			get
			{
				return value ?? this;
			}
			set
			{
				this.value = value;
			}
		}

		public Type ValueType => Value.GetType();

		public object this[string expression] => Expr(expression);

		public Expressionator(object value)
		{
			this.value = value;
		}

		public Expressionator Expressionate(string expression)
		{
			return new Expressionator(Expr(expression));
		}

		public bool Bool(string expression)
		{
			return Expr<bool>(expression);
		}

		public string String(string expression)
		{
			return Expr<string>(expression);
		}

		public int Int(string expression)
		{
			return Expr<int>(expression);
		}

		public object Expr(string expression)
		{
			return Expr<object>(expression);
		}

		private bool GetContextParameters(out Type[] parameters, out string[] parameterNames, out object[] parameterValues)
		{
			if (Context.Count == 0)
			{
				parameters = null;
				parameterNames = null;
				parameterValues = null;
				return false;
			}
			parameters = new Type[Context.Count];
			parameterNames = new string[Context.Count];
			parameterValues = new object[Context.Count + 1];
			int i = 0;
			foreach (KeyValuePair<string, object> entry in Context)
			{
				parameterNames[i] = entry.Key;
				parameterValues[i + 1] = entry.Value;
				parameters[i] = ((entry.Value == null) ? typeof(object) : entry.Value.GetType());
				i++;
			}
			return true;
		}

		public T Expr<T>(string expression)
		{
			Type[] parameters;
			string[] parameterNames;
			object[] parameterValues;
			bool hasParameters = GetContextParameters(out parameters, out parameterNames, out parameterValues);
			string error;
			Delegate del = ExpressionUtility.ParseExpression(expression, new EmitContext
			{
				IsStatic = false,
				Type = ValueType,
				Parameters = parameters,
				ParameterNames = parameterNames
			}, out error);
			if (error != null)
			{
				throw new Exception(error);
			}
			if (hasParameters)
			{
				parameterValues[0] = Value;
				return (T)del.DynamicInvoke(parameterValues);
			}
			return (T)del.DynamicInvoke(Value);
		}

		public IEnumerable<Expressionator> ForEachExpressionate(string expression)
		{
			IEnumerable enumerable = Expr<IEnumerable>(expression);
			if (enumerable == null)
			{
				yield break;
			}
			foreach (object result in enumerable)
			{
				yield return new Expressionator(result);
			}
		}
	}
}
