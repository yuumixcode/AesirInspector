using System;

namespace Sirenix.OdinInspector.Editor.TypeSearch
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DisableNonGenericPolymorphicTypeMatchingAttribute : Attribute
	{
	}
}
