using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor.Expressions;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public class ExpressionValueResolverCreator : ValueResolverCreator
	{
		public override string GetPossibleMatchesString(ref ValueResolverContext context)
		{
			return "C# Expressions: \"@expression\"";
		}

		public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
		{
			if (string.IsNullOrEmpty(context.ResolvedString) || context.ResolvedString.Length < 2 || context.ResolvedString[0] != '@')
			{
				return null;
			}
			string expression = context.ResolvedString.Substring(1);
			int parameterCount = context.NamedValues.Count;
			bool isStatic = context.Property == context.Property.Tree.RootProperty && context.Property.Tree.IsStatic;
			string[] parameterNames = new string[parameterCount];
			Type[] parameterTypes = new Type[parameterCount];
			for (int i = 0; i < parameterCount; i++)
			{
				NamedValue value = context.NamedValues[i];
				parameterNames[i] = value.Name;
				parameterTypes[i] = value.Type;
			}
			string compileError;
			Delegate method = ExpressionUtility.ParseExpression(expression, isStatic, context.ParentType, parameterTypes, parameterNames, out compileError);
			if (compileError != null)
			{
				context.ErrorMessage = compileError;
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			Type returnType = method.Method.ReturnType;
			if (returnType == typeof(void))
			{
				context.ErrorMessage = "Expression cannot evaluate to 'void'; it must evaluate to a value that can be assigned or converted to required type '" + typeof(TResult).GetNiceName() + "'";
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			if (!ConvertUtility.CanConvert(returnType, typeof(TResult)))
			{
				context.ErrorMessage = "Cannot convert expression result type '" + method.Method.ReturnType.GetNiceName() + "' to required type '" + typeof(TResult).GetNiceName() + "'";
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			object[] parameterValues = new object[parameterCount + ((!isStatic) ? 1 : 0)];
			return GetExpressionLambda<TResult>(method, isStatic, context.ParentType.IsValueType, parameterValues);
		}

		private static ValueResolverFunc<TResult> GetExpressionLambda<TResult>(Delegate method, bool isStatic, bool parentIsValueType, object[] parameterValues)
		{
			return delegate(ref ValueResolverContext context, int selectionIndex)
			{
				int num = 0;
				object[] array = parameterValues;
				if (!isStatic)
				{
					array[0] = context.GetParentValue(selectionIndex);
					num = 1;
				}
				for (int i = num; i < array.Length; i++)
				{
					array[i] = context.NamedValues[i - num].CurrentValue;
				}
				TResult result = ConvertUtility.Convert<TResult>(method.DynamicInvoke(parameterValues));
				if (!isStatic && parentIsValueType)
				{
					context.SetParentValue(selectionIndex, array[0]);
				}
				return result;
			};
		}
	}
}
