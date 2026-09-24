using System;
using System.Collections.Generic;
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Internal
{
	/// <summary>
	/// Handles instantiating different versions of the Type Selector depending on the context.
	/// </summary>
	/// <remarks>This handler only handles shared constructors between the two versions, for obsolete or unique constructors use the desired selector.</remarks>
	public static class TypeSelectorHandler_WILL_BE_DEPRECATED
	{
		public static OdinSelector<Type> InstantiateSelector(AssemblyCategory category, bool supportsMultiSelect = false, Type selectedType = null, bool? showCategories = null, bool showHidden = false, bool? preferNamespaces = null, bool? showNoneItem = null, InspectorProperty property = null)
		{
			return InstantiateSelector(GlobalConfig<GeneralDrawerConfig>.Instance.useOldTypeSelector, category, supportsMultiSelect, selectedType, showCategories, showHidden, preferNamespaces, showNoneItem, property);
		}

		public static OdinSelector<Type> InstantiateSelector(IEnumerable<Type> types, bool supportsMultiSelect = false, Type selectedType = null, bool? showCategories = null, bool showHidden = false, bool? preferNamespaces = null, bool? showNoneItem = null, InspectorProperty property = null)
		{
			return InstantiateSelector(GlobalConfig<GeneralDrawerConfig>.Instance.useOldTypeSelector, types, supportsMultiSelect, selectedType, showCategories, showHidden, preferNamespaces, showNoneItem, property);
		}

		internal static OdinSelector<Type> InstantiateSelector(bool useOldSelector, AssemblyCategory category, bool supportsMultiSelect = false, Type selectedType = null, bool? showCategories = null, bool showHidden = false, bool? preferNamespaces = null, bool? showNoneItem = null, InspectorProperty property = null)
		{
			return useOldSelector ? ((OdinSelector<Type>)new TypeSelector(category, supportsMultiSelect)) : ((OdinSelector<Type>)new TypeSelectorV2(category, supportsMultiSelect, selectedType, showCategories, showHidden, preferNamespaces, showNoneItem, property));
		}

		internal static OdinSelector<Type> InstantiateSelector(bool useOldSelector, IEnumerable<Type> types, bool supportsMultiSelect = false, Type selectedType = null, bool? showCategories = null, bool showHidden = false, bool? preferNamespaces = null, bool? showNoneItem = null, InspectorProperty property = null)
		{
			return useOldSelector ? ((OdinSelector<Type>)new TypeSelector(types, supportsMultiSelect)) : ((OdinSelector<Type>)new TypeSelectorV2(types, supportsMultiSelect, selectedType, showCategories, showHidden, preferNamespaces, showNoneItem, property));
		}
	}
}
