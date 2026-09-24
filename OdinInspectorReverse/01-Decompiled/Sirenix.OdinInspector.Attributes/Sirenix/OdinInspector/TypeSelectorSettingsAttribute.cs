using System;

namespace Sirenix.OdinInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class TypeSelectorSettingsAttribute : Attribute
	{
		public const string FILTER_TYPES_FUNCTION_NAMED_VALUE = "type";

		/// <summary>
		/// Function for filtering types displayed in the Type Selector.
		/// </summary>
		/// <example>
		/// <para>
		/// The resolver expects any method that takes a single parameter of <see cref="T:System.Type" />, with the parameter name 'type', and which returns a <see cref="T:System.Boolean" /> indicating whether the <see cref="T:System.Type" /> is included or not;
		/// </para>
		///
		/// <para>Implementation example: <c>public bool SomeFilterMethod(Type type)</c>.</para>
		/// </example>
		public string FilterTypesFunction;

		private bool? showNoneItem;

		private bool? showCategories;

		private bool? preferNamespaces;

		/// <summary> Specifies if the '&lt;none&gt;' item is shown. </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "showNoneItem" })]
		public bool ShowNoneItem
		{
			get
			{
				return showNoneItem == true;
			}
			set
			{
				showNoneItem = value;
			}
		}

		/// <summary> Specifies if categories are shown. </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "showCategories" })]
		public bool ShowCategories
		{
			get
			{
				return showCategories == true;
			}
			set
			{
				showCategories = value;
			}
		}

		/// <summary>
		/// Specifies if namespaces are preferred over assembly category names for category names.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "preferNamespaces" })]
		public bool PreferNamespaces
		{
			get
			{
				return preferNamespaces == true;
			}
			set
			{
				preferNamespaces = value;
			}
		}

		public bool ShowNoneItemIsSet => showNoneItem.HasValue;

		public bool ShowCategoriesIsSet => showCategories.HasValue;

		public bool PreferNamespacesIsSet => preferNamespaces.HasValue;
	}
}
