using System;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Object describing units, including name, symbols and how to convert it to other units.
	/// </summary>
	public class UnitInfo
	{
		/// <summary>
		/// Name of the unit.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// Symbols of the unit. First symbol is considered the primary symbol.
		/// </summary>
		public readonly string[] Symbols;

		/// <summary>
		/// The category of the unit. Units can only be converted within the same category.
		/// </summary>
		public readonly string UnitCategory;

		/// <summary>
		/// Multiplier for converting from the base unit.
		/// </summary>
		public readonly decimal Multiplier;

		/// <summary>
		/// Custom method for converting from the base unit.
		/// </summary>
		public readonly Func<decimal, decimal> ConvertFromBase;

		/// <summary>
		/// Custom method for converting to the base unit.
		/// </summary>
		public readonly Func<decimal, decimal> ConvertToBase;

		/// <summary>
		/// Indicates whether the UnitInfo should use the <c>multiplier</c> or the <c>ConvertFromBase</c> and <c>ConvertToBase</c> methods.
		/// </summary>
		public readonly bool UsesCustomConversion;

		internal UnitInfo(string name, string[] symbols, UnitCategory unitCategory, decimal multiplier)
			: this(name, symbols, unitCategory.ToString(), multiplier)
		{
		}

		internal UnitInfo(string name, string[] symbols, string unitCategory, decimal multiplier)
		{
			Name = name;
			Symbols = symbols;
			UnitCategory = unitCategory;
			Multiplier = multiplier;
			UsesCustomConversion = false;
			ConvertToBase = null;
			ConvertFromBase = null;
		}

		internal UnitInfo(string name, string[] symbols, UnitCategory unitCategory, Func<decimal, decimal> convertToBase, Func<decimal, decimal> convertFromBase)
			: this(name, symbols, unitCategory.ToString(), convertToBase, convertFromBase)
		{
		}

		internal UnitInfo(string name, string[] symbols, string unitCategory, Func<decimal, decimal> convertToBase, Func<decimal, decimal> convertFromBase)
		{
			if (convertToBase == null)
			{
				throw new ArgumentNullException("convertToBase");
			}
			if (convertFromBase == null)
			{
				throw new ArgumentNullException("convertFromBase");
			}
			Name = name;
			Symbols = symbols;
			UnitCategory = unitCategory;
			Multiplier = default(decimal);
			UsesCustomConversion = true;
			ConvertToBase = convertToBase;
			ConvertFromBase = convertFromBase;
		}
	}
}
