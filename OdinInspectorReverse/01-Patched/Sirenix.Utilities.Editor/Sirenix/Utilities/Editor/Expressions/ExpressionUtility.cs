using System;
using System.Collections.Generic;
using System.Threading;

namespace Sirenix.Utilities.Editor.Expressions
{
	/// <summary>
	/// Utility for parsing and emitting expression delegates.
	/// </summary>
	public static class ExpressionUtility
	{
		private struct CachedExpressionKey : IEquatable<CachedExpressionKey>
		{
			public class Comparer : IEqualityComparer<CachedExpressionKey>
			{
				public static readonly Comparer Instance = new Comparer();

				public bool Equals(CachedExpressionKey x, CachedExpressionKey y)
				{
					return x.Equals(y);
				}

				public int GetHashCode(CachedExpressionKey obj)
				{
					return obj.GetHashCode();
				}
			}

			public string Expression;

			public bool RichTextError;

			public Type DelegateType;

			public bool IsStatic;

			public Type Context;

			public Type ReturnType;

			public Type[] Parameters;

			public string[] ParameterNames;

			public int? Hash;

			public CachedExpressionKey(string expression, bool richTextError, Type delegateType, EmitContext context)
			{
				Expression = expression;
				RichTextError = richTextError;
				DelegateType = delegateType;
				IsStatic = context.IsStatic;
				Context = context.Type;
				ReturnType = context.ReturnType;
				Parameters = context.Parameters;
				ParameterNames = context.ParameterNames;
				Hash = null;
				Hash = GetHashCode();
			}

			public CachedExpressionKey(string expression, bool richTextError, Type delegateType, bool isStatic, Type context, Type returnType, Type[] parameters, string[] parameterNames)
			{
				Expression = expression;
				RichTextError = richTextError;
				DelegateType = delegateType;
				IsStatic = isStatic;
				Context = context;
				ReturnType = returnType;
				Parameters = parameters;
				ParameterNames = parameterNames;
				Hash = null;
				Hash = GetHashCode();
			}

			public override bool Equals(object obj)
			{
				if (obj is CachedExpressionKey key)
				{
					return Equals(key);
				}
				return false;
			}

			public bool Equals(CachedExpressionKey other)
			{
				if (Hash != other.Hash || Expression != other.Expression || RichTextError != other.RichTextError || DelegateType != other.DelegateType || IsStatic != other.IsStatic || Context != other.Context || ReturnType != other.ReturnType || Parameters == null != (other.Parameters == null) || (Parameters != null && Parameters.Length != other.Parameters.Length) || ParameterNames == null != (other.ParameterNames == null) || (ParameterNames != null && ParameterNames.Length != other.ParameterNames.Length))
				{
					return false;
				}
				if (Parameters != null)
				{
					for (int i = 0; i < Parameters.Length; i++)
					{
						if (Parameters[i] != other.Parameters[i])
						{
							return false;
						}
					}
				}
				if (ParameterNames != null)
				{
					for (int j = 0; j < ParameterNames.Length; j++)
					{
						if (ParameterNames[j] != other.ParameterNames[j])
						{
							return false;
						}
					}
				}
				return true;
			}

			public override int GetHashCode()
			{
				if (Hash.HasValue)
				{
					return Hash.Value;
				}
				int hash = 367074651;
				hash = hash * -1521134295 + RichTextError.GetHashCode();
				hash = hash * -1521134295 + Expression.GetHashCode();
				hash = hash * -1521134295 + IsStatic.GetHashCode();
				hash = hash * -1521134295 + Context.GetHashCode();
				if (DelegateType != null)
				{
					hash = hash * -1521134295 + DelegateType.GetHashCode();
				}
				if (ReturnType != null)
				{
					hash = hash * -1521134295 + ReturnType.GetHashCode();
				}
				if (Parameters != null)
				{
					for (int i = 0; i < Parameters.Length; i++)
					{
						hash = hash * -1521134295 + Parameters[i].GetHashCode();
					}
				}
				if (ParameterNames != null)
				{
					for (int j = 0; j < ParameterNames.Length; j++)
					{
						hash = ((ParameterNames[j] != null) ? (hash * -1521134295 + ParameterNames[j].GetHashCode()) : (hash * -1521134295));
					}
				}
				Hash = hash;
				return hash;
			}
		}

		public class CachedExpression
		{
			public string Error;

			public Delegate Delegate;

