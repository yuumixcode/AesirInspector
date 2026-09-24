using System;

namespace Sirenix.OdinInspector.Editor
{
	public static class TypeInclusionFilterExtensions
	{
		public static bool IsValidType(this TypeInclusionFilter filter, Type type)
		{
			switch (filter)
			{
			case TypeInclusionFilter.None:
				return false;
			case TypeInclusionFilter.IncludeAbstracts:
				if (type.IsAbstract && !type.IsInterface)
				{
					return !type.IsGenericType;
				}
				return false;
			case TypeInclusionFilter.IncludeGenerics:
				if (type.IsGenericType && !type.IsInterface)
				{
					return !type.IsAbstract;
				}
				return false;
			case TypeInclusionFilter.IncludeInterfaces:
				if (type.IsInterface)
				{
					return !type.IsGenericType;
				}
				return false;
			case TypeInclusionFilter.IncludeConcreteTypes:
				if (!type.IsGenericType && !type.IsInterface)
				{
					return !type.IsAbstract;
				}
				return false;
			default:
			{
				bool includeConcreteTypes = (filter & TypeInclusionFilter.IncludeConcreteTypes) != 0;
				bool includeAbstracts = (filter & TypeInclusionFilter.IncludeAbstracts) != 0;
				bool includeInterfaces = (filter & TypeInclusionFilter.IncludeInterfaces) != 0;
				bool includeGenerics = (filter & TypeInclusionFilter.IncludeGenerics) != 0;
				if (!includeAbstracts && type.IsAbstract && !type.IsInterface)
				{
					return false;
				}
				if (!includeInterfaces && type.IsInterface)
				{
					return false;
				}
				if (!includeGenerics && type.IsGenericType)
				{
					return false;
				}
				if (!includeConcreteTypes && !type.IsAbstract && !type.IsInterface)
				{
					return false;
				}
				return true;
			}
			}
		}
	}
}
