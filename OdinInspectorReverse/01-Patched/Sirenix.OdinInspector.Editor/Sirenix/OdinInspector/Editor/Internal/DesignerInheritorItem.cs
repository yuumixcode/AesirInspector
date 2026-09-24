using System;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerInheritorItem
	{
		public readonly Type Type;

		public readonly bool IsDesigned;

		public DesignerInheritorItem(Type type)
		{
			Type = type;
			IsDesigned = DesignerRegistry.IsTypeDesignedNoHierarchyCheck(type);
		}
	}
}
