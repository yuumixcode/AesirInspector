using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Sirenix.Serialization.Utilities;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Serialization
{
	/// <summary>
	/// Utility class for locating and caching formatters for all non-primitive types.
	/// </summary>
	[InitializeOnLoad]
	public static class FormatterLocator
	{
		private struct FormatterInfo
		{
			public Type FormatterType;

			public Type TargetType;

			public Type WeakFallbackType;

			public bool AskIfCanFormatTypes;

			public int Priority;
		}

		private struct FormatterLocatorInfo
		{
			public IFormatterLocator LocatorInstance;

			public int Priority;
		}

		private static readonly object StrongFormatters_LOCK;

		private static readonly object WeakFormatters_LOCK;

		private static readonly Dictionary<Type, IFormatter> FormatterInstances;

		private static readonly DoubleLookupDictionary<Type, ISerializationPolicy, IFormatter> StrongTypeFormatterMap;

		private static readonly DoubleLookupDictionary<Type, ISerializationPolicy, IFormatter> WeakTypeFormatterMap;

		private static readonly List<FormatterLocatorInfo> FormatterLocators;

		private static readonly List<FormatterInfo> FormatterInfos;

		/// <summary>
		/// Editor-only event that fires whenever an emittable formatter has been located.
		/// This event is used by the AOT formatter pre-emitter to locate types that need to have formatters pre-emitted.
		/// </summary>
		public static event Action<Type> OnLocatedEmittableFormatterForType;

		/// <summary>
		/// Editor-only event that fires whenever a formatter has been located.
		/// </summary>
		public static event Action<IFormatter> OnLocatedFormatter;

		/// <summary>
		/// This event is invoked before everything else when a formatter is being resolved for a given type. If any invoked delegate returns a valid formatter, that formatter is used and the resolve process stops there.
		/// <para />
		/// This can be used to hook into and extend the serialization system's formatter resolution logic.
		/// </summary>
		[Obsolete("Use the new IFormatterLocator interface instead, and register your custom locator with the RegisterFormatterLocator assembly attribute.", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static event Func<Type, IFormatter> FormatterResolve
		{
			add
			{
				throw new NotSupportedException();
			}
			remove
			{
				throw new NotSupportedException();
			}
		}

		static FormatterLocator()
		{
			StrongFormatters_LOCK = new object();
			WeakFormatters_LOCK = new object();
			FormatterInstances = new Dictionary<Type, IFormatter>(FastTypeComparer.Instance);
			StrongTypeFormatterMap = new DoubleLookupDictionary<Type, ISerializationPolicy, IFormatter>(FastTypeComparer.Instance, ReferenceEqualityComparer<ISerializationPolicy>.Default);
			WeakTypeFormatterMap = new DoubleLookupDictionary<Type, ISerializationPolicy, IFormatter>(FastTypeComparer.Instance, ReferenceEqualityComparer<ISerializationPolicy>.Default);
			FormatterLocators = new List<FormatterLocatorInfo>();
			FormatterInfos = new List<FormatterInfo>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly ass in assemblies)
			{
				try
				{
					string name = ass.GetName().Name;
					if (name.StartsWith("System.") || name.StartsWith("UnityEngine") || name.StartsWith("UnityEditor") || name == "mscorlib" || ass.GetName().Name == "Sirenix.Serialization.AOTGenerated" || ass.SafeIsDefined(typeof(EmittedAssemblyAttribute), inherit: true))
					{
						continue;
					}
					object[] array = ass.SafeGetCustomAttributes(typeof(RegisterFormatterAttribute), inherit: true);
					foreach (object attrUncast in array)
					{
						RegisterFormatterAttribute attr = (RegisterFormatterAttribute)attrUncast;
						if (attr.FormatterType.IsClass && !attr.FormatterType.IsAbstract && !(attr.FormatterType.GetConstructor(Type.EmptyTypes) == null) && attr.FormatterType.ImplementsOpenGenericInterface(typeof(IFormatter<>)))
						{
							FormatterInfos.Add(new FormatterInfo
							{
								FormatterType = attr.FormatterType,
								WeakFallbackType = attr.WeakFallback,
								TargetType = attr.FormatterType.GetArgumentsOfInheritedOpenGenericInterface(typeof(IFormatter<>))[0],
								AskIfCanFormatTypes = typeof(IAskIfCanFormatTypes).IsAssignableFrom(attr.FormatterType),
								Priority = attr.Priority
							});
						}
					}
					object[] array2 = ass.SafeGetCustomAttributes(typeof(RegisterFormatterLocatorAttribute), inherit: true);
					foreach (object attrUncast2 in array2)
					{
						RegisterFormatterLocatorAttribute attr2 = (RegisterFormatterLocatorAttribute)attrUncast2;
						if (attr2.FormatterLocatorType.IsClass && !attr2.FormatterLocatorType.IsAbstract && !(attr2.FormatterLocatorType.GetConstructor(Type.EmptyTypes) == null) && typeof(IFormatterLocator).IsAssignableFrom(attr2.FormatterLocatorType))
						{
							try
							{
								FormatterLocators.Add(new FormatterLocatorInfo
								{
									LocatorInstance = (IFormatterLocator)Activator.CreateInstance(attr2.FormatterLocatorType),
									Priority = attr2.Priority
								});
							}
							catch (Exception innerException)
							{
								Debug.LogException(new Exception("Exception was thrown while instantiating FormatterLocator of type " + attr2.FormatterLocatorType.FullName + ".", innerException));
							}
						}
					}
				}
				catch (TypeLoadException)
				{
					if (ass.GetName().Name == "OdinSerializer")
					{
						Debug.LogError("A TypeLoadException occurred when FormatterLocator tried to load types from assembly '" + ass.FullName + "'. No serialization formatters in this assembly will be found. Serialization will be utterly broken.");
					}
				}
				catch (ReflectionTypeLoadException)
				{
					if (ass.GetName().Name == "OdinSerializer")
					{
						Debug.LogError("A ReflectionTypeLoadException occurred when FormatterLocator tried to load types from assembly '" + ass.FullName + "'. No serialization formatters in this assembly will be found. Serialization will be utterly broken.");
					}
				}
				catch (MissingMemberException)
				{
					if (ass.GetName().Name == "OdinSerializer")
					{
						Debug.LogError("A ReflectionTypeLoadException occurred when FormatterLocator tried to load types from assembly '" + ass.FullName + "'. No serialization formatters in this assembly will be found. Serialization will be utterly broken.");
					}
				}
			}
			FormatterInfos.Sort(delegate(FormatterInfo a, FormatterInfo b)
			{
				int num = -a.Priority.CompareTo(b.Priority);
				if (num == 0)
				{
					num = a.FormatterType.Name.CompareTo(b.FormatterType.Name);
				}
				return num;
			});
			FormatterLocators.Sort(delegate(FormatterLocatorInfo a, FormatterLocatorInfo b)
			{
				int num = -a.Priority.CompareTo(b.Priority);
				if (num == 0)
				{
					num = a.LocatorInstance.GetType().Name.CompareTo(b.LocatorInstance.GetType().Name);
				}
				return num;
			});
		}

		/// <summary>
		/// Gets a formatter for the type <see cref="!:T" />.
		/// </summary>
		/// <typeparam name="T">The type to get a formatter for.</typeparam>
		/// <param name="policy">The serialization policy to use if a formatter has to be emitted. If null, <see cref="P:Sirenix.Serialization.SerializationPolicies.Strict" /> is used.</param>
		/// <returns>
		/// A formatter for the type <see cref="!:T" />.
		/// </returns>
		public static IFormatter<T> GetFormatter<T>(ISerializationPolicy policy)
		{
			return (IFormatter<T>)GetFormatter(typeof(T), policy, allowWeakFallbackFormatters: false);
		}

		/// <summary>
		/// Gets a formatter for a given type.
		/// </summary>
		/// <param name="type">The type to get a formatter for.</param>
		/// <param name="policy">The serialization policy to use if a formatter has to be emitted. If null, <see cref="P:Sirenix.Serialization.SerializationPolicies.Strict" /> is used.</param>
		/// <returns>
		/// A formatter for the given type.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The type argument is null.</exception>
		public static IFormatter GetFormatter(Type type, ISerializationPolicy policy)
		{
			return GetFormatter(type, policy, allowWeakFallbackFormatters: true);
		}

		/// <summary>
		/// Gets a formatter for a given type.
		/// </summary>
		/// <param name="type">The type to get a formatter for.</param>
		/// <param name="policy">The serialization policy to use if a formatter has to be emitted. If null, <see cref="P:Sirenix.Serialization.SerializationPolicies.Strict" /> is used.</param>
		/// <param name="allowWeakFallbackFormatters">Whether to allow the use of weak fallback formatters which do not implement the strongly typed <see cref="T:Sirenix.Serialization.IFormatter`1" />, but which conversely do not need to have had AOT support generated.</param>
		/// <returns>
		/// A formatter for the given type.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The type argument is null.</exception>
		public static IFormatter GetFormatter(Type type, ISerializationPolicy policy, bool allowWeakFallbackFormatters)
		{
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			if (policy == null)
			{
				policy = SerializationPolicies.Strict;
			}
			object lockObj = (allowWeakFallbackFormatters ? WeakFormatters_LOCK : StrongFormatters_LOCK);
			DoubleLookupDictionary<Type, ISerializationPolicy, IFormatter> formatterMap = (allowWeakFallbackFormatters ? WeakTypeFormatterMap : StrongTypeFormatterMap);
			IFormatter result;
			lock (lockObj)
			{
				if (!formatterMap.TryGetInnerValue(type, policy, out result))
				{
					try
					{
						result = CreateFormatter(type, policy, allowWeakFallbackFormatters);
					}
					catch (TargetInvocationException ex)
					{
						if (!(ex.GetBaseException() is ExecutionEngineException))
						{
							throw ex;
						}
						LogAOTError(type, ex.GetBaseException() as ExecutionEngineException);
					}
					catch (TypeInitializationException ex2)
					{
						if (!(ex2.GetBaseException() is ExecutionEngineException))
						{
							throw ex2;
						}
						LogAOTError(type, ex2.GetBaseException() as ExecutionEngineException);
					}
					catch (ExecutionEngineException ex3)
					{
						LogAOTError(type, ex3);
					}
					formatterMap.AddInner(type, policy, result);
				}
			}
			if (FormatterLocator.OnLocatedFormatter != null)
			{
				FormatterLocator.OnLocatedFormatter(result);
			}
			if (FormatterLocator.OnLocatedEmittableFormatterForType != null && result.GetType().IsGenericType)
			{
				if (result.GetType().GetGenericTypeDefinition() == typeof(FormatterEmitter.RuntimeEmittedFormatter<>))
				{
					FormatterLocator.OnLocatedEmittableFormatterForType(type);
				}
				else if (result.GetType().GetGenericTypeDefinition() == typeof(ReflectionFormatter<>))
				{
					FormatterLocator.OnLocatedEmittableFormatterForType(type);
				}
			}
			return result;
		}

		private static void LogAOTError(Type type, Exception ex)
		{
			string[] types = new List<string>(GetAllPossibleMissingAOTTypes(type)).ToArray();
			Debug.LogError("Creating a serialization formatter for the type '" + type.GetNiceFullName() + "' failed due to missing AOT support. \n\n Please use Odin's AOT generation feature to generate an AOT dll before building, and MAKE SURE that all of the following types were automatically added to the supported types list after a scan (if they were not, please REPORT AN ISSUE with the details of which exact types the scan is missing and ADD THEM MANUALLY): \n\n" + string.Join("\n", types) + "\n\nIF ALL THE TYPES ARE IN THE SUPPORT LIST AND YOU STILL GET THIS ERROR, PLEASE REPORT AN ISSUE.The exception contained the following message: \n" + ex.Message);
			throw new SerializationAbortException("AOT formatter support was missing for type '" + type.GetNiceFullName() + "'.", ex);
		}

		private static IEnumerable<string> GetAllPossibleMissingAOTTypes(Type type)
		{
			yield return type.GetNiceFullName() + " (name string: '" + TwoWaySerializationBinder.Default.BindToName(type) + "')";
			if (!type.IsGenericType)
			{
				yield break;
			}
			Type[] genericArguments = type.GetGenericArguments();
			foreach (Type arg in genericArguments)
			{
				yield return arg.GetNiceFullName() + " (name string: '" + TwoWaySerializationBinder.Default.BindToName(arg) + "')";
				if (!arg.IsGenericType)
				{
					continue;
				}
				foreach (string allPossibleMissingAOTType in GetAllPossibleMissingAOTTypes(arg))
				{
					yield return allPossibleMissingAOTType;
				}
			}
		}

		internal static List<IFormatter> GetAllCompatiblePredefinedFormatters(Type type, ISerializationPolicy policy)
		{
			if (FormatterUtilities.IsPrimitiveType(type))
			{
				throw new ArgumentException("Cannot create formatters for a primitive type like " + type.Name);
			}
			List<IFormatter> formatters = new List<IFormatter>();
			for (int i = 0; i < FormatterLocators.Count; i++)
			{
				try
				{
					if (FormatterLocators[i].LocatorInstance.TryGetFormatter(type, FormatterLocationStep.BeforeRegisteredFormatters, policy, allowWeakFallbackFormatters: true, out var result))
					{
						formatters.Add(result);
					}
				}
				catch (TargetInvocationException ex)
				{
					throw ex;
				}
				catch (TypeInitializationException ex2)
				{
					throw ex2;
				}
				catch (ExecutionEngineException ex3)
				{
					throw ex3;
				}
				catch (Exception innerException)
				{
					Debug.LogException(new Exception("Exception was thrown while calling FormatterLocator " + FormatterLocators[i].GetType().FullName + ".", innerException));
				}
			}
			for (int j = 0; j < FormatterInfos.Count; j++)
			{
				FormatterInfo info = FormatterInfos[j];
				Type formatterType = null;
				if (type == info.TargetType)
				{
					formatterType = info.FormatterType;
				}
				else if (info.FormatterType.IsGenericType && info.TargetType.IsGenericParameter)
				{
					if (info.FormatterType.TryInferGenericParameters(out var inferredArgs, type))
					{
						formatterType = info.FormatterType.GetGenericTypeDefinition().MakeGenericType(inferredArgs);
					}
				}
				else if (type.IsGenericType && info.FormatterType.IsGenericType && info.TargetType.IsGenericType && type.GetGenericTypeDefinition() == info.TargetType.GetGenericTypeDefinition())
				{
					Type[] args = type.GetGenericArguments();
					if (info.FormatterType.AreGenericConstraintsSatisfiedBy(args))
					{
						formatterType = info.FormatterType.GetGenericTypeDefinition().MakeGenericType(args);
					}
				}
				if (formatterType != null)
				{
					IFormatter instance = GetFormatterInstance(formatterType);
					if (instance != null && (!info.AskIfCanFormatTypes || ((IAskIfCanFormatTypes)instance).CanFormatType(type)))
					{
						formatters.Add(instance);
					}
				}
			}
			for (int k = 0; k < FormatterLocators.Count; k++)
			{
				try
				{
					if (FormatterLocators[k].LocatorInstance.TryGetFormatter(type, FormatterLocationStep.AfterRegisteredFormatters, policy, allowWeakFallbackFormatters: true, out var result2))
					{
						formatters.Add(result2);
					}
				}
				catch (TargetInvocationException ex4)
				{
					throw ex4;
				}
				catch (TypeInitializationException ex5)
				{
					throw ex5;
				}
				catch (ExecutionEngineException ex6)
				{
					throw ex6;
				}
				catch (Exception innerException2)
				{
					Debug.LogException(new Exception("Exception was thrown while calling FormatterLocator " + FormatterLocators[k].GetType().FullName + ".", innerException2));
				}
			}
			formatters.Add((IFormatter)Activator.CreateInstance(typeof(ReflectionFormatter<>).MakeGenericType(type)));
			return formatters;
		}

		private static IFormatter CreateFormatter(Type type, ISerializationPolicy policy, bool allowWeakFormatters)
		{
			if (FormatterUtilities.IsPrimitiveType(type))
			{
				throw new ArgumentException("Cannot create formatters for a primitive type like " + type.Name);
			}
			for (int i = 0; i < FormatterLocators.Count; i++)
			{
				try
				{
					if (FormatterLocators[i].LocatorInstance.TryGetFormatter(type, FormatterLocationStep.BeforeRegisteredFormatters, policy, allowWeakFormatters, out var result))
					{
						return result;
					}
				}
				catch (TargetInvocationException ex)
				{
					throw ex;
				}
				catch (TypeInitializationException ex2)
				{
					throw ex2;
				}
				catch (ExecutionEngineException ex3)
				{
					throw ex3;
				}
				catch (Exception innerException)
				{
					Debug.LogException(new Exception("Exception was thrown while calling FormatterLocator " + FormatterLocators[i].GetType().FullName + ".", innerException));
				}
			}
			for (int j = 0; j < FormatterInfos.Count; j++)
			{
				FormatterInfo info = FormatterInfos[j];
				Type formatterType = null;
				Type weakFallbackType = null;
				Type[] genericFormatterArgs = null;
				if (type == info.TargetType)
				{
					formatterType = info.FormatterType;
				}
				else if (info.FormatterType.IsGenericType && info.TargetType.IsGenericParameter)
				{
					if (info.FormatterType.TryInferGenericParameters(out var inferredArgs, type))
					{
						genericFormatterArgs = inferredArgs;
					}
				}
				else if (type.IsGenericType && info.FormatterType.IsGenericType && info.TargetType.IsGenericType && type.GetGenericTypeDefinition() == info.TargetType.GetGenericTypeDefinition())
				{
					Type[] args = type.GetGenericArguments();
					if (info.FormatterType.AreGenericConstraintsSatisfiedBy(args))
					{
						genericFormatterArgs = args;
					}
				}
				if (formatterType == null && genericFormatterArgs != null)
				{
					formatterType = info.FormatterType.GetGenericTypeDefinition().MakeGenericType(genericFormatterArgs);
					weakFallbackType = info.WeakFallbackType;
				}
				if (!(formatterType != null))
				{
					continue;
				}
				IFormatter instance = null;
				bool aotError = false;
				Exception aotEx = null;
				try
				{
					instance = GetFormatterInstance(formatterType);
				}
				catch (TargetInvocationException ex4)
				{
					aotError = true;
					aotEx = ex4;
				}
				catch (TypeInitializationException ex5)
				{
					aotError = true;
					aotEx = ex5;
				}
				catch (ExecutionEngineException ex6)
				{
					aotError = true;
					aotEx = ex6;
				}
				if (aotError && !EmitUtilities.CanEmit && allowWeakFormatters)
				{
					if (weakFallbackType != null)
					{
						instance = (IFormatter)Activator.CreateInstance(weakFallbackType, type);
					}
					if (instance == null)
					{
						string argsStr = "";
						for (int k = 0; k < genericFormatterArgs.Length; k++)
						{
							if (k > 0)
							{
								argsStr += ", ";
							}
							argsStr += genericFormatterArgs[k].GetNiceFullName();
						}
						Debug.LogError("No AOT support was generated for serialization formatter type '" + info.FormatterType.GetNiceFullName() + "' for the generic arguments <" + argsStr + ">, and no weak fallback formatter was specified.");
						throw aotEx;
					}
				}
				if (instance != null && (!info.AskIfCanFormatTypes || ((IAskIfCanFormatTypes)instance).CanFormatType(type)))
				{
					return instance;
				}
			}
			for (int l = 0; l < FormatterLocators.Count; l++)
			{
				try
				{
					if (FormatterLocators[l].LocatorInstance.TryGetFormatter(type, FormatterLocationStep.AfterRegisteredFormatters, policy, allowWeakFormatters, out var result2))
					{
						return result2;
					}
				}
				catch (TargetInvocationException ex7)
				{
					throw ex7;
				}
				catch (TypeInitializationException ex8)
				{
					throw ex8;
				}
				catch (ExecutionEngineException ex9)
				{
					throw ex9;
				}
				catch (Exception innerException2)
				{
					Debug.LogException(new Exception("Exception was thrown while calling FormatterLocator " + FormatterLocators[l].GetType().FullName + ".", innerException2));
				}
			}
			if (EmitUtilities.CanEmit)
			{
				IFormatter result3 = FormatterEmitter.GetEmittedFormatter(type, policy);
				if (result3 != null)
				{
					return result3;
				}
			}
			if (EmitUtilities.CanEmit)
			{
				Debug.LogWarning("Fallback to reflection for type " + type.Name + " when emit is possible on this platform.");
			}
			try
			{
				return (IFormatter)Activator.CreateInstance(typeof(ReflectionFormatter<>).MakeGenericType(type));
			}
			catch (TargetInvocationException ex10)
			{
				if (allowWeakFormatters)
				{
					return new WeakReflectionFormatter(type);
				}
				throw ex10;
			}
			catch (TypeInitializationException ex11)
			{
				if (allowWeakFormatters)
				{
					return new WeakReflectionFormatter(type);
				}
				throw ex11;
			}
			catch (ExecutionEngineException ex12)
			{
				if (allowWeakFormatters)
				{
					return new WeakReflectionFormatter(type);
				}
				throw ex12;
			}
		}

		private static IFormatter GetFormatterInstance(Type type)
		{
			if (!FormatterInstances.TryGetValue(type, out var formatter))
			{
				try
				{
					formatter = (IFormatter)Activator.CreateInstance(type);
					FormatterInstances.Add(type, formatter);
				}
				catch (TargetInvocationException ex)
				{
					throw ex;
				}
				catch (TypeInitializationException ex2)
				{
					throw ex2;
				}
				catch (ExecutionEngineException ex3)
				{
					throw ex3;
				}
				catch (Exception innerException)
				{
					Debug.LogException(new Exception("Exception was thrown while instantiating formatter '" + type.GetNiceFullName() + "'.", innerException));
				}
			}
			return formatter;
		}
	}
}
