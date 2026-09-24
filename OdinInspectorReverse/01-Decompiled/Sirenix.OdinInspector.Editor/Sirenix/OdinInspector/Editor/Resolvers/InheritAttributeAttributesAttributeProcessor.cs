using System;
using System.Collections.Generic;
using System.Reflection;

namespace Sirenix.OdinInspector.Editor.Resolvers
{
	/// <summary>
	/// This attribute processor will take any attribute already applied to the property with the <see cref="T:Sirenix.OdinInspector.IncludeMyAttributesAttribute" /> applied to,
	/// and take all attributes applied to the attribute (except any <see cref="T:System.AttributeUsageAttribute" />) and add to them to the property.
	/// This allows for adding attributes to attributes in the property system.
	/// </summary>
	[ResolverPriority(-100000.0)]
	[OdinCacheableProcessor]
	public class InheritAttributeAttributesAttributeProcessor : OdinAttributeProcessor
	{
		private static readonly Type TypeOf_OnInspectorInitAttribute = typeof(OnInspectorInitAttribute);

		private static readonly Type TypeOf_PropertyOrderAttribute = typeof(PropertyOrderAttribute);

		/// <summary>
		/// Looks for attributes in the attributes list with a <see cref="T:Sirenix.OdinInspector.IncludeMyAttributesAttribute" /> applied, and adds the attribute from those attributes to the property.
		/// </summary>
		/// <param name="parentProperty">The parent of the member.</param>
		/// <param name="member">The member that is being processed.</param>
		/// <param name="attributes">The list of attributes currently applied to the property.</param>
		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
		{
			bool hasOnInspectorInit = false;
			bool hasPropertyOrder = false;
			for (int i = attributes.Count - 1; i >= 0; i--)
			{
				Type type = attributes[i].GetType();
				if (type.IsDefined(typeof(IncludeMyAttributesAttribute), inherit: false))
				{
					object[] attrs = type.GetCustomAttributes(inherit: false);
					foreach (object attr in attrs)
					{
						if (!(attr is AttributeUsageAttribute))
						{
							attributes.Add(attr as Attribute);
						}
					}
				}
				if (type == TypeOf_OnInspectorInitAttribute)
				{
					hasOnInspectorInit = true;
				}
				else if (type == TypeOf_PropertyOrderAttribute)
				{
					hasPropertyOrder = true;
				}
			}
			if (hasOnInspectorInit && !hasPropertyOrder && member is MethodInfo)
			{
				attributes.Add(new PropertyOrderAttribute(-2.1474836E+09f));
			}
		}
	}
}
