using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	public static class ResolverUtilities
	{
		public static List<Assembly> GetResolverAssemblies()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			List<Assembly> results = new List<Assembly>(assemblies.Length);
			foreach (Assembly assembly in assemblies)
			{
				if (assembly.SafeIsDefined(typeof(ContainsOdinResolversAttribute), inherit: true) || (AssemblyUtilities.GetAssemblyCategory(assembly) & AssemblyCategory.ProjectSpecific) != AssemblyCategory.None)
				{
					results.Add(assembly);
				}
			}
			return results;
		}

		public static double GetResolverPriority(Type resolverType)
		{
			ResolverPriorityAttribute attr = resolverType.GetAttribute<ResolverPriorityAttribute>(inherit: true);
			if (attr != null)
			{
				return attr.Priority;
			}
			if (resolverType.Assembly == typeof(OdinEditor).Assembly)
			{
				return -0.1;
			}
			return 0.0;
		}
	}
}
