using System;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerTypeSelector : OdinSelector<Type>
	{
		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			List<Type> types = TypeRegistry.GetValidTypesInCategory(AssemblyCategory.All);
			for (int i = 0; i < types.Count; i++)
			{
				Type type = types[i];
				if (DesignerUtils.CanTypeBeDesigned(type) && !DesignerRegistry.IsTypeDesignedNoHierarchyCheck(type))
				{
					tree.Add(type.Name, type);
				}
			}
		}
	}
}