			public DateTime LastAccessedTime;
		}

		/// <summary>
		/// The time that the expression cache waits to clear expressions
		/// since the last time they have been used.
		/// </summary>
		public static float ExpressionCacheClearTimeSeconds;

		private static readonly object ExpressionCache_LOCK;

		private static readonly Dictionary<CachedExpressionKey, CachedExpression> ExpressionCache;

		private static readonly Tokenizer Tokenizer;

		private static readonly ASTParser Parser;

		private static readonly ASTEmitter Emitter;

		private static readonly EmitContext Context;

		static ExpressionUtility()
		{
			ExpressionCacheClearTimeSeconds = 30f;
			ExpressionCache_LOCK = new object();
			ExpressionCache = new Dictionary<CachedExpressionKey, CachedExpression>(CachedExpressionKey.Comparer.Instance);
			Tokenizer = new Tokenizer();
			Parser = new ASTParser(Tokenizer);
			Emitter = new ASTEmitter();
			Context = new EmitContext();
			Thread thread = new Thread(ExpressionCleanupThread);
			thread.IsBackground = true;
			thread.Name = "Expression Cache Cleanup Thread";
			thread.Priority = ThreadPriority.Lowest;
			thread.Start();
		}

		private static void ExpressionCleanupThread()
		{
			List<CachedExpressionKey> toClear = new List<CachedExpressionKey>();
			while (true)
			{
				Thread.Sleep(5000);
				lock (ExpressionCache_LOCK)
				{
					DateTime now = DateTime.Now;
					foreach (KeyValuePair<CachedExpressionKey, CachedExpression> entry in ExpressionCache)
					{
						if ((now - entry.Value.LastAccessedTime).TotalSeconds >= (double)ExpressionCacheClearTimeSeconds)
						{
							toClear.Add(entry.Key);
						}
					}
					foreach (CachedExpressionKey key in toClear)
					{
						ExpressionCache.Remove(key);
					}
				}
				toClear.Clear();
			}
		}

		public static string GetASTPrettyPrint(string expression)
		{
			Tokenizer.SetExpressionString(expression);
			ASTNode ast = Parser.Parse();
			return ast.ToPrettyPrint();
		}

		public static bool TryParseTypeNameAsCSharpIdentifier(string typeString, out Type type)
		{
			try
			{
				Tokenizer.SetExpressionString(typeString);
				ASTNode ast = Parser.Parse();
				Context.IsStatic = true;
				Context.Type = typeof(object);
				Context.ReturnType = null;
				Context.Parameters = null;
				Context.ParameterNames = null;
				Emitter.Context = Context;
				Emitter.Visit(ast);
				type = ast.NodeValue as Type;
				return type != null;
			}
			catch (Exception)
			{
				type = null;
				return false;
			}
		}

