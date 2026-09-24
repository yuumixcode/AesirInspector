using System;

namespace Sirenix.OdinInspector
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class PolymorphicDrawerSettingsAttribute : Attribute
	{
		/// <summary>
		/// Indicates if the drawer should be read-only once a value is assigned.
		/// </summary>
		[LabelWidth(190f)]
		public bool ReadOnlyIfNotNullReference;

		/// <summary>
		/// Specifies a custom function for creating an instance of the selected <see cref="T:System.Type" />.
		/// </summary>
		/// <remarks>Does not get called for <see cref="T:UnityEngine.Object">UnityEngine.Object</see> types.</remarks>
		/// <example>
		/// <para>
		/// The resolver expects any method that takes a single parameter of <see cref="T:System.Type" />, where the parameter is named 'type', and which returns an <see cref="T:System.Object" />.
		/// </para>
		///
		/// <para>Implementation example: <c>public object Method(Type type)</c>.</para>
		/// </example>
		public string CreateInstanceFunction;

		[Obsolete("Use OnValueChangedAttribute instead.", false)]
		public string OnInstanceAssigned;

		private bool? showBaseType;

		private NonDefaultConstructorPreference? nonDefaultConstructorPreference;

		/// <summary>
		/// Determines whether the base type should be displayed in the drawer.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "showBaseType" })]
		public bool ShowBaseType
		{
			get
			{
				return showBaseType == true;
			}
			set
			{
				showBaseType = value;
			}
		}

		/// <summary>
		/// Specifies how non-default constructors are handled.
		/// </summary>
		[ShowInInspector]
		[OdinDesignerBinding(new string[] { "nonDefaultConstructorPreference" })]
		[LabelWidth(210f)]
		public NonDefaultConstructorPreference NonDefaultConstructorPreference
		{
			get
			{
				return nonDefaultConstructorPreference ?? NonDefaultConstructorPreference.ConstructIdeal;
			}
			set
			{
				nonDefaultConstructorPreference = value;
			}
		}

		public bool ShowBaseTypeIsSet => showBaseType.HasValue;

		public bool NonDefaultConstructorPreferenceIsSet => nonDefaultConstructorPreference.HasValue;
	}
}
