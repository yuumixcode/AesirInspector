using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class DesignerMethodInfoRetriever
	{
		public static Type Target;

		public static readonly Dictionary<Type, Dictionary<string, MethodInfo>> Cache = new Dictionary<Type, Dictionary<string, MethodInfo>>(16);

		public static void Prepare(Type type)
		{
			if (!(Target == type))
			{
				Target = type;
				Cache.Clear();
			}
		}

		public static MethodInfo Get(Type type, string serializedName)
		{
			if (!Cache.TryGetValue(type, out var methods))
			{
				Dictionary<string, MethodInfo> dictionary = (Cache[type] = BuildCache(type));
				methods = dictionary;
			}
			if (methods.TryGetValue(serializedName, out var method))
			{
				return method;
			}
			return null;
		}

		internal static Dictionary<string, MethodInfo> BuildCache(Type type)
		{
			Dictionary<string, MethodInfo> result = new Dictionary<string, MethodInfo>(32);
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo method in methods)
			{
				if (!method.IsSpecialName && !(method.DeclaringType == typeof(object)))
				{
					result[method.GetNiceName()] = method;
				}
			}
			foreach (MethodInfo method2 in methods)
			{
				if (!method2.IsSpecialName && !(method2.DeclaringType == typeof(object)))
				{
					result[DesignerUtils.CreateSerializedMethodName(method2)] = method2;
				}
			}
			return result;
		}
	}
}
