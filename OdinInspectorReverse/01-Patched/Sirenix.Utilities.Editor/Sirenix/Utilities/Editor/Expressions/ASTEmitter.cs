using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor.Expressions
{
	[InitializeOnLoad]
	internal class ASTEmitter : ASTVisitor
	{
		private static class CommonMembers
		{
			public static readonly MethodInfo Object_ReferenceEquals = typeof(object).GetMethod("ReferenceEquals");

			public static readonly MethodInfo Object_ToString = typeof(object).GetMethod("ToString");

			public static readonly MethodInfo String_Concat = typeof(string).GetMethod("Concat", new Type[2]
			{
				typeof(string),
				typeof(string)
			});

			public static readonly MethodInfo String_get_Chars = typeof(string).GetMethod("get_Chars", new Type[1] { typeof(int) });

			public static readonly MethodInfo Type_GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new Type[1] { typeof(RuntimeTypeHandle) });

			public static readonly Type TypeOf_InspectorProperty = TwoWaySerializationBinder.Default.BindToType("Sirenix.OdinInspector.Editor.InspectorProperty, Sirenix.OdinInspector.Editor");

			public static readonly MethodInfo InspectorProperty_PropertyQueryLookup = TypeOf_InspectorProperty.GetMethod("PropertyQueryLookup", BindingFlags.Static | BindingFlags.NonPublic);

			public static readonly MethodInfo UnityObject_Equals = typeof(UnityEngine.Object).GetOperatorMethod(Operator.Equality, typeof(UnityEngine.Object), typeof(UnityEngine.Object));
		}

		internal class NamespaceInfo : MemberInfo
		{
			private string name;

			public Dictionary<string, NamespaceInfo> ChildNamespaces = new Dictionary<string, NamespaceInfo>();

			public readonly string FullName;

			public override MemberTypes MemberType => MemberTypes.Custom;

			public override string Name => name;

			public override Type DeclaringType
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public override Type ReflectedType
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public override object[] GetCustomAttributes(bool inherit)
			{
				return new object[0];
			}

			public override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				return new object[0];
			}

			public override bool IsDefined(Type attributeType, bool inherit)
			{
				return false;
			}

			public NamespaceInfo(string name, string fullName)
			{
				this.name = name;
				FullName = fullName;
			}
		}

		private class BaseAccessMember : MemberInfo
		{
			public readonly MemberInfo InnerMember;

			public override MemberTypes MemberType
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public override string Name
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public override Type DeclaringType
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public override Type ReflectedType
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public BaseAccessMember(MemberInfo innerMember)
			{
				InnerMember = innerMember;
			}

			public override object[] GetCustomAttributes(bool inherit)
			{
				throw new NotSupportedException();
			}

			public override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new NotSupportedException();
			}

			public override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new NotSupportedException();
			}
		}

		private class UnresolvedMethodOverload : MethodInfo
		{
			private MethodInfo[] overloadCandidates;

			private bool allowInstanceAccess = true;

			private bool allowStaticAccess = true;

			public Action<ILGenerator> LoadThisInstance;

			public override ICustomAttributeProvider ReturnTypeCustomAttributes
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public override RuntimeMethodHandle MethodHandle
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public override MethodAttributes Attributes => MethodAttributes.Static;

			public override string Name
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public override Type DeclaringType
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public override Type ReflectedType
			{
				get
				{
					throw new NotSupportedException();
				}
			}

			public UnresolvedMethodOverload(MethodInfo[] overloadCandidates)
			{
				this.overloadCandidates = overloadCandidates;
				bool hasOverrides = false;
				for (int i = 0; i < this.overloadCandidates.Length; i++)
				{
					MethodInfo cand = this.overloadCandidates[i];
					if (cand.GetBaseDefinition() != cand)
					{
						hasOverrides = true;
						break;
					}
				}
				if (!hasOverrides)
				{
					return;
				}
				List<MethodInfo> list = new List<MethodInfo>(this.overloadCandidates);
				List<MethodInfo> toRemove = new List<MethodInfo>();
				for (int j = 0; j < list.Count; j++)
				{
					MethodInfo current = list[j];
					while (current.GetBaseDefinition() != current)
					{
						current = current.GetBaseDefinition();
						toRemove.Add(current);
					}
				}
				for (int k = 0; k < toRemove.Count; k++)
				{
					list.Remove(toRemove[k]);
				}
				this.overloadCandidates = list.ToArray();
			}

			public MethodInfo ResolveOverload(ASTNode ast, Type[] parameters)
			{
				MethodBase[] candidates = overloadCandidates;
				return (MethodInfo)FindOverload(ast, candidates, parameters, allowInstanceAccess, allowStaticAccess);
			}

			public string GetAllCandidatesString(string separator)
			{
				return string.Join(separator, overloadCandidates.Select((MethodInfo n) => n.GetFullName()).ToArray());
			}

			public override MethodInfo GetBaseDefinition()
			{
				throw new NotSupportedException();
			}

			public override object[] GetCustomAttributes(bool inherit)
			{
				throw new NotSupportedException();
			}

			public override object[] GetCustomAttributes(Type attributeType, bool inherit)
			{
				throw new NotSupportedException();
			}

			public override MethodImplAttributes GetMethodImplementationFlags()
			{
				throw new NotSupportedException();
			}

			public override ParameterInfo[] GetParameters()
			{
				throw new NotSupportedException();
			}

			public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
			{
				throw new NotSupportedException();
			}

			public override bool IsDefined(Type attributeType, bool inherit)
			{
				throw new NotSupportedException();
			}

			public void SetAccess(bool allowInstanceAccess, bool allowStaticAccess)
			{
				this.allowInstanceAccess = allowInstanceAccess;
				this.allowStaticAccess = allowStaticAccess;
			}
		}

		private struct OverloadScore
		{
			public int Score;

			public MethodBase Method;
		}

		internal static class IdentifierLookups
		{
			private static volatile bool initialized = false;

			private static readonly object Initialize_LOCK = new object();

			private const string ASYNC_THREAD_NAME = "Async IdentifierLookups Initialization Thread";

			public static Dictionary<string, Type> PredefinedTypes = new Dictionary<string, Type>
			{
				{
					"bool",
					typeof(bool)
				},
				{
					"byte",
					typeof(byte)
				},
				{
					"char",
					typeof(char)
				},
				{
					"decimal",
					typeof(decimal)
				},
				{
					"double",
					typeof(double)
				},
				{
					"float",
					typeof(float)
				},
				{
					"int",
					typeof(int)
				},
				{
					"long",
					typeof(long)
				},
				{
					"object",
					typeof(object)
				},
				{
					"sbyte",
					typeof(sbyte)
				},
				{
					"short",
					typeof(short)
				},
				{
					"string",
					typeof(string)
				},
				{
					"uint",
					typeof(uint)
				},
				{
					"ulong",
					typeof(ulong)
				},
				{
					"ushort",
					typeof(ushort)
				}
			};

			public static Dictionary<string, Dictionary<string, Type>> TypesByNamespace = new Dictionary<string, Dictionary<string, Type>>(2048);

			public static Dictionary<string, List<Type>> TypesByShortName = new Dictionary<string, List<Type>>(2048);

			public static Dictionary<string, NamespaceInfo> RootNamespaces = new Dictionary<string, NamespaceInfo>(64);

			public static void EnsureInitialized()
			{
				if (initialized)
				{
					return;
				}
				lock (Initialize_LOCK)
				{
					if (!initialized)
					{
						RunInitializeTask();
					}
				}
			}

			public static void InitializeAsync()
			{
				if (!initialized)
				{
					Thread thread = new Thread(RunInitializeTask);
					thread.Name = "Async IdentifierLookups Initialization Thread";
					thread.Start();
				}
			}

			private static void RunInitializeTask()
			{
				if (initialized)
				{
					return;
				}
				lock (Initialize_LOCK)
				{
					if (initialized)
					{
						return;
					}
					Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
					foreach (Assembly assembly in assemblies)
					{
						if (assembly.IsDynamic())
						{
							continue;
						}
						string assemblyName = assembly.GetName().Name;
						if (assemblyName == "Mono.Cecil" || assemblyName == "Boo.Lang")
						{
							continue;
						}
						Type[] array = assembly.SafeGetTypes();
						foreach (Type type in array)
						{
							if (type.IsNested)
							{
								continue;
							}
							if (type.IsNotPublic)
							{
								AssemblyCategory flags = AssemblyUtilities.GetAssemblyCategory(type.Assembly);
								if (flags == AssemblyCategory.Unknown || flags.HasFlag(AssemblyCategory.DotNetRuntime))
								{
									continue;
								}
							}
							string @namespace = type.Namespace ?? "";
							if (!TypesByNamespace.TryGetValue(@namespace, out var types))
							{
								RegisterNamespace(@namespace);
								types = new Dictionary<string, Type>();
								TypesByNamespace.Add(@namespace, types);
							}
							if (!types.ContainsKey(type.Name))
							{
								types.Add(type.Name, type);
								if (!TypesByShortName.TryGetValue(type.Name, out var types2))
								{
									types2 = new List<Type>();
									TypesByShortName.Add(type.Name, types2);
								}
								types2.Add(type);
							}
						}
					}
					initialized = true;
				}
			}

			private static void RegisterNamespace(string @namespace)
			{
				if (@namespace == "")
				{
					RootNamespaces.Add("", new NamespaceInfo("", ""));
					return;
				}
				Dictionary<string, NamespaceInfo> current = RootNamespaces;
				string[] names = @namespace.Split(new char[1] { '.' });
				string currentFullName = null;
				foreach (string name in names)
				{
					currentFullName = ((currentFullName == null) ? name : (currentFullName + "." + name));
					if (!current.TryGetValue(name, out var n))
					{
						n = new NamespaceInfo(name, currentFullName);
						current.Add(name, n);
					}
					current = n.ChildNamespaces;
				}
			}
		}

		private static class NullParameter
		{
		}

		private static readonly Type[] ActionDefinitions;

		private static readonly Type[] FuncDefinitions;

		private static Dictionary<Type, Type> GenericResolutionMap;

		private static List<OverloadScore> OverloadScores;

		public EmitContext Context;

		private List<Action<ILGenerator>> actions = new List<Action<ILGenerator>>();

		private bool isVisitingMembersForValueAssignment;

		static ASTEmitter()
		{
			ActionDefinitions = new Type[11]
			{
				typeof(ExpressionAction),
				typeof(ExpressionAction<>),
				typeof(ExpressionAction<, >),
				typeof(ExpressionAction<, , >),
				typeof(ExpressionAction<, , , >),
				typeof(ExpressionAction<, , , , >),
				typeof(ExpressionAction<, , , , , >),
				typeof(ExpressionAction<, , , , , , >),
				typeof(ExpressionAction<, , , , , , , >),
				typeof(ExpressionAction<, , , , , , , , >),
				typeof(ExpressionAction<, , , , , , , , , >)
			};
			FuncDefinitions = new Type[12]
			{
				null,
				typeof(ExpressionFunc<>),
				typeof(ExpressionFunc<, >),
				typeof(ExpressionFunc<, , >),
				typeof(ExpressionFunc<, , , >),
				typeof(ExpressionFunc<, , , , >),
				typeof(ExpressionFunc<, , , , , >),
				typeof(ExpressionFunc<, , , , , , >),
				typeof(ExpressionFunc<, , , , , , , >),
				typeof(ExpressionFunc<, , , , , , , , >),
				typeof(ExpressionFunc<, , , , , , , , , >),
				typeof(ExpressionFunc<, , , , , , , , , , >)
			};
			GenericResolutionMap = new Dictionary<Type, Type>();
			OverloadScores = new List<OverloadScore>();
			IdentifierLookups.InitializeAsync();
		}

		public Delegate EmitMethod(string name, ASTNode ast, EmitContext context)
		{
			return EmitMethod(name, ast, context, null);
		}

		public Delegate EmitMethod(string name, ASTNode ast, EmitContext context, Type delegateType)
		{
			if (delegateType != null && !typeof(Delegate).IsAssignableFrom(delegateType))
			{
				throw new ArgumentException("delegateType must be a type of delegate.");
			}
			Context = context;
			if (Context.IsStatic || Context.Type.IsStatic())
			{
				Context.IsStatic = true;
			}
			actions.Clear();
			Visit(ast);
			Type expressionResult = ast.TypeOfValue;
			if (expressionResult == null)
			{
				throw new SyntaxException(ast, "Expression does not have a valid return value");
			}
			if (context.ReturnType != null && expressionResult != context.ReturnType)
			{
				throw new InvalidOperationException("Expected return type of " + context.ReturnType?.ToString() + " but instead got " + expressionResult);
			}
			if (expressionResult == typeof(void))
			{
				expressionResult = null;
			}
			Type[] parameters;
			if (Context.IsStatic)
			{
				parameters = Context.Parameters ?? Type.EmptyTypes;
			}
			else
			{
				parameters = new Type[(Context.Parameters == null) ? 1 : (Context.Parameters.Length + 1)];
				parameters[0] = Context.Type;
				for (int i = 1; i < parameters.Length; i++)
				{
					parameters[i] = Context.Parameters[i - 1];
				}
			}
			DynamicMethod method = new DynamicMethod(name, expressionResult, parameters, restrictedSkipVisibility: true);
			ILGenerator il = method.GetILGenerator();
			for (int j = 0; j < actions.Count; j++)
			{
				actions[j](il);
			}
			il.Emit(OpCodes.Ret);
			if (delegateType == null)
			{
				if (expressionResult != null)
				{
					Type[] funcParameters = new Type[parameters.Length + 1];
					funcParameters[^1] = expressionResult;
					for (int k = 0; k < parameters.Length; k++)
					{
						funcParameters[k] = parameters[k];
					}
					if (funcParameters.Length == 0 || funcParameters.Length >= FuncDefinitions.Length)
					{
						throw new ArgumentException("Invalid number of arguments: " + funcParameters.Length);
					}
					delegateType = FuncDefinitions[funcParameters.Length].MakeGenericType(funcParameters);
				}
				else
				{
					if (parameters.Length >= ActionDefinitions.Length)
					{
						throw new ArgumentException("Invalid number of arguments: " + parameters.Length);
					}
					delegateType = ActionDefinitions[parameters.Length].MakeGenericType(parameters);
				}
			}
			return method.CreateDelegate(delegateType);
		}

		protected override void Add(ASTNode ast)
		{
			EmitOperator(ast, Operator.Addition, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void AddressOf(ASTNode ast)
		{
			throw new SyntaxException(ast, "Address-of operator '&' is not supported");
		}

		protected override void ArrayOf(ASTNode ast)
		{
			Visit(ast.Children[0]);
			Type type = ast.Children[0].NodeValue as Type;
			if (type == null)
			{
				throw new SyntaxException(ast.Children[0], "Expected type identifier");
			}
			int rank = (int)ast.NodeValue;
			ast.NodeValue = ((rank == 1) ? type.MakeArrayType() : type.MakeArrayType(rank));
		}

		protected override void BaseAccess(ASTNode ast)
		{
			if (Context.IsStatic)
			{
				throw new SyntaxException(ast, "Cannot use 'base' in a static context");
			}
			ast.TypeOfValue = Context.Type.BaseType;
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Ldarg_0);
			});
		}

		protected override void BitwiseAnd(ASTNode ast)
		{
			EmitOperator(ast, Operator.BitwiseAnd, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void BitwiseExclusiveOr(ASTNode ast)
		{
			EmitOperator(ast, Operator.ExclusiveOr, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void BitwiseInclusiveOr(ASTNode ast)
		{
			EmitOperator(ast, Operator.BitwiseOr, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void Checked(ASTNode ast)
		{
			throw new SyntaxException(ast, "The 'checked' keyword is not supported");
		}

		protected override void ConstantBoolean(ASTNode ast)
		{
			ast.TypeOfValue = typeof(bool);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, (bool)ast.NodeValue);
			});
		}

		protected override void ConstantChar(ASTNode ast)
		{
			ast.TypeOfValue = typeof(char);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, (char)ast.NodeValue);
			});
		}

		protected override void ConstantDecimal(ASTNode ast)
		{
			ast.TypeOfValue = typeof(decimal);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, (decimal)ast.NodeValue);
			});
		}

		protected override void ConstantFloat32(ASTNode ast)
		{
			ast.TypeOfValue = typeof(float);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, (float)ast.NodeValue);
			});
		}

		protected override void ConstantFloat64(ASTNode ast)
		{
			ast.TypeOfValue = typeof(double);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, (double)ast.NodeValue);
			});
		}

		protected override void ConstantNull(ASTNode ast)
		{
			ast.TypeOfValue = typeof(object);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, null);
			});
		}

		protected override void ConstantSignedInt32(ASTNode ast)
		{
			ast.TypeOfValue = typeof(int);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, (int)ast.NodeValue);
			});
		}

		protected override void ConstantSignedInt64(ASTNode ast)
		{
			ast.TypeOfValue = typeof(long);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, (long)ast.NodeValue);
			});
		}

		protected override void ConstantString(ASTNode ast)
		{
			ast.TypeOfValue = typeof(string);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, (string)ast.NodeValue);
			});
		}

		protected override void ConstantUnsignedInt32(ASTNode ast)
		{
			ast.TypeOfValue = typeof(uint);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, (uint)ast.NodeValue);
			});
		}

		protected override void ConstantUnsignedInt64(ASTNode ast)
		{
			ast.TypeOfValue = typeof(ulong);
			actions.Add(delegate(ILGenerator il)
			{
				EmitConstant(il, (ulong)ast.NodeValue);
			});
		}

		protected override void DefaultInferred(ASTNode ast)
		{
			throw new SyntaxException(ast, "default values with inferred types are not supported yet");
		}

		protected override void DefaultTyped(ASTNode ast)
		{
			Visit(ast.Children[0]);
			Type type = ast.Children[0].NodeValue as Type;
			if (type == null)
			{
				throw new SyntaxException(ast, "Expected type identifier after 'default('");
			}
			if (type.IsValueType)
			{
				if (type.IsPrimitive || type.IsEnum)
				{
					actions.Add(delegate(ILGenerator il)
					{
						EmitConstant(il, Activator.CreateInstance(type));
					});
				}
				else
				{
					actions.Add(delegate(ILGenerator il)
					{
						LocalBuilder local = il.DeclareLocal(type);
						il.Emit(OpCodes.Ldloca, local);
						il.Emit(OpCodes.Initobj, type);
						il.Emit(OpCodes.Ldloc, local);
					});
				}
			}
			else
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Ldnull);
				});
			}
			ast.TypeOfValue = type;
		}

		protected override void DereferencePointer(ASTNode ast)
		{
			throw new SyntaxException(ast, "Unsafe operations are not supported");
		}

		protected override void Divide(ASTNode ast)
		{
			EmitOperator(ast, Operator.Division, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void ElementAccess(ASTNode ast)
		{
			ElementAccess(ast, containerVisited: false);
		}

		protected void ElementAccess(ASTNode ast, bool containerVisited)
		{
			for (int i = 0; i < ast.Children.Count; i++)
			{
				if (!(i == 0 && containerVisited))
				{
					Visit(ast.Children[i]);
				}
			}
			bool isBaseAccess = ast.Children[0].NodeType == NodeType.BASE_ACCESS;
			Type container = ast.Children[0].TypeOfValue;
			Type[] elementArgs = new Type[ast.Children.Count - 1];
			for (int j = 1; j < ast.Children.Count; j++)
			{
				elementArgs[j - 1] = ast.Children[j].TypeOfValue;
			}
			if (container.IsArray)
			{
				int rank = container.GetArrayRank();
				Type type = container.GetElementType();
				if (rank != elementArgs.Length)
				{
					throw new SyntaxException(ast, "Expected '" + rank + "' element access arguments, but got '" + elementArgs.Length + "'");
				}
				for (int k = 0; k < elementArgs.Length; k++)
				{
					if (!IsAnyInteger(elementArgs[k]))
					{
						throw new SyntaxException(ast.Children[k + 1], "Array indexer argument '" + k + "' is not any integer type '" + elementArgs.Length + "'");
					}
				}
				if (rank == 1)
				{
					actions.Add(delegate(ILGenerator il)
					{
						il.Emit(OpCodes.Ldelem, type);
					});
				}
				else
				{
					MethodInfo method = container.GetMethod("Get");
					actions.Add(delegate(ILGenerator il)
					{
						EmitMethodCall(il, method, container);
					});
				}
				ast.TypeOfValue = type;
			}
			else if (container == typeof(string))
			{
				if (elementArgs.Length != 1)
				{
					throw new SyntaxException(ast, "Wrong number of arguments given for string element accessor (requires 1)");
				}
				bool convertType = false;
				if (elementArgs[0] != typeof(int))
				{
					if (!elementArgs[0].IsCastableTo(typeof(int), requireImplicitCast: true))
					{
						throw new SyntaxException(ast, "The type '" + elementArgs[0]?.ToString() + "' is not implicitly castable to type 'int'");
					}
					convertType = true;
				}
				actions.Add(delegate(ILGenerator il)
				{
					if (convertType)
					{
						EmitTypeConversion(ast, il, elementArgs[0], typeof(int));
					}
					EmitMethodCall(il, CommonMembers.String_get_Chars, typeof(string));
				});
				ast.TypeOfValue = typeof(char);
			}
			else
			{
				MethodInfo method2 = container.GetMethod("get_Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, elementArgs, null);
				if (method2 == null)
				{
					throw new SyntaxException(ast, "Type '" + container.GetNiceFullName() + "' does not have a compatible index accessor");
				}
				ast.TypeOfValue = method2.ReturnType;
				actions.Add(delegate(ILGenerator il)
				{
					EmitConvertParamsIfNecessary(ast, il, method2, elementArgs);
					EmitMethodCall(il, method2, container, isBaseAccess);
				});
			}
		}

		protected override void ElementAccessNullConditional(ASTNode ast)
		{
			ValueAccessNullConditional(ast, delegate
			{
				ElementAccess(ast, containerVisited: true);
			});
		}

		protected void ValueAccessNullConditional(ASTNode ast, Action valueAccess)
		{
			Visit(ast.Children[0]);
			Type typeOfMaybeNullValue = ast.Children[0].TypeOfValue;
			if (typeOfMaybeNullValue == null)
			{
				throw new SyntaxException(ast.Children[0], "Cannot access a static context via conditional operator '?.', use a normal '.' operator instead");
			}
			bool isNullableValueType = false;
			if (typeOfMaybeNullValue.IsValueType)
			{
				if (!typeOfMaybeNullValue.IsGenericType || typeOfMaybeNullValue.GetGenericTypeDefinition() != typeof(Nullable<>))
				{
					throw new SyntaxException(ast.Children[0], "Cannot use null conditional operator on non-nullable value type '" + typeOfMaybeNullValue.GetNiceName() + "'");
				}
				isNullableValueType = true;
			}
			Label nullCase = default(Label);
			actions.Add(delegate(ILGenerator il)
			{
				EmitNullConditionalBefore(il, out nullCase, typeOfMaybeNullValue, isNullableValueType);
			});
			valueAccess();
			actions.Add(delegate(ILGenerator il)
			{
				EmitNullConditionalAfter(il, nullCase, ast.TypeOfValue);
			});
		}

		private static void EmitDefaultValueForType(ILGenerator il, Type type)
		{
			if (type.IsValueType)
			{
				if (type.IsPrimitive || type.IsEnum)
				{
					EmitConstant(il, Activator.CreateInstance(type));
					return;
				}
				LocalBuilder loc = il.DeclareLocal(type);
				il.Emit(OpCodes.Ldloca, loc);
				il.Emit(OpCodes.Initobj, type);
				il.Emit(OpCodes.Ldloc, loc);
			}
			else
			{
				il.Emit(OpCodes.Ldnull);
			}
		}

		protected override void NumberedExpressionArgument(ASTNode ast)
		{
			int argumentNumber = (int)ast.NodeValue;
			if (Context.Parameters == null || Context.Parameters.Length <= argumentNumber || argumentNumber < 0)
			{
				throw new SyntaxException(ast, "Invalid argument number " + argumentNumber);
			}
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Ldarg, argumentNumber + ((!Context.IsStatic) ? 1 : 0));
			});
			ast.TypeOfValue = Context.Parameters[argumentNumber];
		}

		protected override void NamedExpressionArgument(ASTNode ast)
		{
			string argumentName = (string)ast.NodeValue;
			if (Context.ParameterNames == null)
			{
				throw new SyntaxException(ast, "Invalid expression argument name " + argumentName + "; no named expression arguments have been provided");
			}
			int parameterIndex = -1;
			for (int i = 0; i < Context.ParameterNames.Length; i++)
			{
				if (Context.ParameterNames[i] == argumentName)
				{
					parameterIndex = i;
					break;
				}
			}
			if (parameterIndex == -1)
			{
				throw new SyntaxException(ast, "Invalid expression argument name " + argumentName + "; only the following named expression arguments are available: " + string.Join(", ", (from n in Context.ParameterNames
					where n != null
					select "$" + n).ToArray()));
			}
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Ldarg, parameterIndex + ((!Context.IsStatic) ? 1 : 0));
			});
			ast.TypeOfValue = Context.Parameters[parameterIndex];
		}

		protected override void Equals(ASTNode ast)
		{
			EmitOperator(ast, Operator.Equality, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void GreaterThan(ASTNode ast)
		{
			EmitOperator(ast, Operator.GreaterThan, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void GreaterThanOrEqual(ASTNode ast)
		{
			EmitOperator(ast, Operator.GreaterThanOrEqual, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void Identifier(ASTNode ast)
		{
			Identifier(ast, null, isBaseAccess: false, !Context.IsStatic, allowStaticAccess: true);
		}

		private void Identifier(ASTNode ast, MemberInfo contextMember, bool isBaseAccess, bool allowInstanceAccess, bool allowStaticAccess)
		{
			Type[] genericArgs = null;
			if (ast.Children.Count > 0)
			{
				genericArgs = new Type[ast.Children.Count];
				for (int i = 0; i < ast.Children.Count; i++)
				{
					Visit(ast.Children[i]);
					genericArgs[i] = (Type)ast.Children[i].NodeValue;
				}
			}
			bool stackIsPrepared = contextMember != null;
			contextMember = contextMember ?? Context.Type;
			MemberInfo member = ResolveIdentifier(ast, contextMember, (string)ast.NodeValue, stackIsPrepared, genericArgs);
			bool memberIsStatic = member is Type || member is NamespaceInfo || member.IsStatic();
			if (!(member is UnresolvedMethodOverload) && !(member is Type) && !(member is NamespaceInfo) && member.DeclaringType != null)
			{
				if (memberIsStatic && !allowStaticAccess)
				{
					throw new SyntaxException(ast, "Cannot access static members via an instance");
				}
				if (!memberIsStatic && !allowInstanceAccess)
				{
					throw new SyntaxException(ast, "Cannot access instance members in a static context");
				}
			}
			if (member is UnresolvedMethodOverload)
			{
				(member as UnresolvedMethodOverload).SetAccess(allowInstanceAccess, allowStaticAccess);
			}
			if (member is FieldInfo)
			{
				FieldInfo field = member as FieldInfo;
				ast.TypeOfValue = field.FieldType;
				bool fieldTypeIsValueType = ast.TypeOfValue.IsValueType;
				if (memberIsStatic)
				{
					if (field.IsLiteral)
					{
						if (fieldTypeIsValueType && isVisitingMembersForValueAssignment)
						{
							throw new SyntaxException(ast, "Cannot set a value in a const");
						}
						actions.Add(delegate(ILGenerator il)
						{
							EmitConstant(il, field.GetValue(null));
						});
					}
					else if (field.DeclaringType.IsEnum)
					{
						if (fieldTypeIsValueType && isVisitingMembersForValueAssignment)
						{
							throw new SyntaxException(ast, "Cannot set a value in an enum");
						}
						actions.Add(delegate(ILGenerator il)
						{
							EmitEnumField(il, field);
						});
					}
					else if (fieldTypeIsValueType && isVisitingMembersForValueAssignment)
					{
						actions.Add(delegate(ILGenerator il)
						{
							il.Emit(OpCodes.Ldsflda, field);
						});
					}
					else
					{
						actions.Add(delegate(ILGenerator il)
						{
							il.Emit(OpCodes.Ldsfld, field);
						});
					}
					return;
				}
				bool loadAddress = fieldTypeIsValueType && isVisitingMembersForValueAssignment;
				actions.Add(delegate(ILGenerator il)
				{
					if (!stackIsPrepared)
					{
						il.Emit(OpCodes.Ldarg_0);
					}
					if (loadAddress)
					{
						il.Emit(OpCodes.Ldflda, field);
					}
					else
					{
						il.Emit(OpCodes.Ldfld, field);
					}
				});
				return;
			}
			if (member is PropertyInfo)
			{
				PropertyInfo property = member as PropertyInfo;
				MethodInfo getMethod = property.GetGetMethod(nonPublic: true);
				if (getMethod == null)
				{
					throw new SyntaxException(ast, "Property '" + property.Name + "' has no getter. Whoever wrote this property should question their own sanity, and seek professional help.");
				}
				ast.TypeOfValue = property.PropertyType;
				if (ast.TypeOfValue.IsValueType && isVisitingMembersForValueAssignment)
				{
					throw new SyntaxException(ast, "Cannot assign a value to members of a struct returned from a property getter.");
				}
				if (memberIsStatic)
				{
					actions.Add(delegate(ILGenerator il)
					{
						EmitMethodCall(il, getMethod, null);
					});
					return;
				}
				actions.Add(delegate(ILGenerator il)
				{
					if (!stackIsPrepared)
					{
						il.Emit(OpCodes.Ldarg_0);
					}
					EmitMethodCall(il, getMethod, (contextMember as Type) ?? getMethod.DeclaringType);
				});
				return;
			}
			if (!stackIsPrepared && member is UnresolvedMethodOverload)
			{
				(member as UnresolvedMethodOverload).LoadThisInstance = delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Ldarg_0);
				};
			}
			else if (!stackIsPrepared && !memberIsStatic && (member is MethodInfo || member is EventInfo))
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Ldarg_0);
				});
			}
			if (isBaseAccess)
			{
				ast.NodeValue = new BaseAccessMember(member);
			}
			else
			{
				ast.NodeValue = member;
			}
		}

		protected override void InstantiateType(ASTNode ast)
		{
			Visit(ast.Children[0]);
			ASTNode typeAst = ast.Children[0];
			Type type = typeAst.NodeValue as Type;
			if (type == null)
			{
				throw new SyntaxException(typeAst, "Expected type identifier after 'new'");
			}
			ast.TypeOfValue = type;
			LocalBuilder structLocal = null;
			if (type.IsValueType)
			{
				actions.Add(delegate(ILGenerator il)
				{
					structLocal = il.DeclareLocal(type);
					il.Emit(OpCodes.Ldloca, structLocal);
				});
			}
			if (type.IsValueType && ast.Children.Count == 1)
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Initobj, type);
					il.Emit(OpCodes.Ldloc, structLocal);
				});
				return;
			}
			for (int i = 1; i < ast.Children.Count; i++)
			{
				Visit(ast.Children[i]);
			}
			Type[] args = new Type[ast.Children.Count - 1];
			for (int i2 = 0; i2 < args.Length; i2++)
			{
				ASTNode child = ast.Children[i2 + 1];
				if (child.NodeType == NodeType.CONSTANT_NULL)
				{
					args[i2] = typeof(NullParameter);
					continue;
				}
				args[i2] = child.TypeOfValue;
				if (args[i2] == null)
				{
					throw new SyntaxException(ast.Children[i2 + 1], "Expected a value expression");
				}
			}
			ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			ASTNode ast2 = ast;
			MethodBase[] candidates = constructors;
			ConstructorInfo useConstructor = (ConstructorInfo)FindOverload(ast2, candidates, args, allowInstanceAccess: true, allowStaticAccess: true);
			if (useConstructor == null)
			{
				throw new SyntaxException(ast, "Type '" + type.GetNiceFullName() + "' has no valid constructor overloads for the given parameters of (" + string.Join(", ", args.Select((Type n) => n.GetNiceName()).ToArray()) + ") amongst the following candidates:\n\n" + string.Join("\n", constructors.Select((ConstructorInfo n) => n.GetFullName()).ToArray()));
			}
			if (type.IsValueType)
			{
				actions.Add(delegate(ILGenerator il)
				{
					EmitConvertParamsIfNecessary(ast, il, useConstructor, args);
					il.Emit(OpCodes.Call, useConstructor);
					il.Emit(OpCodes.Ldloc, structLocal);
				});
			}
			else
			{
				actions.Add(delegate(ILGenerator il)
				{
					EmitConvertParamsIfNecessary(ast, il, useConstructor, args);
					il.Emit(OpCodes.Newobj, useConstructor);
				});
			}
		}

		protected override void Invocation(ASTNode ast)
		{
			Invocation(ast, isTypeCastDelegateHack: false);
		}

		protected void Invocation(ASTNode ast, bool isTypeCastDelegateHack)
		{
			bool isNullConditionalInvocation = false;
			if (ast.Children[0].NodeType == NodeType.MEMBER_ACCESS_NULL_CONDITIONAL)
			{
				isNullConditionalInvocation = true;
				ast.Children[0].NodeType = NodeType.MEMBER_ACCESS;
			}
			else if (ast.Children[0].NodeType == NodeType.ELEMENT_ACCESS_NULL_CONDITIONAL)
			{
				isNullConditionalInvocation = true;
				ast.Children[0].NodeType = NodeType.ELEMENT_ACCESS;
			}
			if (isTypeCastDelegateHack && isNullConditionalInvocation)
			{
				throw new SyntaxException(ast, "Cannot perform a null conditional invocation on a delegate in this particular syntactical structure; please report an issue with the exact expression that is causing this issue so we can fix it. Thank you");
			}
			Label nullCase = default(Label);
			if (!isTypeCastDelegateHack)
			{
				Visit(ast.Children[0]);
				if (isNullConditionalInvocation)
				{
					Type contextType = ast.Children[0].Children[0].TypeOfValue;
					bool isNullableValueType = false;
					if (contextType.IsValueType)
					{
						if (!contextType.IsGenericType || contextType.GetGenericTypeDefinition() != typeof(Nullable<>))
						{
							throw new SyntaxException(ast.Children[0].Children[0], "Cannot null conditionally invoke a method on a non-nullable value type");
						}
						isNullableValueType = true;
					}
					actions.Add(delegate(ILGenerator il)
					{
						EmitNullConditionalBefore(il, out nullCase, contextType, isNullableValueType);
					});
				}
				for (int i = 1; i < ast.Children.Count; i++)
				{
					Visit(ast.Children[i]);
				}
			}
			ASTNode invokee = ast.Children[0];
			bool isBaseAccess = false;
			object member = invokee.NodeValue;
			if (member is BaseAccessMember)
			{
				member = (member as BaseAccessMember).InnerMember;
				isBaseAccess = true;
			}
			MethodInfo method;
			if (member is EventInfo)
			{
				EventInfo @event = member as EventInfo;
				method = @event.GetRaiseMethod(nonPublic: true);
			}
			else if (member is MethodInfo)
			{
				method = member as MethodInfo;
			}
			else
			{
				if (!typeof(Delegate).IsAssignableFrom(invokee.TypeOfValue))
				{
					throw new SyntaxException(ast, "Cannot do a method invocation on a value of type '" + invokee.TypeOfValue.GetNiceFullName() + "'");
				}
				method = invokee.TypeOfValue.GetMethod("Invoke", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				if (method == null)
				{
					throw new SyntaxException(ast, "Could not find Invoke method on delegate of type '" + invokee.TypeOfValue.GetNiceFullName() + "'");
				}
			}
			Type[] parameterTypes = new Type[ast.Children.Count - 1];
			for (int i2 = 0; i2 < parameterTypes.Length; i2++)
			{
				ASTNode child = ast.Children[i2 + 1];
				if (child.NodeType == NodeType.CONSTANT_NULL)
				{
					parameterTypes[i2] = typeof(NullParameter);
				}
				else
				{
					parameterTypes[i2] = child.TypeOfValue;
				}
			}
			if (method is UnresolvedMethodOverload)
			{
				UnresolvedMethodOverload overload = method as UnresolvedMethodOverload;
				method = overload.ResolveOverload(ast, parameterTypes);
				if (method == null)
				{
					throw new SyntaxException(ast, "Unable to find a method overload compatible with the given parameters of (" + string.Join(", ", parameterTypes.Select((Type n) => n.GetNiceName()).ToArray()) + ") amongst the following candidates:\n\n" + overload.GetAllCandidatesString("\n"));
				}
				if (!method.IsStatic && overload.LoadThisInstance != null)
				{
					actions.Add(delegate(ILGenerator il)
					{
						ParameterInfo[] parameters = method.GetParameters();
						LocalBuilder[] array = null;
						if (parameters.Length != 0)
						{
							array = new LocalBuilder[parameters.Length];
							for (int j = 0; j < parameters.Length; j++)
							{
								array[j] = il.DeclareLocal(parameters[j].ParameterType);
							}
							for (int num = parameters.Length - 1; num >= 0; num--)
							{
								il.Emit(OpCodes.Stloc, array[num]);
							}
						}
						overload.LoadThisInstance(il);
						if (parameters.Length != 0)
						{
							for (int k = 0; k < parameters.Length; k++)
							{
								il.Emit(OpCodes.Ldloc, array[k]);
							}
						}
					});
				}
			}
			if (method.IsGenericMethodDefinition)
			{
				method = InferGenericMethod(ast, method, parameterTypes);
			}
			ParameterInfo[] methodParameters = method.GetParameters();
			if (parameterTypes.Length != methodParameters.Length)
			{
				throw new SyntaxException(ast, "Wrong number of parameters given; expected " + methodParameters.Length + " but got " + parameterTypes.Length);
			}
			for (int i3 = 0; i3 < parameterTypes.Length; i3++)
			{
				Type givenParam = parameterTypes[i3];
				Type methodParam = methodParameters[i3].ParameterType;
				bool valid = true;
				if (givenParam == typeof(NullParameter))
				{
					if (methodParam.IsValueType)
					{
						valid = false;
					}
				}
				else if (!givenParam.IsCastableTo(methodParam, requireImplicitCast: true))
				{
					valid = false;
				}
				if (!valid)
				{
					throw new SyntaxException(ast.Children[i3 + 1], "The given parameter of type '" + givenParam.GetNiceFullName() + "' is not assignable or implicitly castable to '" + methodParam.GetNiceFullName() + "'");
				}
			}
			ast.TypeOfValue = method.ReturnType;
			Type instanceType = null;
			if (!method.IsStatic)
			{
				instanceType = ast.Children[0].GetHighestPushedStackType();
			}
			actions.Add(delegate(ILGenerator il)
			{
				EmitConvertParamsIfNecessary(ast, il, method, parameterTypes);
				EmitMethodCall(il, method, instanceType, isBaseAccess);
			});
			if (isNullConditionalInvocation)
			{
				actions.Add(delegate(ILGenerator il)
				{
					EmitNullConditionalAfter(il, nullCase, ast.TypeOfValue);
				});
			}
		}

		protected override void LeftShift(ASTNode ast)
		{
			EmitOperator(ast, Operator.LeftShift, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void LessThan(ASTNode ast)
		{
			EmitOperator(ast, Operator.LessThan, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void LessThanOrEqual(ASTNode ast)
		{
			EmitOperator(ast, Operator.LessThanOrEqual, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void LogicalAnd(ASTNode ast)
		{
			Label earlyOut = default(Label);
			Label end = default(Label);
			actions.Add(delegate(ILGenerator il)
			{
				earlyOut = il.DefineLabel();
				end = il.DefineLabel();
			});
			Visit(ast.Children[0]);
			actions.Add(delegate(ILGenerator il)
			{
				if (ast.Children[0].TypeOfValue != typeof(bool))
				{
					EmitTypeConversion(ast, il, ast.Children[0].TypeOfValue, typeof(bool));
				}
				il.Emit(OpCodes.Brfalse, earlyOut);
			});
			Visit(ast.Children[1]);
			actions.Add(delegate(ILGenerator il)
			{
				if (ast.Children[1].TypeOfValue != typeof(bool))
				{
					EmitTypeConversion(ast, il, ast.Children[1].TypeOfValue, typeof(bool));
				}
				il.Emit(OpCodes.Br, end);
				il.MarkLabel(earlyOut);
				il.Emit(OpCodes.Ldc_I4_0);
				il.MarkLabel(end);
			});
			ast.TypeOfValue = typeof(bool);
		}

		protected override void LogicalOr(ASTNode ast)
		{
			Label earlyOut = default(Label);
			Label end = default(Label);
			actions.Add(delegate(ILGenerator il)
			{
				earlyOut = il.DefineLabel();
				end = il.DefineLabel();
			});
			Visit(ast.Children[0]);
			actions.Add(delegate(ILGenerator il)
			{
				if (ast.Children[0].TypeOfValue != typeof(bool))
				{
					EmitTypeConversion(ast, il, ast.Children[0].TypeOfValue, typeof(bool));
				}
				il.Emit(OpCodes.Brtrue, earlyOut);
			});
			Visit(ast.Children[1]);
			actions.Add(delegate(ILGenerator il)
			{
				if (ast.Children[1].TypeOfValue != typeof(bool))
				{
					EmitTypeConversion(ast, il, ast.Children[1].TypeOfValue, typeof(bool));
				}
				il.Emit(OpCodes.Br, end);
				il.MarkLabel(earlyOut);
				il.Emit(OpCodes.Ldc_I4_1);
				il.MarkLabel(end);
			});
			ast.TypeOfValue = typeof(bool);
		}

		protected override void MemberAccess(ASTNode ast)
		{
			MemberAccess(ast, parentVisited: false);
		}

		protected void MemberAccess(ASTNode ast, bool parentVisited)
		{
			ASTNode parentAst = ast.Children[0];
			ASTNode identifierAst = ast.Children[1];
			if (!parentVisited)
			{
				Visit(parentAst);
			}
			bool allowInstanceAccess = false;
			bool allowStaticAccess = false;
			if (parentAst.TypeOfValue != null)
			{
				allowInstanceAccess = true;
			}
			else
			{
				allowStaticAccess = true;
			}
			Identifier(identifierAst, (parentAst.NodeValue as MemberInfo) ?? parentAst.TypeOfValue, parentAst.NodeType == NodeType.BASE_ACCESS, allowInstanceAccess, allowStaticAccess);
			ast.NodeValue = identifierAst.NodeValue;
			ast.TypeOfValue = identifierAst.TypeOfValue;
		}

		protected override void MemberAccessNullConditional(ASTNode ast)
		{
			ValueAccessNullConditional(ast, delegate
			{
				MemberAccess(ast, parentVisited: true);
			});
		}

		protected override void MemberAccessPointerDereference(ASTNode ast)
		{
			throw new SyntaxException(ast, "Unsafe operations are not supported");
		}

		protected override void Multiply(ASTNode ast)
		{
			EmitOperator(ast, Operator.Multiply, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void NotEquals(ASTNode ast)
		{
			EmitOperator(ast, Operator.Inequality, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void NullCoalesce(ASTNode ast)
		{
			if (ast.Children[0].NodeType == NodeType.CONSTANT_NULL)
			{
				Visit(ast.Children[1]);
				ast.TypeOfValue = ast.Children[1].TypeOfValue;
				return;
			}
			Visit(ast.Children[0]);
			Label label = default(Label);
			actions.Add(delegate(ILGenerator il)
			{
				label = il.DefineLabel();
				il.Emit(OpCodes.Dup);
				il.Emit(OpCodes.Brtrue, label);
				il.Emit(OpCodes.Pop);
			});
			Visit(ast.Children[1]);
			Type value = ast.Children[0].TypeOfValue;
			Type replacement = ast.Children[1].TypeOfValue;
			if (!replacement.IsCastableTo(value, requireImplicitCast: true))
			{
				throw new SyntaxException(ast, "Value '" + replacement.GetNiceFullName() + "' is not assignable or implicitly castable to type '" + value.GetNiceFullName() + "'");
			}
			actions.Add(delegate(ILGenerator il)
			{
				EmitTypeConversion(ast, il, replacement, value);
				il.Emit(OpCodes.Nop);
				il.MarkLabel(label);
			});
			ast.TypeOfValue = value;
		}

		protected override void ParenthesizedExpression(ASTNode ast)
		{
			Visit(ast.Children[0]);
			ast.TypeOfValue = ast.Children[0].TypeOfValue;
		}

		protected override void PostDecrement(ASTNode ast)
		{
			throw new SyntaxException(ast, "Post-decrement operations are not supported");
		}

		protected override void PostIncrement(ASTNode ast)
		{
			throw new SyntaxException(ast, "Post-increment operations are not supported");
		}

		protected override void PreDecrement(ASTNode ast)
		{
			throw new SyntaxException(ast, "Pre-decrement operations are not supported");
		}

		protected override void PreIncrement(ASTNode ast)
		{
			throw new SyntaxException(ast, "Pre-increment operations are not supported");
		}

		protected override void PropertyQuery(ASTNode ast)
		{
			int propertyArgIndex = -1;
			if (Context.ParameterNames != null)
			{
				for (int i = 0; i < Context.ParameterNames.Length; i++)
				{
					if (Context.ParameterNames[i] == "property" && Context.Parameters[i] == CommonMembers.TypeOf_InspectorProperty)
					{
						propertyArgIndex = i;
						break;
					}
				}
			}
			if (propertyArgIndex < 0)
			{
				throw new SyntaxException(ast, "The property query operation is only supported in expressions with a named parameter 'property' of type 'InspectorProperty'.");
			}
			MethodInfo method = CommonMembers.InspectorProperty_PropertyQueryLookup;
			string propertyName = (string)ast.NodeValue;
			if (!Context.IsStatic)
			{
				propertyArgIndex++;
			}
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Ldarg, propertyArgIndex);
				il.Emit(OpCodes.Ldstr, propertyName);
				il.Emit(OpCodes.Call, method);
			});
			ast.TypeOfValue = CommonMembers.TypeOf_InspectorProperty;
		}

		protected override void RelationalAs(ASTNode ast)
		{
			ASTNode valueAst = ast.Children[0];
			ASTNode typeAst = ast.Children[1];
			Visit(valueAst);
			Visit(typeAst);
			Type type = typeAst.NodeValue as Type;
			if (type == null)
			{
				throw new SyntaxException(typeAst, "Expected type identifier");
			}
			if (type.IsValueType)
			{
				throw new SyntaxException(typeAst, "Cannot use value types with 'as' keyword");
			}
			Type valueType = valueAst.TypeOfValue;
			if (valueType.IsValueType)
			{
				if (!type.IsAssignableFrom(valueType))
				{
					throw new SyntaxException(ast, "Cannot convert '" + valueType.GetNiceFullName() + "' to '" + type.GetNiceFullName() + "'");
				}
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Box, valueType);
				});
			}
			else
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Isinst, type);
				});
			}
			ast.TypeOfValue = type;
		}

		protected override void RelationalIs(ASTNode ast)
		{
			ASTNode valueAst = ast.Children[0];
			ASTNode typeAst = ast.Children[1];
			Visit(valueAst);
			Visit(typeAst);
			Type type = typeAst.NodeValue as Type;
			if (type == null)
			{
				throw new SyntaxException(typeAst, "Expected type identifier");
			}
			if (type.IsValueType)
			{
				throw new SyntaxException(typeAst, "Cannot use value types with 'is' keyword");
			}
			Type valueType = valueAst.TypeOfValue;
			if (valueType.IsValueType)
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Pop);
					EmitConstant(il, type.IsAssignableFrom(valueType));
				});
			}
			else
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Isinst, type);
					il.Emit(OpCodes.Ldnull);
					il.Emit(OpCodes.Cgt_Un);
				});
			}
			ast.TypeOfValue = typeof(bool);
		}

		protected override void Remainder(ASTNode ast)
		{
			EmitOperator(ast, Operator.Modulus, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void RightShift(ASTNode ast)
		{
			EmitOperator(ast, Operator.RightShift, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void SimpleAssignment(ASTNode ast)
		{
			ASTNode assignTo = ast.Children[0];
			ASTNode assignValue = ast.Children[1];
			if (assignTo.NodeType == NodeType.ELEMENT_ACCESS || assignTo.NodeType == NodeType.ELEMENT_ACCESS_NULL_CONDITIONAL)
			{
				SimpleAssignment_ElementAccess(ast, assignTo, assignValue);
				return;
			}
			if (assignTo.NodeType == NodeType.IDENTIFIER || assignTo.NodeType == NodeType.MEMBER_ACCESS || assignTo.NodeType == NodeType.MEMBER_ACCESS_NULL_CONDITIONAL)
			{
				SimpleAssignment_IdentifierAccess(ast, assignTo, assignValue);
				return;
			}
			throw new SyntaxException(assignTo, "Cannot assign a value to the expression on the left-hand side of the assignment; the left-hand expression is of kind '" + assignTo.NodeType.ToString() + "'!");
		}

		private void SimpleAssignment_IdentifierAccess(ASTNode ast, ASTNode assignTo, ASTNode assignValue)
		{
			bool allowInstanceAccess = false;
			bool allowStaticAccess = false;
			bool ldArg0IfIsInstance = false;
			bool isNullConditionalAssignment = false;
			ASTNode assignToIdentifier;
			MemberInfo contextMember;
			if (assignTo.NodeType == NodeType.IDENTIFIER)
			{
				assignToIdentifier = assignTo;
				allowInstanceAccess = !Context.IsStatic;
				allowStaticAccess = true;
				if (allowInstanceAccess)
				{
					ldArg0IfIsInstance = true;
				}
				contextMember = Context.Type;
			}
			else
			{
				if (assignTo.NodeType != NodeType.MEMBER_ACCESS && assignTo.NodeType != NodeType.MEMBER_ACCESS_NULL_CONDITIONAL)
				{
					throw new SyntaxException(assignTo, "Cannot assign a value to the expression on the left-hand side of the assignment!");
				}
				if (assignTo.NodeType == NodeType.MEMBER_ACCESS_NULL_CONDITIONAL)
				{
					isNullConditionalAssignment = true;
					assignTo.NodeType = NodeType.MEMBER_ACCESS;
				}
				ASTNode memberAccessParent = assignTo.Children[0];
				ASTNode memberAccessIdentifier = assignTo.Children[1];
				try
				{
					isVisitingMembersForValueAssignment = true;
					Visit(memberAccessParent);
				}
				finally
				{
					isVisitingMembersForValueAssignment = false;
				}
				if (memberAccessParent.TypeOfValue != null)
				{
					allowInstanceAccess = true;
				}
				else
				{
					allowStaticAccess = true;
				}
				contextMember = (memberAccessParent.NodeValue as MemberInfo) ?? memberAccessParent.TypeOfValue;
				assignToIdentifier = memberAccessIdentifier;
			}
			if (assignToIdentifier.Children.Count > 0)
			{
				throw new SyntaxException(assignTo, "Cannot assign a value to a generic identifier!");
			}
			MemberInfo member = ResolveIdentifier(assignToIdentifier, contextMember, (string)assignToIdentifier.NodeValue, onlyLocals: true, null);
			if (member is UnresolvedMethodOverload || member is MethodInfo)
			{
				throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a method declaration!");
			}
			if (member is Type)
			{
				throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a type declaration!");
			}
			if (member is NamespaceInfo)
			{
				throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a namespace!");
			}
			if (member is EventInfo)
			{
				throw new SyntaxException(assignToIdentifier, "Cannot assign a value to an event!");
			}
			FieldInfo field = member as FieldInfo;
			PropertyInfo prop = member as PropertyInfo;
			if (field == null && prop == null)
			{
				throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a '" + member.GetType().Name + "'!");
			}
			bool memberIsStatic = member.IsStatic();
			if (memberIsStatic && !allowStaticAccess)
			{
				throw new SyntaxException(assignToIdentifier, "Cannot access static members via an instance");
			}
			if (!memberIsStatic && !allowInstanceAccess)
			{
				throw new SyntaxException(assignToIdentifier, "Cannot access instance members in a static context");
			}
			if (!memberIsStatic && ldArg0IfIsInstance)
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Ldarg_0);
				});
			}
			Label nullCase = default(Label);
			if (isNullConditionalAssignment)
			{
				Type contextType = contextMember as Type;
				if (contextType == null)
				{
					throw new SyntaxException(assignTo, "Cannot assign to left-hand side of expression using a null conditional assignment");
				}
				if (contextType.IsValueType)
				{
					throw new SyntaxException(assignTo, "Cannot assign values to members of a value type using a null conditional assignment - not even to a nullable value type");
				}
				actions.Add(delegate(ILGenerator il)
				{
					EmitNullConditionalBefore(il, out nullCase, contextType, isNullableValueType: false);
				});
			}
			Visit(assignValue);
			if (assignValue.TypeOfValue == null)
			{
				throw new SyntaxException(assignValue, "Expression to assign does not evaluate to any value");
			}
			Type typeOfMember = member.GetReturnType();
			if (assignValue.TypeOfValue == typeof(object) && IsConstantNull(assignValue))
			{
				if (typeOfMember.IsValueType)
				{
					throw new SyntaxException(assignValue, "Cannot assign null to a value type '" + typeOfMember.GetNiceFullName() + "'");
				}
				assignValue.TypeOfValue = typeOfMember;
			}
			LocalBuilder dupLocal = null;
			actions.Add(delegate(ILGenerator il)
			{
				dupLocal = il.DeclareLocal(assignValue.TypeOfValue);
				il.Emit(OpCodes.Dup);
				il.Emit(OpCodes.Stloc, dupLocal);
				EmitTypeConversion(assignValue, il, assignValue.TypeOfValue, typeOfMember);
			});
			if (field != null)
			{
				if (field.IsLiteral)
				{
					throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a const field");
				}
				if (field.DeclaringType.IsEnum)
				{
					throw new SyntaxException(assignToIdentifier, "Cannot assign a value to a const field");
				}
				if (memberIsStatic)
				{
					actions.Add(delegate(ILGenerator il)
					{
						il.Emit(OpCodes.Stsfld, field);
					});
				}
				else
				{
					actions.Add(delegate(ILGenerator il)
					{
						il.Emit(OpCodes.Stfld, field);
					});
				}
			}
			else
			{
				MethodInfo setter = prop.GetSetMethod(nonPublic: true);
				if (setter == null)
				{
					throw new SyntaxException(assignToIdentifier, "Property '" + prop.Name + "' has no setter");
				}
				actions.Add(delegate(ILGenerator il)
				{
					EmitMethodCall(il, setter, setter.DeclaringType);
				});
			}
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Ldloc, dupLocal);
			});
			if (isNullConditionalAssignment)
			{
				actions.Add(delegate(ILGenerator il)
				{
					EmitNullConditionalAfter(il, nullCase, assignValue.TypeOfValue);
				});
			}
			ast.TypeOfValue = assignValue.TypeOfValue;
		}

		private static void EmitNullConditionalAfter(ILGenerator il, Label nullCase, Type valueType)
		{
			Label end = il.DefineLabel();
			il.Emit(OpCodes.Br, end);
			il.MarkLabel(nullCase);
			if (valueType != null && valueType != typeof(void))
			{
				EmitDefaultValueForType(il, valueType);
			}
			il.MarkLabel(end);
			il.Emit(OpCodes.Nop);
		}

		private static void EmitNullConditionalBefore(ILGenerator il, out Label nullCase, Type contextType, bool isNullableValueType)
		{
			nullCase = il.DefineLabel();
			if (isNullableValueType)
			{
				PropertyInfo getHasValueProp = contextType.GetProperty("HasValue");
				MethodInfo getHasValueMethod = getHasValueProp.GetGetMethod(nonPublic: true);
				LocalBuilder valueLocal = il.DeclareLocal(contextType);
				il.Emit(OpCodes.Stloc, valueLocal);
				il.Emit(OpCodes.Ldloc, valueLocal);
				il.Emit(OpCodes.Ldloca, valueLocal);
				il.Emit(OpCodes.Call, getHasValueMethod);
			}
			else
			{
				il.Emit(OpCodes.Dup);
				if (typeof(UnityEngine.Object).IsAssignableFrom(contextType))
				{
					il.Emit(OpCodes.Ldnull);
					il.Emit(OpCodes.Call, CommonMembers.UnityObject_Equals);
					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Ceq);
				}
				else
				{
					il.Emit(OpCodes.Ldnull);
					il.Emit(OpCodes.Ceq);
					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Ceq);
				}
			}
			Label notNullCase = il.DefineLabel();
			il.Emit(OpCodes.Brtrue, notNullCase);
			il.Emit(OpCodes.Pop);
			il.Emit(OpCodes.Br, nullCase);
			il.MarkLabel(notNullCase);
			il.Emit(OpCodes.Nop);
		}

		private void SimpleAssignment_ElementAccess(ASTNode ast, ASTNode assignTo, ASTNode assignValue)
		{
			bool isNullConditionalAccess = false;
			if (assignTo.NodeType == NodeType.ELEMENT_ACCESS_NULL_CONDITIONAL)
			{
				isNullConditionalAccess = true;
				assignTo.NodeType = NodeType.ELEMENT_ACCESS;
			}
			try
			{
				isVisitingMembersForValueAssignment = true;
				Visit(assignTo.Children[0]);
			}
			finally
			{
				isVisitingMembersForValueAssignment = false;
			}
			Label nullCase = default(Label);
			if (isNullConditionalAccess)
			{
				Type contextType = assignTo.Children[0].TypeOfValue;
				if (contextType.IsValueType)
				{
					throw new SyntaxException(assignTo, "Cannot make a null conditional element assignment to a value type");
				}
				actions.Add(delegate(ILGenerator il)
				{
					EmitNullConditionalBefore(il, out nullCase, contextType, isNullableValueType: false);
				});
			}
			for (int i = 1; i < assignTo.Children.Count; i++)
			{
				Visit(assignTo.Children[i]);
			}
			Type container = assignTo.Children[0].TypeOfValue;
			Type[] elementArgs = new Type[assignTo.Children.Count - 1];
			for (int i2 = 1; i2 < assignTo.Children.Count; i2++)
			{
				elementArgs[i2 - 1] = assignTo.Children[i2].TypeOfValue;
			}
			if (container == typeof(string))
			{
				throw new SyntaxException(ast, "Cannot write values to a string indexer");
			}
			Visit(assignValue);
			if (assignValue.TypeOfValue == null)
			{
				throw new SyntaxException(assignValue, "Expression to assign does not evaluate to any value");
			}
			bool valueIsConstNull = false;
			if (assignValue.TypeOfValue == typeof(object) && IsConstantNull(assignValue))
			{
				valueIsConstNull = true;
			}
			LocalBuilder dupLocal = null;
			actions.Add(delegate(ILGenerator il)
			{
				dupLocal = il.DeclareLocal(assignValue.TypeOfValue);
				il.Emit(OpCodes.Dup);
				il.Emit(OpCodes.Stloc, dupLocal);
			});
			if (container.IsArray)
			{
				int rank = container.GetArrayRank();
				Type elementType = container.GetElementType();
				if (valueIsConstNull)
				{
					if (elementType.IsValueType)
					{
						throw new SyntaxException(assignValue, "Cannot assign null to value type '" + elementType.GetNiceFullName() + "'");
					}
					assignValue.TypeOfValue = elementType;
				}
				if (rank != elementArgs.Length)
				{
					throw new SyntaxException(assignTo, "Expected '" + rank + "' array indexer arguments, but got '" + elementArgs.Length + "'");
				}
				for (int i3 = 0; i3 < elementArgs.Length; i3++)
				{
					if (!IsAnyInteger(elementArgs[i3]))
					{
						throw new SyntaxException(assignTo.Children[i3 + 1], "Array indexer argument '" + i3 + "' is not any integer type '" + elementArgs.Length + "'");
					}
				}
				if (rank == 1)
				{
					actions.Add(delegate(ILGenerator il)
					{
						EmitTypeConversion(assignValue, il, assignValue.TypeOfValue, elementType);
						il.Emit(OpCodes.Stelem, elementType);
						il.Emit(OpCodes.Ldloc, dupLocal);
					});
				}
				else
				{
					MethodInfo method = container.GetMethod("Set");
					actions.Add(delegate(ILGenerator il)
					{
						EmitTypeConversion(assignValue, il, assignValue.TypeOfValue, elementType);
						EmitMethodCall(il, method, container);
						il.Emit(OpCodes.Ldloc, dupLocal);
					});
				}
			}
			else
			{
				Type[] setArgs = new Type[elementArgs.Length + 1];
				Array.Copy(elementArgs, setArgs, elementArgs.Length);
				setArgs[setArgs.Length - 1] = assignValue.TypeOfValue;
				MethodInfo method2 = container.GetMethod("set_Item", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, setArgs, null);
				if (method2 == null)
				{
					throw new SyntaxException(ast, "Type '" + container.GetNiceFullName() + "' does not have a compatible index accessor");
				}
				actions.Add(delegate(ILGenerator il)
				{
					EmitConvertParamsIfNecessary(assignTo, il, method2, setArgs);
					EmitMethodCall(il, method2, container, assignTo.Children[0].NodeType == NodeType.BASE_ACCESS, fixValueTypeLoading: false);
					il.Emit(OpCodes.Ldloc, dupLocal);
				});
			}
			if (isNullConditionalAccess)
			{
				actions.Add(delegate(ILGenerator il)
				{
					EmitNullConditionalAfter(il, nullCase, assignValue.TypeOfValue);
				});
			}
			ast.TypeOfValue = assignValue.TypeOfValue;
		}

		protected override void SizeOf(ASTNode ast)
		{
			Visit(ast.Children[0]);
			Type type = ast.Children[0].NodeValue as Type;
			if (type == null)
			{
				throw new SyntaxException(ast.Children[0], "Expected type identifier");
			}
			int size;
			try
			{
				size = Marshal.SizeOf(type);
			}
			catch (ArgumentException)
			{
				throw new SyntaxException(ast, "Cannot get the size of type '" + type.GetNiceFullName() + "'; it cannot be marshaled as an unmanaged structure");
			}
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Ldc_I4, size);
			});
			ast.TypeOfValue = typeof(int);
		}

		protected override void Subtract(ASTNode ast)
		{
			EmitOperator(ast, Operator.Subtraction, ast.Children[0], ast.Children[1], out ast.TypeOfValue);
		}

		protected override void TernaryConditional(ASTNode ast)
		{
			ASTNode condition = ast.Children[0];
			ASTNode value1 = ast.Children[1];
			ASTNode value2 = ast.Children[2];
			Visit(condition);
			Type type = condition.TypeOfValue;
			if (type == null)
			{
				throw new SyntaxException(condition, "Expected boolean value for ternary operator");
			}
			if (type != typeof(bool))
			{
				if (!type.IsCastableTo(typeof(bool), requireImplicitCast: true))
				{
					throw new SyntaxException(condition, "Type '" + type.GetNiceFullName() + "' is not implicitly castable to type 'bool'");
				}
				actions.Add(delegate(ILGenerator il)
				{
					EmitTypeConversion(condition, il, type, typeof(bool));
				});
			}
			Label secondValue = default(Label);
			Label end = default(Label);
			actions.Add(delegate(ILGenerator il)
			{
				secondValue = il.DefineLabel();
				end = il.DefineLabel();
				il.Emit(OpCodes.Brtrue, secondValue);
			});
			Visit(value2);
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Br, end);
				il.Emit(OpCodes.Nop);
				il.MarkLabel(secondValue);
			});
			Visit(value1);
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Nop);
				il.MarkLabel(end);
			});
			if (value1.TypeOfValue != value2.TypeOfValue)
			{
				throw new SyntaxException(ast, "Expected same type as result values of ternary conditional, but got types '" + value1.TypeOfValue?.ToString() + "' and '" + value2.TypeOfValue?.ToString() + "'");
			}
			ast.TypeOfValue = value1.TypeOfValue;
		}

		protected override void ThisAccess(ASTNode ast)
		{
			if (Context.IsStatic)
			{
				throw new SyntaxException(ast, "Cannot use 'this' in a static context");
			}
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Ldarg_0);
			});
			ast.TypeOfValue = Context.Type;
		}

		protected override void TypeCast(ASTNode ast)
		{
			TypeCast(ast, visitedChildren: false);
		}

		private void TypeCast(ASTNode ast, bool visitedChildren)
		{
			ASTNode typeAst = ast.Children[0];
			ASTNode valueAst = ast.Children[1];
			if (!visitedChildren)
			{
				Visit(typeAst);
				Visit(valueAst);
			}
			Type castToType = typeAst.NodeValue as Type;
			if (castToType == null)
			{
				if (typeAst.TypeOfValue != null && typeof(Delegate).IsAssignableFrom(typeAst.TypeOfValue))
				{
					Invocation(ast, isTypeCastDelegateHack: true);
					return;
				}
				throw new SyntaxException(typeAst, "Expected type identifier");
			}
			Type valueType = valueAst.TypeOfValue;
			if (valueType == typeof(void))
			{
				throw new SyntaxException(ast, "Cannot cast type 'void' to anything");
			}
			if (IsAnyNumber(valueType) && IsAnyNumber(castToType))
			{
				actions.Add(delegate(ILGenerator il)
				{
					EmitConvertToNumber(il, castToType);
				});
			}
			else if (castToType.IsAssignableFrom(valueType))
			{
				if (valueType.IsValueType && !castToType.IsValueType)
				{
					actions.Add(delegate(ILGenerator il)
					{
						il.Emit(OpCodes.Box, valueType);
					});
				}
			}
			else if (valueType.IsAssignableFrom(castToType))
			{
				actions.Add(delegate(ILGenerator il)
				{
					if (valueType.IsValueType && !castToType.IsValueType)
					{
						il.Emit(OpCodes.Box, valueType);
					}
					if (!valueType.IsValueType && castToType.IsValueType)
					{
						il.Emit(OpCodes.Unbox_Any, castToType);
					}
					else
					{
						il.Emit(OpCodes.Castclass, castToType);
					}
				});
			}
			else
			{
				MethodInfo castMethod = valueType.GetCastMethod(castToType);
				if (castMethod == null)
				{
					throw new SyntaxException(ast, "There is no type conversion from '" + valueType.GetNiceFullName() + "' to '" + castToType.GetNiceFullName() + "'");
				}
				actions.Add(delegate(ILGenerator il)
				{
					EmitMethodCall(il, castMethod, null);
				});
			}
			ast.TypeOfValue = castToType;
		}

		protected override void TypeOf(ASTNode ast)
		{
			Visit(ast.Children[0]);
			Type type = ast.Children[0].NodeValue as Type;
			if (type == null)
			{
				throw new SyntaxException(ast.Children[0], "Expected type identifier");
			}
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Ldtoken, type);
				EmitMethodCall(il, CommonMembers.Type_GetTypeFromHandle, null);
			});
			ast.TypeOfValue = typeof(Type);
		}

		protected override void TypeOfVoid(ASTNode ast)
		{
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Ldtoken, typeof(void));
				EmitMethodCall(il, CommonMembers.Type_GetTypeFromHandle, null);
			});
			ast.TypeOfValue = typeof(Type);
		}

		protected override void UnaryComplement(ASTNode ast)
		{
			Visit(ast.Children[0]);
			Type type = ast.Children[0].TypeOfValue;
			if (IsAnyNumber(type))
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Not);
				});
				ast.TypeOfValue = type;
				return;
			}
			MethodInfo operatorMethod = type.GetOperatorMethod(Operator.BitwiseComplement);
			actions.Add(delegate(ILGenerator il)
			{
				EmitMethodCall(il, operatorMethod, null);
			});
			ast.TypeOfValue = operatorMethod.ReturnType;
		}

		protected override void UnaryMinus(ASTNode ast)
		{
			Visit(ast.Children[0]);
			Type type = ast.Children[0].TypeOfValue;
			if (IsAnyNumber(type))
			{
				if (type == typeof(ulong))
				{
					throw new SyntaxException(ast, "Cannot apply unary operator '-' to type ulong");
				}
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Neg);
				});
				if (IsFloatingPoint(type))
				{
					ast.TypeOfValue = type;
				}
				else if (type == typeof(uint) || type == typeof(long))
				{
					ast.TypeOfValue = typeof(long);
				}
				else
				{
					ast.TypeOfValue = typeof(int);
				}
			}
			else
			{
				MethodInfo operatorMethod = type.GetOperatorMethod(Operator.BitwiseComplement);
				if (operatorMethod == null)
				{
					throw new SyntaxException(ast, "Type '" + type.GetNiceFullName() + "' has no method override for unary complement operator '~'");
				}
				actions.Add(delegate(ILGenerator il)
				{
					EmitMethodCall(il, operatorMethod, null);
				});
				ast.TypeOfValue = operatorMethod.ReturnType;
			}
		}

		protected override void UnaryNot(ASTNode ast)
		{
			Visit(ast.Children[0]);
			Type type = ast.Children[0].TypeOfValue;
			if (type == typeof(bool))
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Ceq);
				});
				ast.TypeOfValue = typeof(bool);
				return;
			}
			MethodInfo operatorMethod = type.GetOperatorMethod(Operator.LogicalNot);
			if (operatorMethod != null)
			{
				actions.Add(delegate(ILGenerator il)
				{
					EmitMethodCall(il, operatorMethod, null);
				});
				ast.TypeOfValue = operatorMethod.ReturnType;
				return;
			}
			MethodInfo castMethod;
			if ((castMethod = type.GetCastMethod(typeof(bool), requireImplicitCast: true)) != null)
			{
				actions.Add(delegate(ILGenerator il)
				{
					EmitMethodCall(il, castMethod, null);
					il.Emit(OpCodes.Ldc_I4_0);
					il.Emit(OpCodes.Ceq);
				});
				ast.TypeOfValue = typeof(bool);
				return;
			}
			throw new SyntaxException(ast, "Type '" + type.GetNiceFullName() + "' has no method override for unary not operator '!' and no implicit cast to bool.");
		}

		protected override void Unchecked(ASTNode ast)
		{
			throw new SyntaxException(ast, "The 'unchecked' keyword is not supported");
		}

		private static MemberInfo ResolveIdentifier(ASTNode ast, MemberInfo context, string identifier, bool onlyLocals, Type[] genericArgs)
		{
			IdentifierLookups.EnsureInitialized();
			if (!onlyLocals && IdentifierLookups.PredefinedTypes.TryGetValue(identifier, out var type))
			{
				return type;
			}
			if (onlyLocals && context == null)
			{
				throw new ArgumentException();
			}
			string genericIdentifier = null;
			if (genericArgs != null)
			{
				genericIdentifier = identifier + "`" + genericArgs.Length;
			}
			if (context is Type)
			{
				Type type2 = context as Type;
				List<MemberInfo> members = type2.GetAllMembers(identifier, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).ToList();
				if (members.Count > 0)
				{
					if (genericArgs != null)
					{
						List<MemberInfo> possibles = new List<MemberInfo>();
						for (int i = 0; i < members.Count; i++)
						{
							if (IsMemberCompatibleWithGenericArgs(members[i], genericArgs, out var genericResult))
							{
								possibles.Add(genericResult);
							}
						}
						if (possibles.Count == 1)
						{
							return possibles[0];
						}
						if (possibles.Count > 1)
						{
							return new UnresolvedMethodOverload(possibles.Cast<MethodInfo>().ToArray());
						}
					}
					else
					{
						if (members.Count == 1)
						{
							return members[0];
						}
						if (members.All((MemberInfo m) => m is MethodInfo))
						{
							return new UnresolvedMethodOverload(members.Cast<MethodInfo>().ToArray());
						}
						if (members.All((MemberInfo m) => m is PropertyInfo))
						{
							return members[0];
						}
					}
				}
				if (!type2.IsGenericType && identifier == type2.Name)
				{
					return type2;
				}
				string useIdentifier = identifier;
				if (genericArgs != null)
				{
					useIdentifier = genericIdentifier;
				}
				Type nested = null;
				Type current = type2;
				while (nested == null && current != null)
				{
					nested = current.GetNestedType(useIdentifier, BindingFlags.Public | BindingFlags.NonPublic);
					current = current.DeclaringType;
				}
				if (nested != null)
				{
					if (genericArgs != null)
					{
						if (!nested.AreGenericConstraintsSatisfiedBy(genericArgs))
						{
							throw new SyntaxException(ast, "Given generic parameters do not satisfy the generic constraints of type '" + nested.GetNiceFullName() + "'");
						}
						nested = nested.MakeGenericType(genericArgs);
					}
					return nested;
				}
				if (onlyLocals)
				{
					goto IL_0503;
				}
				string @namespace = type2.Namespace ?? "";
				if (IdentifierLookups.TypesByNamespace.TryGetValue(@namespace, out var namespaceTypes))
				{
					Type found;
					if (genericArgs != null)
					{
						if (namespaceTypes.TryGetValue(identifier + "`" + genericArgs.Length, out found))
						{
							if (!found.AreGenericConstraintsSatisfiedBy(genericArgs))
							{
								throw new SyntaxException(ast, "Given generic parameters do not satisfy the generic constraints of type '" + found.GetNiceFullName() + "'");
							}
							return found.MakeGenericType(genericArgs);
						}
					}
					else if (namespaceTypes.TryGetValue(identifier, out found))
					{
						return found;
					}
				}
			}
			else if (context is NamespaceInfo)
			{
				NamespaceInfo namespace2 = context as NamespaceInfo;
				if (namespace2.ChildNamespaces.TryGetValue(identifier, out var childNamespace))
				{
					return childNamespace;
				}
				string useIdentifier2 = identifier;
				if (genericArgs != null)
				{
					useIdentifier2 = identifier + "`" + genericArgs.Length;
				}
				if (IdentifierLookups.TypesByNamespace.TryGetValue(namespace2.FullName, out var namespaceTypes2) && namespaceTypes2.TryGetValue(useIdentifier2, out var type3))
				{
					if (genericArgs != null)
					{
						if (!type3.AreGenericConstraintsSatisfiedBy(genericArgs))
						{
							throw new SyntaxException(ast, "Given generic parameters do not satisfy the generic constraints of type '" + type3.GetNiceFullName() + "'");
						}
						return type3.MakeGenericType(genericArgs);
					}
					return type3;
				}
				throw new SyntaxException(ast, "Could not find identifier '" + identifier + "' in namespace '" + namespace2.FullName + "'");
			}
			if (!onlyLocals)
			{
				if (genericArgs == null && IdentifierLookups.RootNamespaces.TryGetValue(identifier, out var namespace3))
				{
					return namespace3;
				}
				string useIdentifier3 = identifier;
				if (genericArgs != null)
				{
					useIdentifier3 = useIdentifier3 + "`" + genericArgs.Length;
				}
				if (IdentifierLookups.TypesByShortName.TryGetValue(useIdentifier3, out var typesWithShortName))
				{
					if (typesWithShortName.Count == 1)
					{
						if (genericArgs != null)
						{
							if (!typesWithShortName[0].AreGenericConstraintsSatisfiedBy(genericArgs))
							{
								throw new SyntaxException(ast, "Given generic parameters do not satisfy the generic constraints of type '" + typesWithShortName[0].GetNiceFullName() + "'");
							}
							return typesWithShortName[0].MakeGenericType(genericArgs);
						}
						return typesWithShortName[0];
					}
					switch (useIdentifier3)
					{
					case "Debug":
						return typeof(Debug);
					case "Color":
						return typeof(Color);
					case "Units":
						return typeof(Units);
					case "GUIHelper":
						return typeof(GUIHelper);
					}
				}
				if (typesWithShortName != null && typesWithShortName.Count > 1)
				{
					throw new SyntaxException(ast, "There are " + typesWithShortName.Count + " eligible types with the name '" + identifier + "', please fully quality your type name as one of the following: \n\n" + string.Join("\n", typesWithShortName.Select((Type t) => t.GetNiceFullName() + " (" + t.AssemblyQualifiedName + ")").ToArray()));
				}
			}
			goto IL_0503;
			IL_0503:
			if (context != null)
			{
				if (context is Type)
				{
					throw new SyntaxException(ast, "Unable to locate identifier '" + identifier + "' in context of type '" + (context as Type).GetNiceFullName() + "' (Note that extension methods are not supported)");
				}
				if (context is NamespaceInfo)
				{
					throw new SyntaxException(ast, "Unable to locate identifier '" + identifier + "' in context of namespace '" + (context as NamespaceInfo).FullName + "' (Note that extension methods are not supported)");
				}
			}
			throw new SyntaxException(ast, "Unable to locate identifier '" + identifier + "' (Note that extension methods are not supported)");
		}

		private static bool IsMemberCompatibleWithGenericArgs(MemberInfo member, Type[] args, out MemberInfo genericResult)
		{
			if (member is Type)
			{
				Type type = member as Type;
				if (type.IsGenericType && type.AreGenericConstraintsSatisfiedBy(args))
				{
					genericResult = type.MakeGenericType(args);
					return true;
				}
			}
			else if (member is MethodInfo)
			{
				MethodInfo method = member as MethodInfo;
				if (method.IsGenericMethod && method.AreGenericConstraintsSatisfiedBy(args))
				{
					genericResult = method.MakeGenericMethod(args);
					return true;
				}
			}
			genericResult = null;
			return false;
		}

		private static bool CanAccessOverloadCandidate(MethodBase candidate, bool allowInstanceAccess, bool allowStaticAccess)
		{
			if (candidate is ConstructorInfo)
			{
				return true;
			}
			if (candidate.IsStatic)
			{
				return allowStaticAccess;
			}
			return allowInstanceAccess;
		}

		private static MethodBase FindOverload(ASTNode ast, MethodBase[] candidates, Type[] parameters, bool allowInstanceAccess, bool allowStaticAccess)
		{
			foreach (MethodBase candidate in candidates)
			{
				if (!CanAccessOverloadCandidate(candidate, allowInstanceAccess, allowStaticAccess))
				{
					continue;
				}
				ParameterInfo[] candidateParams = candidate.GetParameters();
				if (candidateParams.Length != parameters.Length)
				{
					continue;
				}
				bool valid = true;
				for (int j = 0; j < parameters.Length; j++)
				{
					if (candidateParams[j].ParameterType != parameters[j])
					{
						valid = false;
						break;
					}
				}
				if (valid)
				{
					return candidate;
				}
			}
			foreach (MethodBase candidate2 in candidates)
			{
				if (!CanAccessOverloadCandidate(candidate2, allowInstanceAccess, allowStaticAccess))
				{
					continue;
				}
				MethodInfo methodInfo = candidate2 as MethodInfo;
				if (methodInfo == null || !candidate2.IsGenericMethodDefinition)
				{
					continue;
				}
				ParameterInfo[] candidateParams2 = candidate2.GetParameters();
				if (candidateParams2.Length != parameters.Length)
				{
					continue;
				}
				bool valid2 = true;
				lock (GenericResolutionMap)
				{
					GenericResolutionMap.Clear();
					for (int l = 0; l < parameters.Length; l++)
					{
						Type candParam = candidateParams2[l].ParameterType;
						Type givenParam = parameters[l];
						if (candParam.IsGenericParameter)
						{
							if (!candParam.GenericParameterIsFulfilledBy(givenParam))
							{
								valid2 = false;
								break;
							}
							if (GenericResolutionMap.ContainsKey(candParam) && GenericResolutionMap[candParam] != givenParam)
							{
								valid2 = false;
								break;
							}
							GenericResolutionMap[candParam] = givenParam;
						}
						else if (!candParam.IsAssignableFrom(givenParam))
						{
							valid2 = false;
							break;
						}
					}
					if (!valid2)
					{
						continue;
					}
					Type[] genericArgs = candidate2.GetGenericArguments();
					if (GenericResolutionMap.Count != genericArgs.Length)
					{
						continue;
					}
					for (int m = 0; m < genericArgs.Length; m++)
					{
						genericArgs[m] = GenericResolutionMap[genericArgs[m]];
					}
					return methodInfo.MakeGenericMethod(genericArgs);
				}
			}
			lock (OverloadScores)
			{
				OverloadScores.Clear();
				foreach (MethodBase candidate3 in candidates)
				{
					if (!CanAccessOverloadCandidate(candidate3, allowInstanceAccess, allowStaticAccess))
					{
						continue;
					}
					ParameterInfo[] candidateParams3 = candidate3.GetParameters();
					if (candidateParams3.Length != parameters.Length)
					{
						continue;
					}
					OverloadScore score = new OverloadScore
					{
						Method = candidate3
					};
					bool valid3 = true;
					for (int num = 0; num < parameters.Length; num++)
					{
						Type candParam2 = candidateParams3[num].ParameterType;
						Type givenParam2 = parameters[num];
						if (!candParam2.IsValueType && givenParam2 == typeof(NullParameter))
						{
							continue;
						}
						if (candParam2.IsAssignableFrom(givenParam2))
						{
							score.Score += givenParam2.GetInheritanceDistance(candParam2) + 1;
							continue;
						}
						if (givenParam2.IsCastableTo(candParam2, requireImplicitCast: true))
						{
							score.Score++;
							continue;
						}
						valid3 = false;
						break;
					}
					if (valid3)
					{
						OverloadScores.Add(score);
					}
				}
				if (OverloadScores.Count > 0)
				{
					if (OverloadScores.Count > 1)
					{
						OverloadScores.Sort((OverloadScore a, OverloadScore b) => a.Score.CompareTo(b.Score));
						if (OverloadScores[0].Score == OverloadScores[1].Score)
						{
							throw new SyntaxException(ast, "Ambiguous method overload match");
						}
					}
					return OverloadScores[0].Method;
				}
			}
			return null;
		}

		private static MethodInfo InferGenericMethod(ASTNode ast, MethodInfo method, Type[] parameters)
		{
			if (!method.IsGenericMethodDefinition)
			{
				throw new ArgumentException();
			}
			ParameterInfo[] methodParams = method.GetParameters();
			if (parameters.Length != methodParams.Length)
			{
				throw new SyntaxException(ast, "Wrong number of parameters given to generic method; expected " + methodParams.Length + " but got " + parameters.Length);
			}
			lock (GenericResolutionMap)
			{
				GenericResolutionMap.Clear();
				for (int j = 0; j < parameters.Length; j++)
				{
					ParameterInfo candParam = methodParams[j];
					Type givenParam = parameters[j];
					if (candParam.ParameterType.IsGenericParameter)
					{
						if (!candParam.ParameterType.GenericParameterIsFulfilledBy(givenParam))
						{
							throw new SyntaxException(ast, "The constraints of generic parameter '" + candParam.Name + "' are not fulfilled by given type '" + givenParam.GetNiceFullName() + "'. The constraints are: " + candParam.ParameterType.GetGenericParameterConstraintsString());
						}
						if (GenericResolutionMap.ContainsKey(candParam.ParameterType) && GenericResolutionMap[candParam.ParameterType] != givenParam)
						{
							throw new SyntaxException(ast, "The generic parameter '" + candParam.Name + "' cannot be inferred to be both type '" + GenericResolutionMap[candParam.ParameterType].GetNiceFullName() + "' and '" + givenParam.GetNiceFullName() + "'.");
						}
						GenericResolutionMap[candParam.ParameterType] = givenParam;
					}
					else if (!candParam.ParameterType.IsAssignableFrom(givenParam))
					{
						throw new SyntaxException(ast, "The given argument '" + givenParam.GetNiceFullName() + "' cannot be assigned to parameter '" + candParam.Name + "' of type '" + candParam.ParameterType.GetNiceFullName() + "'.");
					}
				}
				Type[] genericArgs = method.GetGenericArguments();
				if (GenericResolutionMap.Count == genericArgs.Length)
				{
					for (int i = 0; i < genericArgs.Length; i++)
					{
						genericArgs[i] = GenericResolutionMap[genericArgs[i]];
					}
					return method.MakeGenericMethod(genericArgs);
				}
			}
			throw new SyntaxException(ast, "Could not infer generic usage of method from given parameters");
		}

		private static bool Is64BitInteger(Type type)
		{
			EnumConv(ref type);
			if (!(type == typeof(long)))
			{
				return type == typeof(ulong);
			}
			return true;
		}

		private static bool Is32BitOrLessInteger(Type type)
		{
			EnumConv(ref type);
			if (!(type == typeof(sbyte)) && !(type == typeof(short)) && !(type == typeof(int)) && !(type == typeof(byte)) && !(type == typeof(ushort)))
			{
				return type == typeof(uint);
			}
			return true;
		}

		private static bool IsSignedInteger(Type type)
		{
			EnumConv(ref type);
			if (!(type == typeof(sbyte)) && !(type == typeof(short)) && !(type == typeof(int)))
			{
				return type == typeof(long);
			}
			return true;
		}

		private static bool IsAnyInteger(Type type)
		{
			EnumConv(ref type);
			if (!Is64BitInteger(type))
			{
				return Is32BitOrLessInteger(type);
			}
			return true;
		}

		private static bool IsFloatingPoint(Type type)
		{
			EnumConv(ref type);
			if (!(type == typeof(float)))
			{
				return type == typeof(double);
			}
			return true;
		}

		private static bool IsAnyNumber(Type type)
		{
			EnumConv(ref type);
			if (!IsAnyInteger(type))
			{
				return IsFloatingPoint(type);
			}
			return true;
		}

		private static bool IntegersHaveSameSign(Type a, Type b)
		{
			EnumConv(ref a);
			EnumConv(ref b);
			if (IsAnyInteger(a) && IsAnyInteger(b))
			{
				return IsSignedInteger(a) == IsSignedInteger(b);
			}
			return false;
		}

		private static int GetBitCountOfNumber(Type type)
		{
			EnumConv(ref type);
			if (type == typeof(byte) || type == typeof(sbyte))
			{
				return 8;
			}
			if (type == typeof(short) || type == typeof(ushort))
			{
				return 16;
			}
			if (type == typeof(int) || type == typeof(uint) || type == typeof(float))
			{
				return 32;
			}
			if (type == typeof(long) || type == typeof(ulong) || type == typeof(double))
			{
				return 64;
			}
			throw new ArgumentException("Type '" + type.FullName + "' is not a number.");
		}

		private static void EnumConv(ref Type e)
		{
			if (e.IsEnum)
			{
				e = Enum.GetUnderlyingType(e);
			}
		}

		private Type GetAndValidateNumberMathOperationResult(ASTNode ast, Operator op, Type a, Type b)
		{
			EnumConv(ref a);
			EnumConv(ref b);
			if (IsNumberBooleanOperator(op))
			{
				return typeof(bool);
			}
			int aBits = GetBitCountOfNumber(a);
			int bBits = GetBitCountOfNumber(b);
			if (IsBitwiseOperator(op))
			{
				if (op == Operator.BitwiseComplement)
				{
					throw new NotSupportedException();
				}
				if (IsFloatingPoint(a) || IsFloatingPoint(b))
				{
					throw new SyntaxException(ast, "Cannot use bitwise operations on floating point numbers");
				}
				if (op == Operator.RightShift || op == Operator.LeftShift)
				{
					if (!Is32BitOrLessInteger(b) || b == typeof(uint))
					{
						throw new SyntaxException(ast, "Cannot bitshift by a number of type '" + b.GetNiceName() + "'");
					}
					return a;
				}
				if (a == b)
				{
					return a;
				}
				if (aBits == 64 && bBits == 64 && !IntegersHaveSameSign(a, b))
				{
					throw new SyntaxException(ast, "Cannot apply bitwise and/or/exclusive or operations to numbers of type 'long' and 'ulong'");
				}
				if (aBits == 64 || bBits == 64)
				{
					return a;
				}
				if (aBits < 32 && bBits < 32)
				{
					return typeof(int);
				}
				bool aSigned = IsSignedInteger(a);
				bool bSigned = IsSignedInteger(b);
				if (aSigned && bSigned)
				{
					return typeof(int);
				}
				if (!aSigned && !bSigned)
				{
					return typeof(uint);
				}
				return typeof(long);
			}
			if (!IsMathOperator(op))
			{
				throw new NotImplementedException(op.ToString());
			}
			Type highestBits = ((aBits > bBits) ? a : b);
			if (IsFloatingPoint(a))
			{
				if (IsFloatingPoint(b))
				{
					return highestBits;
				}
				return a;
			}
			if (IsFloatingPoint(b))
			{
				if (IsFloatingPoint(a))
				{
					return highestBits;
				}
				return b;
			}
			if (!IntegersHaveSameSign(a, b))
			{
				if (aBits == 32 && bBits == 64 && !IsSignedInteger(b))
				{
					return b;
				}
				if (aBits == 64 && bBits == 32 && !IsSignedInteger(a))
				{
					if (aBits <= bBits)
					{
						return b;
					}
					return a;
				}
				throw new ArgumentException("Math operator (+-/*%) is ambiguous on operands of type '" + a.GetNiceName() + "' and '" + b.GetNiceName() + "'");
			}
			if (aBits <= bBits)
			{
				return b;
			}
			return a;
		}

		private static void EmitConvertToNumber(ILGenerator il, Type to)
		{
			EnumConv(ref to);
			if (to == typeof(float))
			{
				il.Emit(OpCodes.Conv_R4);
				return;
			}
			if (to == typeof(double))
			{
				il.Emit(OpCodes.Conv_R8);
				return;
			}
			if (to == typeof(sbyte))
			{
				il.Emit(OpCodes.Conv_I1);
				return;
			}
			if (to == typeof(short))
			{
				il.Emit(OpCodes.Conv_I2);
				return;
			}
			if (to == typeof(int))
			{
				il.Emit(OpCodes.Conv_I4);
				return;
			}
			if (to == typeof(long))
			{
				il.Emit(OpCodes.Conv_I8);
				return;
			}
			if (to == typeof(byte))
			{
				il.Emit(OpCodes.Conv_U1);
				return;
			}
			if (to == typeof(ushort))
			{
				il.Emit(OpCodes.Conv_U2);
				return;
			}
			if (to == typeof(uint))
			{
				il.Emit(OpCodes.Conv_U4);
				return;
			}
			if (to == typeof(ulong))
			{
				il.Emit(OpCodes.Conv_U8);
				return;
			}
			throw new NotImplementedException(to.ToString());
		}

		private static void EmitOperatorOpCodes(ILGenerator il, Operator op)
		{
			switch (op)
			{
			case Operator.Equality:
				il.Emit(OpCodes.Ceq);
				break;
			case Operator.Inequality:
				il.Emit(OpCodes.Ceq);
				il.Emit(OpCodes.Ldc_I4_0);
				il.Emit(OpCodes.Ceq);
				break;
			case Operator.Addition:
				il.Emit(OpCodes.Add);
				break;
			case Operator.Subtraction:
				il.Emit(OpCodes.Sub);
				break;
			case Operator.Multiply:
				il.Emit(OpCodes.Mul);
				break;
			case Operator.Division:
				il.Emit(OpCodes.Div);
				break;
			case Operator.LessThan:
				il.Emit(OpCodes.Clt);
				break;
			case Operator.GreaterThan:
				il.Emit(OpCodes.Cgt);
				break;
			case Operator.LessThanOrEqual:
				il.Emit(OpCodes.Cgt);
				il.Emit(OpCodes.Ldc_I4_0);
				il.Emit(OpCodes.Ceq);
				break;
			case Operator.GreaterThanOrEqual:
				il.Emit(OpCodes.Clt);
				il.Emit(OpCodes.Ldc_I4_0);
				il.Emit(OpCodes.Ceq);
				break;
			case Operator.Modulus:
				il.Emit(OpCodes.Rem);
				break;
			case Operator.RightShift:
				il.Emit(OpCodes.Shr);
				break;
			case Operator.LeftShift:
				il.Emit(OpCodes.Shl);
				break;
			case Operator.BitwiseAnd:
			case Operator.LogicalAnd:
				il.Emit(OpCodes.And);
				break;
			case Operator.BitwiseOr:
			case Operator.LogicalOr:
				il.Emit(OpCodes.Or);
				break;
			case Operator.BitwiseComplement:
				il.Emit(OpCodes.Not);
				break;
			case Operator.ExclusiveOr:
				il.Emit(OpCodes.Xor);
				break;
			default:
				throw new NotImplementedException(op.ToString());
			}
		}

		private void EmitOperator(ASTNode ast, Operator op, ASTNode leftAst, ASTNode rightAst, out Type resultType, bool useAwfulImplicitCastHack = false, Type leftOverride = null, Type rightOverride = null)
		{
			if (op == Operator.BitwiseComplement)
			{
				throw new NotSupportedException();
			}
			Type left;
			Type right;
			if (!useAwfulImplicitCastHack)
			{
				Visit(leftAst);
				Visit(rightAst);
				left = leftAst.TypeOfValue;
				right = rightAst.TypeOfValue;
			}
			else
			{
				left = leftOverride;
				right = rightOverride;
			}
			if (op == Operator.Addition)
			{
				if (left == typeof(string) && right == typeof(string))
				{
					actions.Add(delegate(ILGenerator il)
					{
						il.Emit(OpCodes.Call, CommonMembers.String_Concat);
					});
					resultType = typeof(string);
					return;
				}
				if (left == typeof(string))
				{
					actions.Add(delegate(ILGenerator il)
					{
						if (right.IsValueType)
						{
							il.Emit(OpCodes.Box, right);
						}
						il.Emit(OpCodes.Callvirt, CommonMembers.Object_ToString);
						il.Emit(OpCodes.Call, CommonMembers.String_Concat);
					});
					resultType = typeof(string);
					return;
				}
				if (right == typeof(string))
				{
					actions.Add(delegate(ILGenerator il)
					{
						LocalBuilder local = il.DeclareLocal(typeof(string));
						il.Emit(OpCodes.Stloc, local);
						if (left.IsValueType)
						{
							il.Emit(OpCodes.Box, left);
						}
						il.Emit(OpCodes.Callvirt, CommonMembers.Object_ToString);
						il.Emit(OpCodes.Ldloc, local);
						il.Emit(OpCodes.Call, CommonMembers.String_Concat);
					});
					resultType = typeof(string);
					return;
				}
			}
			if (IsAnyNumber(left) && IsAnyNumber(right))
			{
				Type convertTo;
				if (left.IsEnum && right.IsEnum)
				{
					if (left != right)
					{
						throw new SyntaxException(ast, "Cannot use the " + op.ToString() + " operator on enums of differing types");
					}
					if (IsNumberBooleanOperator(op))
					{
						resultType = typeof(bool);
					}
					else
					{
						resultType = left;
					}
					convertTo = left;
					EnumConv(ref convertTo);
				}
				else if (left.IsEnum && !right.IsEnum)
				{
					if (IsFloatingPoint(right))
					{
						throw new SyntaxException(ast, "Cannot perform math operations with enums and floating points numbers");
					}
					Type enumNumberType = Enum.GetUnderlyingType(left);
					Type numResult = GetAndValidateNumberMathOperationResult(ast, op, enumNumberType, right);
					if (numResult != enumNumberType && !IntegersHaveSameSign(numResult, enumNumberType))
					{
						throw new SyntaxException(ast, "Cannot perform math operations with an enum of the underlying type '" + enumNumberType.GetNiceName() + "' and '" + right?.ToString() + "'");
					}
					convertTo = enumNumberType;
					resultType = left;
				}
				else
				{
					if (!left.IsEnum && right.IsEnum)
					{
						throw new SyntaxException(ast, "Can only do math operations with enums if the enum is on the left-hand side");
					}
					resultType = GetAndValidateNumberMathOperationResult(ast, op, left, right);
					convertTo = resultType;
				}
				if (IsNumberBooleanOperator(op))
				{
					if (IsFloatingPoint(left) && IsAnyInteger(right))
					{
						actions.Add(delegate(ILGenerator il)
						{
							EmitConvertToNumber(il, left);
							EmitOperatorOpCodes(il, op);
						});
					}
					else if (IsAnyInteger(left) && IsFloatingPoint(right))
					{
						actions.Add(delegate(ILGenerator il)
						{
							LocalBuilder local = il.DeclareLocal(right);
							il.Emit(OpCodes.Stloc, local);
							EmitConvertToNumber(il, right);
							il.Emit(OpCodes.Ldloc, local);
							EmitOperatorOpCodes(il, op);
						});
					}
					else
					{
						actions.Add(delegate(ILGenerator il)
						{
							EmitOperatorOpCodes(il, op);
						});
					}
					return;
				}
				if (IsBitwiseOperator(op))
				{
					actions.Add(delegate(ILGenerator il)
					{
						EmitOperatorOpCodes(il, op);
					});
					return;
				}
				actions.Add(delegate(ILGenerator il)
				{
					if (left != right)
					{
						LocalBuilder local = il.DeclareLocal(right);
						il.Emit(OpCodes.Stloc, local);
						EmitConvertToNumber(il, convertTo);
						il.Emit(OpCodes.Ldloc, local);
						EmitConvertToNumber(il, convertTo);
					}
					EmitOperatorOpCodes(il, op);
				});
				return;
			}
			if (IsLogicalBooleanOperator(op) && left == typeof(bool) && right == typeof(bool))
			{
				resultType = typeof(bool);
				actions.Add(delegate(ILGenerator il)
				{
					EmitOperatorOpCodes(il, op);
				});
				return;
			}
			if (!left.IsValueType && IsConstantNull(rightAst))
			{
				right = left;
			}
			else if (!right.IsValueType && IsConstantNull(leftAst))
			{
				left = right;
			}
			MethodInfo operatorMethod = GetOperatorMethod(op, left, right);
			if (operatorMethod != null)
			{
				actions.Add(delegate(ILGenerator il)
				{
					EmitConvertParamsIfNecessary(ast, il, operatorMethod, left, right);
					EmitMethodCall(il, operatorMethod, null);
				});
				resultType = operatorMethod.ReturnType;
				return;
			}
			if (right != left)
			{
				operatorMethod = GetOperatorMethod(op, right, left);
				if (operatorMethod != null)
				{
					actions.Add(delegate(ILGenerator il)
					{
						LocalBuilder local = il.DeclareLocal(left);
						LocalBuilder local2 = il.DeclareLocal(right);
						il.Emit(OpCodes.Stloc, local2);
						il.Emit(OpCodes.Stloc, local);
						il.Emit(OpCodes.Ldloc, local2);
						il.Emit(OpCodes.Ldloc, local);
						EmitConvertParamsIfNecessary(ast, il, operatorMethod, right, left);
						EmitMethodCall(il, operatorMethod, null);
					});
					resultType = operatorMethod.ReturnType;
					return;
				}
			}
			if ((left == typeof(object) || right == typeof(object)) && (op == Operator.Equality || op == Operator.Inequality))
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Ceq);
					if (op == Operator.Inequality)
					{
						il.Emit(OpCodes.Ldc_I4_0);
						il.Emit(OpCodes.Ceq);
					}
				});
				resultType = typeof(bool);
				return;
			}
			if (!useAwfulImplicitCastHack)
			{
				for (int i = 0; i < 2; i++)
				{
					Type castedOperand = ((i == 0) ? left : right);
					Type nonCastedOperand = ((i == 0) ? right : left);
					List<Type> casts = GetImplicitTypeCastTargets(castedOperand);
					if (casts == null)
					{
						continue;
					}
					foreach (Type cast in casts)
					{
						int actionsCountBefore = actions.Count;
						Type tempResultType;
						try
						{
							EmitOperator(ast, op, leftAst, rightAst, out tempResultType, useAwfulImplicitCastHack: true, (i == 0) ? cast : nonCastedOperand, (i == 0) ? nonCastedOperand : cast);
						}
						catch (SyntaxException)
						{
							while (actions.Count > actionsCountBefore)
							{
								actions.RemoveAt(actions.Count - 1);
							}
							continue;
						}
						if (i == 0)
						{
							actions.Insert(actionsCountBefore, delegate(ILGenerator il)
							{
								LocalBuilder local = il.DeclareLocal(right);
								il.Emit(OpCodes.Stloc, local);
								EmitTypeConversion(rightAst, il, left, cast);
								il.Emit(OpCodes.Ldloc, local);
							});
						}
						else
						{
							actions.Insert(actionsCountBefore, delegate(ILGenerator il)
							{
								EmitTypeConversion(rightAst, il, right, cast);
							});
						}
						resultType = tempResultType;
						return;
					}
				}
			}
			if ((op == Operator.Equality || op == Operator.Inequality) && !left.IsValueType && !right.IsValueType)
			{
				actions.Add(delegate(ILGenerator il)
				{
					il.Emit(OpCodes.Ceq);
					if (op == Operator.Inequality)
					{
						il.Emit(OpCodes.Ldc_I4_0);
						il.Emit(OpCodes.Ceq);
					}
				});
				resultType = typeof(bool);
				return;
			}
			throw new SyntaxException(ast, "No valid " + op.ToString().ToLower() + " operators found for types '" + left.GetNiceFullName() + "' and '" + right.GetNiceFullName() + "'");
		}

		private static MethodInfo GetOperatorMethod(Operator op, Type left, Type right)
		{
			if (!IsOverridableOperator(op))
			{
				return null;
			}
			return left.GetOperatorMethods(op).FirstOrDefault((MethodInfo m) => IsValidOperatorFor(m, left, right)) ?? right.GetOperatorMethods(op).FirstOrDefault((MethodInfo m) => IsValidOperatorFor(m, left, right));
		}

		private static bool IsValidOperatorFor(MethodInfo method, Type left, Type right)
		{
			ParameterInfo[] parameters = method.GetParameters();
			if (parameters.Length != 2)
			{
				return false;
			}
			if (method.ReturnType == null || method.ReturnType == typeof(void))
			{
				return false;
			}
			if (!left.IsCastableTo(parameters[0].ParameterType, requireImplicitCast: true))
			{
				return false;
			}
			if (!right.IsCastableTo(parameters[1].ParameterType, requireImplicitCast: true))
			{
				return false;
			}
			return true;
		}

		private static bool IsLogicalBooleanOperator(Operator op)
		{
			if ((uint)op <= 1u || op == Operator.ExclusiveOr || (uint)(op - 17) <= 1u)
			{
				return true;
			}
			return false;
		}

		private static bool IsNumberBooleanOperator(Operator op)
		{
			if ((uint)op <= 1u || (uint)(op - 6) <= 3u)
			{
				return true;
			}
			return false;
		}

		private static bool IsMathOperator(Operator op)
		{
			if ((uint)(op - 2) <= 3u || op == Operator.Modulus)
			{
				return true;
			}
			return false;
		}

		private static bool IsBitwiseOperator(Operator op)
		{
			if ((uint)(op - 11) <= 5u)
			{
				return true;
			}
			return false;
		}

		private static bool IsOverridableOperator(Operator op)
		{
			if ((uint)(op - 8) <= 1u || (uint)(op - 17) <= 1u)
			{
				return false;
			}
			return true;
		}

		private void EmitConvertParamsIfNecessary(ASTNode ast, ILGenerator il, MethodBase method, params Type[] args)
		{
			ParameterInfo[] methodParams = method.GetParameters();
			bool anyConversionNecessary = false;
			for (int i = 0; i < args.Length; i++)
			{
				Type param = methodParams[i].ParameterType;
				Type arg = args[i];
				if (!(param == arg) && (param.IsValueType || arg.IsValueType))
				{
					anyConversionNecessary = true;
				}
			}
			if (anyConversionNecessary)
			{
				LocalBuilder[] locals = new LocalBuilder[args.Length];
				for (int j = 0; j < args.Length; j++)
				{
					locals[j] = il.DeclareLocal(args[j]);
				}
				for (int i2 = args.Length - 1; i2 >= 0; i2--)
				{
					il.Emit(OpCodes.Stloc, locals[i2]);
				}
				for (int k = 0; k < args.Length; k++)
				{
					il.Emit(OpCodes.Ldloc, locals[k]);
					EmitTypeConversion(ast, il, args[k], methodParams[k].ParameterType);
				}
			}
		}

		private static void EmitTypeConversion(ASTNode ast, ILGenerator il, Type from, Type to)
		{
			if (from == to || from == typeof(NullParameter))
			{
				return;
			}
			if (IsAnyNumber(from) && IsAnyNumber(to))
			{
				EmitConvertToNumber(il, to);
				return;
			}
			if (from.IsValueType)
			{
				if (to == typeof(object) || to == typeof(ValueType) || to.IsInterface || (from.IsEnum && to == typeof(Enum)))
				{
					il.Emit(OpCodes.Box, from);
					return;
				}
			}
			else if (to.IsAssignableFrom(from))
			{
				return;
			}
			MethodInfo method = from.GetCastMethod(to, requireImplicitCast: true);
			if (method == null)
			{
				throw new SyntaxException(ast, "The type '" + from.GetNiceFullName() + "' is not implicitly castable to type '" + to.GetNiceFullName() + "'");
			}
			EmitMethodCall(il, method, null);
		}

		private static void EmitMethodCall(ILGenerator il, MethodInfo method, Type instanceType, bool isBaseAccess = false, bool fixValueTypeLoading = true)
		{
			if (method.IsStatic)
			{
				il.Emit(OpCodes.Call, method);
				return;
			}
			if (instanceType == null)
			{
				instanceType = method.DeclaringType;
			}
			if (fixValueTypeLoading && instanceType.IsValueType)
			{
				LocalBuilder loc = il.DeclareLocal(instanceType);
				ParameterInfo[] parameters = method.GetParameters();
				LocalBuilder[] paramLocals = null;
				if (parameters.Length != 0)
				{
					paramLocals = new LocalBuilder[parameters.Length];
					for (int i = 0; i < parameters.Length; i++)
					{
						paramLocals[i] = il.DeclareLocal(parameters[i].ParameterType);
					}
					for (int i2 = parameters.Length - 1; i2 >= 0; i2--)
					{
						il.Emit(OpCodes.Stloc, paramLocals[i2]);
					}
				}
				il.Emit(OpCodes.Stloc, loc);
				il.Emit(OpCodes.Ldloca, loc);
				if (parameters.Length != 0)
				{
					for (int j = 0; j < parameters.Length; j++)
					{
						il.Emit(OpCodes.Ldloc, paramLocals[j]);
					}
				}
			}
			if (instanceType.IsEnum && instanceType.IsValueType)
			{
				il.Emit(OpCodes.Constrained, instanceType);
			}
			if (!isBaseAccess && (method.IsVirtual || method.IsAbstract))
			{
				il.Emit(OpCodes.Callvirt, method);
			}
			else
			{
				il.Emit(OpCodes.Call, method);
			}
		}

		private static void EmitConstant(ILGenerator il, object constant, Type type = null)
		{
			if (constant == null)
			{
				il.Emit(OpCodes.Ldnull);
				return;
			}
			if (type == null)
			{
				type = constant.GetType();
			}
			if (type == typeof(int) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort))
			{
				il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(constant));
			}
			else if (type == typeof(uint))
			{
				il.Emit(OpCodes.Ldc_I4, (int)(uint)constant);
			}
			else if (type == typeof(long))
			{
				il.Emit(OpCodes.Ldc_I8, (long)constant);
			}
			else if (type == typeof(ulong))
			{
				il.Emit(OpCodes.Ldc_I8, (long)(ulong)constant);
			}
			else if (type == typeof(float))
			{
				il.Emit(OpCodes.Ldc_R4, (float)constant);
			}
			else if (type == typeof(double))
			{
				il.Emit(OpCodes.Ldc_R8, (double)constant);
			}
			else if (type == typeof(string))
			{
				il.Emit(OpCodes.Ldstr, (string)constant);
			}
			else if (type == typeof(char))
			{
				il.Emit(OpCodes.Ldc_I4, (char)constant);
			}
			else if (type == typeof(decimal))
			{
				int[] bits = decimal.GetBits((decimal)constant);
				ConstructorInfo constructor = typeof(decimal).GetConstructor(new Type[1] { typeof(int[]) });
				LocalBuilder arrLocal = il.DeclareLocal(typeof(int[]));
				il.Emit(OpCodes.Ldc_I4, bits.Length);
				il.Emit(OpCodes.Newarr, typeof(int));
				il.Emit(OpCodes.Stloc, arrLocal);
				for (int i = 0; i < bits.Length; i++)
				{
					il.Emit(OpCodes.Ldloc, arrLocal);
					il.Emit(OpCodes.Ldc_I4, i);
					il.Emit(OpCodes.Ldc_I4, bits[i]);
					il.Emit(OpCodes.Stelem_I4);
				}
				il.Emit(OpCodes.Ldloc, arrLocal);
				il.Emit(OpCodes.Newobj, constructor);
			}
			else if (type == typeof(bool))
			{
				il.Emit(((bool)constant) ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
			}
			else
			{
				if (!type.IsEnum)
				{
					throw new NotSupportedException("Type " + type.GetNiceFullName() + " is not supported as a constant.");
				}
				EmitConstant(il, constant, Enum.GetUnderlyingType(type));
			}
		}

		private static void EmitEnumField(ILGenerator il, FieldInfo field)
		{
			object value = field.GetValue(null);
			Type num = Enum.GetUnderlyingType(field.DeclaringType);
			if (num == typeof(ulong))
			{
				il.Emit(OpCodes.Ldc_I8, (long)(ulong)value);
			}
			else if (num == typeof(long))
			{
				il.Emit(OpCodes.Ldc_I8, (long)value);
			}
			else if (num == typeof(uint))
			{
				il.Emit(OpCodes.Ldc_I4, (int)(uint)value);
			}
			else
			{
				il.Emit(OpCodes.Ldc_I4, Convert.ToInt32(value));
			}
		}

		private static bool IsConstantNull(ASTNode node)
		{
			while (node.NodeType == NodeType.PARENTHESIZED_EXPRESSION)
			{
				node = node.Children[0];
			}
			return node.NodeType == NodeType.CONSTANT_NULL;
		}

		private static List<Type> GetImplicitTypeCastTargets(Type type)
		{
			if (type == typeof(object))
			{
				return null;
			}
			List<Type> result = null;
			IEnumerable<MethodInfo> methods = type.GetAllMembers<MethodInfo>(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo method in methods)
			{
				if (method.Name != "op_Implicit")
				{
					continue;
				}
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters.Length == 1 && !(parameters[0].ParameterType != method.DeclaringType))
				{
					if (result == null)
					{
						result = new List<Type>();
					}
					result.Add(method.ReturnType);
				}
			}
			return result;
		}

		protected override void Nop(ASTNode ast)
		{
			actions.Add(delegate(ILGenerator il)
			{
				il.Emit(OpCodes.Nop);
			});
			ast.TypeOfValue = typeof(void);
		}
	}
}
