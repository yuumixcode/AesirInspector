using System;
using System.Collections.Generic;
using System.Linq;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(TypeSelectorSettingsAttribute))]
	internal class TypeSelectorSettingsExample
	{
		[ShowInInspector]
		public Type Default;

		[TypeSelectorSettings(ShowCategories = true)]
		[LabelText("On")]
		[ShowInInspector]
		[Title("Show Categories", null, TitleAlignments.Left, true, true)]
		public Type ShowCategories_On;

		[ShowInInspector]
		[LabelText("Off")]
		[TypeSelectorSettings(ShowCategories = false)]
		public Type ShowCategories_Off;

		[LabelText("On")]
		[Title("Prefer Namespaces", null, TitleAlignments.Left, true, true)]
		[ShowInInspector]
		[TypeSelectorSettings(PreferNamespaces = true, ShowCategories = true)]
		public Type PreferNamespaces_On;

		[ShowInInspector]
		[TypeSelectorSettings(PreferNamespaces = false, ShowCategories = true)]
		[LabelText("Off")]
		public Type PreferNamespaces_Off;

		[Title("Show None Item", null, TitleAlignments.Left, true, true)]
		[ShowInInspector]
		[LabelText("On")]
		[TypeSelectorSettings(ShowNoneItem = true)]
		public Type ShowNoneItem_On;

		[TypeSelectorSettings(ShowNoneItem = false)]
		[LabelText("Off")]
		[ShowInInspector]
		public Type ShowNoneItem_Off;

		[Title("Custom Type Filter", null, TitleAlignments.Left, true, true)]
		[ShowInInspector]
		[TypeSelectorSettings(FilterTypesFunction = "TypeFilter", ShowCategories = false)]
		public Type CustomTypeFilterExample;

		private bool TypeFilter(Type type)
		{
			return type.GetInterfaces().Any((Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
		}
	}
}
