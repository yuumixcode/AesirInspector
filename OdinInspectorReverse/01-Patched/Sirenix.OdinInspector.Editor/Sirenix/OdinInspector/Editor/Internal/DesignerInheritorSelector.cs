using System;
using System.Collections.Generic;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class DesignerInheritorSelector : OdinSelector<DesignerInheritorItem>
	{
		[HideInInspector]
		public readonly Type BaseType;

		[HideInInspector]
		public bool IsGenericDefinition;

		public DesignerInheritorSelector(Type baseType)
		{
			BaseType = baseType;
		}

		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			IsGenericDefinition = false;
			if (BaseType.IsGenericType)
			{
				Type definition = BaseType.GetGenericTypeDefinition();
				if (definition == BaseType)
				{
					if (DesignerRegistry.RegisteredGenericVariants.TryGetValue(definition, out var types))
					{
						foreach (Type type in types)
						{
							tree.Add(type.GetNiceName(), new DesignerInheritorItem(type), GUIHelper.GetAssetThumbnail(null, type, preferObjectPreviewOverFileIcon: false));
						}
					}
					IsGenericDefinition = true;
				}
			}
			if (!IsGenericDefinition)
			{
				List<Type> inheritors = TypeRegistry.GetInheritors(BaseType);
				for (int i = 0; i < inheritors.Count; i++)
				{
					Type inheritor = inheritors[i];
					if (!(inheritor.BaseType != BaseType))
					{
						tree.Add(inheritor.Name, new DesignerInheritorItem(inheritor), GUIHelper.GetAssetThumbnail(null, inheritor, preferObjectPreviewOverFileIcon: false));
					}
				}
			}
			tree.Config.EXPERIMENTAL_INTERNAL_SparseFixedLayouting = true;
			tree.Config.DefaultMenuStyle.Height = 24;
			tree.RootMenuItem.ChildMenuItems.Sort(CompareItems);
			EnableSingleClickToSelect();
		}

		[PropertyOrder(1f)]
		[OnInspectorGUI]
		public void DrawGenericVariant()
		{
		}

		internal static int CompareItems(OdinMenuItem lhs, OdinMenuItem rhs)
		{
			DesignerInheritorItem lhsItem = (DesignerInheritorItem)lhs.Value;
			DesignerInheritorItem rhsItem = (DesignerInheritorItem)rhs.Value;
			bool isDesigned = rhsItem.IsDesigned;
			int cmp = isDesigned.CompareTo(lhsItem.IsDesigned);
			if (cmp != 0)
			{
				return cmp;
			}
			return string.Compare(lhs.Name, rhs.Name, StringComparison.Ordinal);
		}
	}
}