		/// <summary>Parses an expression and tries to emit a delegate method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
		public static Delegate ParseExpression(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.IsStatic = isStatic;
			Context.Type = contextType;
			Context.ReturnType = null;
			Context.Parameters = null;
			Context.ParameterNames = null;
			return ParseExpression(expression, Context, null, out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and tries to emit a delegate method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="parameters">The parameters of the expression delegate.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
		public static Delegate ParseExpression(string expression, bool isStatic, Type contextType, Type[] parameters, out string errorMessage, bool richTextError = true)
		{
			Context.IsStatic = isStatic;
			Context.Type = contextType;
			Context.ReturnType = null;
			Context.Parameters = parameters;
			Context.ParameterNames = null;
			return ParseExpression(expression, Context, null, out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and tries to emit a delegate method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="parameters">The parameters of the expression delegate.</param>
		/// <param name="parameterNames">The names of the expression's parameters, for use with the named parameter syntax.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
		public static Delegate ParseExpression(string expression, bool isStatic, Type contextType, Type[] parameters, string[] parameterNames, out string errorMessage, bool richTextError = true)
		{
			Context.IsStatic = isStatic;
			Context.Type = contextType;
			Context.ReturnType = null;
			Context.Parameters = parameters;
			Context.ParameterNames = parameterNames;
			return ParseExpression(expression, Context, null, out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and tries to emit a delegate method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="context">The emit context.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
		public static Delegate ParseExpression(string expression, EmitContext context, out string errorMessage, bool richTextError = true)
		{
			return ParseExpression(expression, context, null, out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and tries to emit a delegate of the specified type.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="context">The emit context.</param>
		/// <param name="delegateType">The type of the delegate to emit.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted delegate if the expression is compiled successfully. Otherwise, null.</returns>
		public static Delegate ParseExpression(string expression, EmitContext context, Type delegateType, out string errorMessage, bool richTextError = true)
		{
			errorMessage = null;
			CachedExpressionKey cachingKey = new CachedExpressionKey(expression, richTextError, delegateType, context);
			CachedExpression cachedExpression;
			lock (ExpressionCache_LOCK)
			{
				if (!ExpressionCache.TryGetValue(cachingKey, out cachedExpression))
				{
					cachedExpression = new CachedExpression();
					try
					{
						Tokenizer.SetExpressionString(expression);
						cachedExpression.Delegate = Emitter.EmitMethod("$Expression(" + expression + ")_" + Guid.NewGuid(), Parser.Parse(), context, delegateType);
					}
					catch (SyntaxException ex)
					{
						cachedExpression.Error = ex.GetNiceErrorMessage(expression, richTextError);
						cachedExpression.Delegate = null;
					}
					if (cachingKey.Parameters != null)
					{
						Type[] newParamsArr = new Type[cachingKey.Parameters.Length];
						for (int i = 0; i < newParamsArr.Length; i++)
						{
							newParamsArr[i] = cachingKey.Parameters[i];
						}
						cachingKey.Parameters = newParamsArr;
					}
					if (cachingKey.ParameterNames != null)
					{
						string[] newParamNamesArr = new string[cachingKey.ParameterNames.Length];
						for (int j = 0; j < newParamNamesArr.Length; j++)
						{
							newParamNamesArr[j] = cachingKey.ParameterNames[j];
						}
						cachingKey.ParameterNames = newParamNamesArr;
					}
					ExpressionCache.Add(cachingKey, cachedExpression);
				}
				cachedExpression.LastAccessedTime = DateTime.Now;
			}
			errorMessage = cachedExpression.Error;
			return cachedExpression.Delegate;
		}

		/// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionFunc<TResult> ParseFunc<TResult>(string expression, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(TResult);
			Context.IsStatic = true;
			Context.Parameters = Type.EmptyTypes;
			Context.ParameterNames = null;
			return (ExpressionFunc<TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<TResult>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionFunc<T1, TResult> ParseFunc<T1, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(TResult);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? Type.EmptyTypes : new Type[1] { typeof(T1) });
			Context.ParameterNames = null;
			return (ExpressionFunc<T1, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, TResult>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionFunc<T1, T2, TResult> ParseFunc<T1, T2, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(TResult);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[1] { typeof(T2) } : new Type[2]
			{
				typeof(T1),
				typeof(T2)
			});
			Context.ParameterNames = null;
			return (ExpressionFunc<T1, T2, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, TResult>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionFunc<T1, T2, T3, TResult> ParseFunc<T1, T2, T3, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(TResult);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[2]
			{
				typeof(T2),
				typeof(T3)
			} : new Type[3]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3)
			});
			Context.ParameterNames = null;
			return (ExpressionFunc<T1, T2, T3, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, TResult>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionFunc<T1, T2, T3, T4, TResult> ParseFunc<T1, T2, T3, T4, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(TResult);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[3]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4)
			} : new Type[4]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4)
			});
			Context.ParameterNames = null;
			return (ExpressionFunc<T1, T2, T3, T4, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, TResult>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionFunc<T1, T2, T3, T4, T5, TResult> ParseFunc<T1, T2, T3, T4, T5, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(TResult);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[4]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5)
			} : new Type[5]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5)
			});
			Context.ParameterNames = null;
			return (ExpressionFunc<T1, T2, T3, T4, T5, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, TResult>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionFunc<T1, T2, T3, T4, T5, T6, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(TResult);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[5]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6)
			} : new Type[6]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6)
			});
			Context.ParameterNames = null;
			return (ExpressionFunc<T1, T2, T3, T4, T5, T6, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, TResult>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, T7, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(TResult);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[6]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7)
			} : new Type[7]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7)
			});
			Context.ParameterNames = null;
			return (ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, TResult>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(TResult);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[7]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7),
				typeof(T8)
			} : new Type[8]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7),
				typeof(T8)
			});
			Context.ParameterNames = null;
			return (ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, TResult>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionFunc method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionFunc if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> ParseFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(TResult);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[8]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7),
				typeof(T8),
				typeof(T9)
			} : new Type[9]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7),
				typeof(T8),
				typeof(T9)
			});
			Context.ParameterNames = null;
			return (ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>)ParseExpression(expression, Context, typeof(ExpressionFunc<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionAction method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionAction ParseAction(string expression, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(void);
			Context.IsStatic = true;
			Context.Parameters = Type.EmptyTypes;
			Context.ParameterNames = null;
			return (ExpressionAction)ParseExpression(expression, Context, typeof(ExpressionAction), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionAction method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionAction<T1> ParseAction<T1>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(void);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? Type.EmptyTypes : new Type[1] { typeof(T1) });
			Context.ParameterNames = null;
			return (ExpressionAction<T1>)ParseExpression(expression, Context, typeof(ExpressionAction<T1>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionAction method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionAction<T1, T2> ParseAction<T1, T2>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(void);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[1] { typeof(T2) } : new Type[2]
			{
				typeof(T1),
				typeof(T2)
			});
			Context.ParameterNames = null;
			return (ExpressionAction<T1, T2>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionAction method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionAction<T1, T2, T3> ParseAction<T1, T2, T3>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(void);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[2]
			{
				typeof(T2),
				typeof(T3)
			} : new Type[3]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3)
			});
			Context.ParameterNames = null;
			return (ExpressionAction<T1, T2, T3>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionAction method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionAction<T1, T2, T3, T4> ParseAction<T1, T2, T3, T4>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(void);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[3]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4)
			} : new Type[4]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4)
			});
			Context.ParameterNames = null;
			return (ExpressionAction<T1, T2, T3, T4>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionAction method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionAction<T1, T2, T3, T4, T5> ParseAction<T1, T2, T3, T4, T5>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(void);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[4]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5)
			} : new Type[5]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5)
			});
			Context.ParameterNames = null;
			return (ExpressionAction<T1, T2, T3, T4, T5>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionAction method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionAction<T1, T2, T3, T4, T5, T6> ParseAction<T1, T2, T3, T4, T5, T6>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(void);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[5]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6)
			} : new Type[6]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6)
			});
			Context.ParameterNames = null;
			return (ExpressionAction<T1, T2, T3, T4, T5, T6>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionAction method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionAction<T1, T2, T3, T4, T5, T6, T7> ParseAction<T1, T2, T3, T4, T5, T6, T7>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(void);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[6]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7)
			} : new Type[7]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7)
			});
			Context.ParameterNames = null;
			return (ExpressionAction<T1, T2, T3, T4, T5, T6, T7>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6, T7>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionAction method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8> ParseAction<T1, T2, T3, T4, T5, T6, T7, T8>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(void);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[7]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7),
				typeof(T8)
			} : new Type[8]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7),
				typeof(T8)
			});
			Context.ParameterNames = null;
			return (ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8>), out errorMessage, richTextError);
		}

		/// <summary>Parses an expression and emits an ExpressionAction method.</summary>
		/// <param name="expression">The expression to parse.</param>
		/// <param name="isStatic">Indicates if the expression should be static instead of instanced.</param>
		/// <param name="contextType">The context type for the execution of the expression.</param>
		/// <param name="errorMessage">Output for any errors that may occur.</param>
		/// <param name="richTextError">If <c>true</c> then error message will be formatted with color tags. Otherwise, the error message will be formatted with text only.</param>
		/// <returns>Returns the emitted ExpressionAction if the expression is compiled successfully. Otherwise, null.</returns>
		public static ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> ParseAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(string expression, bool isStatic, Type contextType, out string errorMessage, bool richTextError = true)
		{
			Context.Type = contextType;
			Context.ReturnType = typeof(void);
			Context.IsStatic = isStatic || contextType.IsStatic();
			Context.Parameters = ((!Context.IsStatic) ? new Type[8]
			{
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7),
				typeof(T8),
				typeof(T9)
			} : new Type[9]
			{
				typeof(T1),
				typeof(T2),
				typeof(T3),
				typeof(T4),
				typeof(T5),
				typeof(T6),
				typeof(T7),
				typeof(T8),
				typeof(T9)
			});
			Context.ParameterNames = null;
			return (ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>)ParseExpression(expression, Context, typeof(ExpressionAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>), out errorMessage, richTextError);
		}
	}
}
