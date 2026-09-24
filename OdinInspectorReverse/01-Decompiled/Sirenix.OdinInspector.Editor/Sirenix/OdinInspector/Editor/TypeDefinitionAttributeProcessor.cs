using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Find attributes attached to the type definition of a property and adds to them to attribute list.
	/// </summary>
	[OdinCacheableProcessor]
	[ResolverPriority(1000.0)]
	public class TypeDefinitionAttributeProcessor : OdinAttributeProcessor
	{
		private static readonly Dictionary<Type, bool> HadResultCache = new Dictionary<Type, bool>(FastTypeComparer.Instance);

		/// <summary>
		/// This attribute processor can only process for properties.
		/// </summary>
		/// <param name="parentProperty">The parent of the specified member.</param>
		/// <param name="member">The member to process.</param>
		/// <returns><c>false</c>.</returns>
		public override bool CanProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member)
		{
			return false;
		}

		/// <summary>
		/// This attribute processor can only process for properties with an attached value entry.
		/// </summary>
		/// <param name="property">The property to process.</param>
		/// <returns><c>true</c> if the specified property has a value entry. Otherwise <c>false</c>.</returns>
		public override bool CanProcessSelfAttributes(InspectorProperty property)
		{
			IPropertyValueEntry entry = property.ValueEntry;
			if (entry == null)
			{
				return false;
			}
			if (FastTypeComparer.Instance.Equals(entry.TypeOfValue, entry.BaseValueType) && HadResultCache.TryGetValue(entry.BaseValueType, out var result))
			{
				return result;
			}
			return true;
		}

		/// <summary>
		/// Finds all attributes attached to the type and base types of the specified property value and adds them to the attribute list.
		/// </summary>
		/// <param name="property">The property to process.</param>
		/// <param name="attributes">The list of attributes for the property.</param>
		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			Type current = property.ValueEntry.TypeOfValue;
			if (FastTypeComparer.Instance.Equals(current, property.ValueEntry.BaseValueType))
			{
				bool wasInCache = false;
				Type original = current;
				if (HadResultCache.TryGetValue(original, out var hadResult))
				{
					if (!hadResult)
					{
						return;
					}
					wasInCache = true;
				}
				while (!(current == null))
				{
					AssemblyCategory flag = AssemblyUtilities.GetAssemblyCategory(current.Assembly);
					if ((flag & AssemblyCategory.ProjectSpecific) == 0)
					{
						break;
					}
					if (current.IsDefined(typeof(Attribute), inherit: false))
					{
						hadResult = true;
						attributes.AddRange(current.GetAttributes(inherit: false));
					}
					current = current.BaseType;
				}
				if (!wasInCache)
				{
					HadResultCache.Add(original, hadResult);
				}
				return;
			}
			while (!(current == null))
			{
				AssemblyCategory flag2 = AssemblyUtilities.GetAssemblyCategory(current.Assembly);
				if ((flag2 & AssemblyCategory.ProjectSpecific) == 0)
				{
					break;
				}
				if (current.IsDefined(typeof(Attribute), inherit: false))
				{
					attributes.AddRange(current.GetAttributes(inherit: false));
				}
				current = current.BaseType;
			}
			current = property.ValueEntry.BaseValueType;
			if (!current.IsInterface)
			{
				return;
			}
			while (!(current == null))
			{
				AssemblyCategory flag3 = AssemblyUtilities.GetAssemblyCategory(current.Assembly);
				if ((flag3 & AssemblyCategory.ProjectSpecific) != AssemblyCategory.None)
				{
					if (current.IsDefined(typeof(Attribute), inherit: false))
					{
						attributes.AddRange(current.GetAttributes(inherit: false));
					}
					current = current.BaseType;
					continue;
				}
				break;
			}
		}
	}
}
