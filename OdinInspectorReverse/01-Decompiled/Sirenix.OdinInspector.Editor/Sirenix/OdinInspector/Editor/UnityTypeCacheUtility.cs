using System;
using System.Collections.Generic;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor
{
	[Obsolete("Use UnityEditor.TypeCache instead", false)]
	public static class UnityTypeCacheUtility
	{
		public static readonly bool IsAvailable = true;

		public static IList<Type> GetTypesDerivedFrom(Type type)
		{
			return TypeCache.GetTypesDerivedFrom(type);
		}

		public static IList<Type> GetTypesWithAttribute<T>() where T : Attribute
		{
			return TypeCache.GetTypesWithAttribute<T>();
		}

		public static IList<Type> GetTypesWithAttribute(Type attributeType)
		{
			return TypeCache.GetTypesWithAttribute(attributeType);
		}
	}
}
