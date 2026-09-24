using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class AtomHandlerLocator
	{
		private static readonly Dictionary<Type, Type> AtomHandlerTypes;

		private static readonly Dictionary<Type, IAtomHandler> AtomHandlers;

		static AtomHandlerLocator()
		{
			AtomHandlerTypes = new Dictionary<Type, Type>();
			AtomHandlers = new Dictionary<Type, IAtomHandler>();
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				if (!assembly.SafeIsDefined(typeof(AtomContainerAttribute), inherit: false))
				{
					continue;
				}
				Type[] types = assembly.SafeGetTypes();
				foreach (Type handler in types)
				{
					if (!typeof(IAtomHandler).IsAssignableFrom(handler) || handler.IsAbstract || !handler.IsDefined(typeof(AtomHandlerAttribute), inherit: false) || handler.GetConstructor(Type.EmptyTypes) == null)
					{
						continue;
					}
					Type[] args = handler.GetArgumentsOfInheritedOpenGenericInterface(typeof(IAtomHandler<>));
					if (args != null)
					{
						Type atomicType = args[0];
						if (atomicType.IsAbstract)
						{
							Debug.LogError("The type '" + atomicType.GetNiceName() + "' cannot be marked atomic, as it is abstract.");
						}
						else
						{
							AtomHandlerTypes.Add(atomicType, handler);
						}
					}
				}
			}
		}

		public static bool IsMarkedAtomic(this Type type)
		{
			return AtomHandlerTypes.ContainsKey(type);
		}

		public static IAtomHandler GetAtomHandler(Type type)
		{
			if (!AtomHandlerTypes.ContainsKey(type))
			{
				return null;
			}
			if (!AtomHandlers.TryGetValue(type, out var result))
			{
				result = (IAtomHandler)Activator.CreateInstance(AtomHandlerTypes[type]);
				AtomHandlers[type] = result;
			}
			return result;
		}

		public static IAtomHandler<T> GetAtomHandler<T>()
		{
			return (IAtomHandler<T>)GetAtomHandler(typeof(T));
		}
	}
}
