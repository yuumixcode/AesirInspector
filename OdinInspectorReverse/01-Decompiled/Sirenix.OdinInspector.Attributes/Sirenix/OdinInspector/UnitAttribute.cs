using System;

namespace Sirenix.OdinInspector
{
	public class UnitAttribute : Attribute
	{
		/// <summary>
		/// The unit of underlying value.
		/// </summary>
		public Units Base = Units.Unset;

		/// <summary>
		/// The unit displayed in the number field.
		/// </summary>
		public Units Display = Units.Unset;

		/// <summary>
		/// Name of the underlying unit.
		/// </summary>
		public string BaseName;

		/// <summary>
		/// Name of the unit displayed in the number field.
		/// </summary>
		public string DisplayName;

		/// <summary>
		/// If <c>true</c> the number field is drawn as read-only text.
		/// </summary>
		public bool DisplayAsString;

		/// <summary>
		/// If <c>true</c> disables the option to change display unit with the right-click context menu.
		/// </summary>
		public bool ForceDisplayUnit;

		/// <summary>
		/// Displays the number as a unit field.
		/// </summary>
		/// <param name="unit">The unit of underlying value.</param>
		public UnitAttribute(Units unit)
		{
			Base = unit;
			Display = unit;
		}

		/// <summary>
		/// Displays the number as a unit field.
		/// </summary>
		/// <param name="unit">The name of the underlying value.</param>
		public UnitAttribute(string unit)
		{
			BaseName = unit;
			DisplayName = unit;
		}

		/// <summary>
		/// Displays the number as a unit field.
		/// </summary>
		/// <param name="base">The unit of underlying value.</param>
		/// <param name="display">The unit to display the value as in the inspector.</param>
		public UnitAttribute(Units @base, Units display)
		{
			Base = @base;
			Display = display;
		}

		/// <summary>
		/// Displays the number as a unit field.
		/// </summary>
		/// <param name="base">The unit of underlying value.</param>
		/// <param name="display">The unit to display the value as in the inspector.</param>
		public UnitAttribute(Units @base, string display)
		{
			Base = @base;
			DisplayName = display;
		}

		/// <summary>
		/// Displays the number as a unit field.
		/// </summary>
		/// <param name="base">The unit of underlying value.</param>
		/// <param name="display">The unit to display the value as in the inspector.</param>
		public UnitAttribute(string @base, Units display)
		{
			BaseName = @base;
			Display = display;
		}

		/// <summary>
		/// Displays the number as a unit field.
		/// </summary>
		/// <param name="base">The unit of underlying value.</param>
		/// <param name="display">The unit to display the value as in the inspector.</param>
		public UnitAttribute(string @base, string display)
		{
			BaseName = @base;
			DisplayName = display;
		}
	}
}
