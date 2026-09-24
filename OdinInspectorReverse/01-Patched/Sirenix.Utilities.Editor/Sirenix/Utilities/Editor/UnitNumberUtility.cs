using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

namespace Sirenix.Utilities.Editor
{
	/// <summary>
	/// Tools for converting between units, for example, converting from inches to meters.
	/// </summary>
	/// <seealso cref="T:Sirenix.Utilities.Editor.UnitInfo" />
	/// <seealso cref="T:Sirenix.OdinInspector.UnitAttribute" />
	public static class UnitNumberUtility
	{
		private static readonly UnitInfo[] enumUnitInfoMap;

		private static readonly List<UnitInfo> customUnits;

		static UnitNumberUtility()
		{
			customUnits = new List<UnitInfo>();
			int enumUnitInfoMapLength = 0;
			Array enumValues = Enum.GetValues(typeof(Units));
			foreach (object e in enumValues)
			{
				enumUnitInfoMapLength = Math.Max((int)e, enumUnitInfoMapLength);
			}
			enumUnitInfoMap = new UnitInfo[enumUnitInfoMapLength + 1];
			enumUnitInfoMap[0] = new UnitInfo("Nanometer", new string[1] { "nm" }, UnitCategory.Distance, 1000000000.0m);
			enumUnitInfoMap[1] = new UnitInfo("Micrometer", new string[2] { "µm", "um" }, UnitCategory.Distance, 1000000.0m);
			enumUnitInfoMap[2] = new UnitInfo("Millimeter", new string[1] { "mm" }, UnitCategory.Distance, 1000.0m);
			enumUnitInfoMap[3] = new UnitInfo("Centimeter", new string[1] { "cm" }, UnitCategory.Distance, 100.0m);
			enumUnitInfoMap[4] = new UnitInfo("Meter", new string[1] { "m" }, UnitCategory.Distance, 1.0m);
			enumUnitInfoMap[5] = new UnitInfo("Kilometer", new string[1] { "km" }, UnitCategory.Distance, 0.001m);
			enumUnitInfoMap[6] = new UnitInfo("Inch", new string[2] { "\"", "in" }, UnitCategory.Distance, 39.37007874m);
			enumUnitInfoMap[7] = new UnitInfo("Feet", new string[2] { "'", "ft" }, UnitCategory.Distance, 3.280839895m);
			enumUnitInfoMap[8] = new UnitInfo("Mile", new string[1] { "mi" }, UnitCategory.Distance, 0.0006213712m);
			enumUnitInfoMap[9] = new UnitInfo("Yard", new string[1] { "yd" }, UnitCategory.Distance, 1.0936132983m);
			enumUnitInfoMap[10] = new UnitInfo("Nautical Mile", new string[1] { "nmi" }, UnitCategory.Distance, 0.0005399568m);
			enumUnitInfoMap[11] = new UnitInfo("Light Year", new string[1] { "ly" }, UnitCategory.Distance, 0.0000000000000001057000834m);
			enumUnitInfoMap[12] = new UnitInfo("Parsec", new string[1] { "pc" }, UnitCategory.Distance, 0.00000000000000003240779289m);
			enumUnitInfoMap[13] = new UnitInfo("Astronomical Unit", new string[1] { "AU" }, UnitCategory.Distance, 0.000000000006684587122m);
			enumUnitInfoMap[14] = new UnitInfo("Cubic Meter", new string[1] { "m³" }, UnitCategory.Volume, 1.0m);
			enumUnitInfoMap[15] = new UnitInfo("Cubic Kilometer", new string[1] { "km³" }, UnitCategory.Volume, 0.000000001m);
			enumUnitInfoMap[16] = new UnitInfo("Cubic Centimeter", new string[1] { "cm³" }, UnitCategory.Volume, 1000000m);
			enumUnitInfoMap[17] = new UnitInfo("Cubic Millimeter", new string[1] { "mm³" }, UnitCategory.Volume, 1000000000m);
			enumUnitInfoMap[18] = new UnitInfo("Liter", new string[1] { "L" }, UnitCategory.Volume, 1000m);
			enumUnitInfoMap[19] = new UnitInfo("Milliliter", new string[1] { "ml" }, UnitCategory.Volume, 1000000m);
			enumUnitInfoMap[20] = new UnitInfo("Centiliter", new string[1] { "cl" }, UnitCategory.Volume, 100000.0m);
			enumUnitInfoMap[21] = new UnitInfo("Deciliter", new string[1] { "dl" }, UnitCategory.Volume, 10000.0m);
			enumUnitInfoMap[22] = new UnitInfo("Hectoliter", new string[1] { "hl" }, UnitCategory.Volume, 10.0m);
			enumUnitInfoMap[23] = new UnitInfo("Cubic Inch", new string[1] { "in³" }, UnitCategory.Volume, 61023.744095m);
			enumUnitInfoMap[24] = new UnitInfo("Cubic Feet", new string[1] { "ft³" }, UnitCategory.Volume, 35.314666721m);
			enumUnitInfoMap[25] = new UnitInfo("Cubic Yard", new string[1] { "yd³" }, UnitCategory.Volume, 1.3079506193m);
			enumUnitInfoMap[26] = new UnitInfo("Acre Feet", new string[1] { "acre ft" }, UnitCategory.Volume, 0.0008107132m);
			enumUnitInfoMap[27] = new UnitInfo("Barrel Oil", new string[1] { "bbl (oil)" }, UnitCategory.Volume, 6.2898107704m);
			enumUnitInfoMap[35] = new UnitInfo("Barrel (US)", new string[1] { "bbl (US)" }, UnitCategory.Volume, 8.3864143606m);
			enumUnitInfoMap[36] = new UnitInfo("Fluid Ounce (US)", new string[1] { "fl oz (US)" }, UnitCategory.Volume, 33814.022702m);
			enumUnitInfoMap[28] = new UnitInfo("Teaspoon (US)", new string[1] { "tsp (US)" }, UnitCategory.Volume, 202884.13621m);
			enumUnitInfoMap[29] = new UnitInfo("Tablespoon (US)", new string[1] { "tbsp (US)" }, UnitCategory.Volume, 67628.045404m);
			enumUnitInfoMap[30] = new UnitInfo("Cup (US)", new string[1] { "cup (US)" }, UnitCategory.Volume, 4226.7528377m);
			enumUnitInfoMap[31] = new UnitInfo("Gill (US)", new string[1] { "gill (US)" }, UnitCategory.Volume, 8453.5056755m);
			enumUnitInfoMap[32] = new UnitInfo("Pint (US)", new string[1] { "pt (US)" }, UnitCategory.Volume, 2113.3764189m);
			enumUnitInfoMap[33] = new UnitInfo("Quart (US)", new string[1] { "qt (US)" }, UnitCategory.Volume, 1056.6882094m);
			enumUnitInfoMap[34] = new UnitInfo("Gallon (US)", new string[1] { "gal (US)" }, UnitCategory.Volume, 264.17205236m);
			enumUnitInfoMap[37] = new UnitInfo("Barrel (UK)", new string[1] { "bbl (UK)" }, UnitCategory.Volume, 6.1102568972m);
			enumUnitInfoMap[38] = new UnitInfo("Fluid Ounce (UK)", new string[1] { "fl oz (UK)" }, UnitCategory.Volume, 35195.079728m);
			enumUnitInfoMap[39] = new UnitInfo("Teaspoon (UK)", new string[1] { "tsp (UK)" }, UnitCategory.Volume, 168936.38269m);
			enumUnitInfoMap[40] = new UnitInfo("Tablespoon (UK)", new string[1] { "tbsp (UK)" }, UnitCategory.Volume, 56312.127565m);
			enumUnitInfoMap[41] = new UnitInfo("Cup (UK)", new string[1] { "cup (UK)" }, UnitCategory.Volume, 3519.5079728m);
			enumUnitInfoMap[42] = new UnitInfo("Gill (UK)", new string[1] { "gill (UK)" }, UnitCategory.Volume, 7039.0159456m);
			enumUnitInfoMap[43] = new UnitInfo("Pint (UK)", new string[1] { "pt (UK)" }, UnitCategory.Volume, 1759.7539864m);
			enumUnitInfoMap[44] = new UnitInfo("Quart (UK)", new string[1] { "qt (UK)" }, UnitCategory.Volume, 879.8769932m);
			enumUnitInfoMap[45] = new UnitInfo("Gallon (UK)", new string[1] { "gal (UK)" }, UnitCategory.Volume, 219.9692483m);
			enumUnitInfoMap[46] = new UnitInfo("Square Meter", new string[1] { "m²" }, UnitCategory.Area, 1.0m);
			enumUnitInfoMap[55] = new UnitInfo("Hectare", new string[1] { "ha" }, UnitCategory.Area, 0.0001m);
			enumUnitInfoMap[47] = new UnitInfo("Square Kilometer", new string[1] { "km²" }, UnitCategory.Area, 0.000001m);
			enumUnitInfoMap[48] = new UnitInfo("Square Centimeter", new string[1] { "cm²" }, UnitCategory.Area, 10000m);
			enumUnitInfoMap[49] = new UnitInfo("Square Millimeter", new string[1] { "mm²" }, UnitCategory.Area, 1000000m);
			enumUnitInfoMap[50] = new UnitInfo("Square Micrometer", new string[2] { "µm²", "um²" }, UnitCategory.Area, 1000000000000m);
			enumUnitInfoMap[51] = new UnitInfo("Square Inch", new string[1] { "in²" }, UnitCategory.Area, 1550.0031m);
			enumUnitInfoMap[52] = new UnitInfo("Square Feet", new string[1] { "ft²" }, UnitCategory.Area, 10.763910417m);
			enumUnitInfoMap[53] = new UnitInfo("Square Yard", new string[1] { "yd²" }, UnitCategory.Area, 1.1959900463m);
			enumUnitInfoMap[54] = new UnitInfo("Square Mile", new string[1] { "mi²" }, UnitCategory.Area, 0.0000003861021585m);
			enumUnitInfoMap[56] = new UnitInfo("Acre", new string[1] { "ac" }, UnitCategory.Area, 0.0002471054m);
			enumUnitInfoMap[57] = new UnitInfo("Are", new string[1] { "a" }, UnitCategory.Area, 0.01m);
			enumUnitInfoMap[58] = new UnitInfo("Joule", new string[1] { "J" }, UnitCategory.Energy, 1m);
			enumUnitInfoMap[59] = new UnitInfo("Kilojoule", new string[1] { "kJ" }, UnitCategory.Energy, 0.001m);
			enumUnitInfoMap[60] = new UnitInfo("Watt-hour", new string[1] { "W*h" }, UnitCategory.Energy, 0.0002777778m);
			enumUnitInfoMap[61] = new UnitInfo("Kilowatt-Hour", new string[1] { "kW*h" }, UnitCategory.Energy, 0.0000002777777777m);
			enumUnitInfoMap[62] = new UnitInfo("Horsepower-Hour", new string[1] { "hp*h" }, UnitCategory.Energy, 0.0000003725061361m);
			enumUnitInfoMap[63] = new UnitInfo("Newton", new string[1] { "N" }, UnitCategory.Force, 1.0m);
			enumUnitInfoMap[64] = new UnitInfo("Kilonewton", new string[1] { "kN" }, UnitCategory.Force, 0.001m);
			enumUnitInfoMap[65] = new UnitInfo("Meganewton", new string[1] { "MN" }, UnitCategory.Force, 0.000001m);
			enumUnitInfoMap[66] = new UnitInfo("Giganewton", new string[1] { "GN" }, UnitCategory.Force, 0.000000001m);
			enumUnitInfoMap[67] = new UnitInfo("Teranewton", new string[1] { "TN" }, UnitCategory.Force, 0.000000000001m);
			enumUnitInfoMap[68] = new UnitInfo("Centinewton", new string[1] { "cN" }, UnitCategory.Force, 100m);
			enumUnitInfoMap[69] = new UnitInfo("Millinewton", new string[1] { "mN" }, UnitCategory.Force, 1000m);
			enumUnitInfoMap[70] = new UnitInfo("Joule/Meter", new string[1] { "J/m" }, UnitCategory.Force, 1m);
			enumUnitInfoMap[71] = new UnitInfo("Joule/Centimeter", new string[1] { "J/cm" }, UnitCategory.Force, 100m);
			enumUnitInfoMap[72] = new UnitInfo("Gram-Force", new string[1] { "gf" }, UnitCategory.Force, 101.9716213m);
			enumUnitInfoMap[73] = new UnitInfo("Kilogram-Force", new string[1] { "kgf" }, UnitCategory.Force, 0.1019716213m);
			enumUnitInfoMap[74] = new UnitInfo("Ton-Force", new string[1] { "tf" }, UnitCategory.Force, 0.0001019716m);
			enumUnitInfoMap[75] = new UnitInfo("Pound-Force", new string[1] { "lbf" }, UnitCategory.Force, 0.2248089431m);
			enumUnitInfoMap[76] = new UnitInfo("Kilopound-Force", new string[1] { "klbf" }, UnitCategory.Force, 0.0002248089m);
			enumUnitInfoMap[77] = new UnitInfo("Ounce-Force", new string[1] { "ozf" }, UnitCategory.Force, 3.5969430896m);
			enumUnitInfoMap[78] = new UnitInfo("Meters per Second", new string[1] { "m/s" }, UnitCategory.Speed, 1.0m);
			enumUnitInfoMap[79] = new UnitInfo("Meters per Minute", new string[1] { "m/min" }, UnitCategory.Speed, 60m);
			enumUnitInfoMap[80] = new UnitInfo("Meters per Hour", new string[1] { "m/h" }, UnitCategory.Speed, 3600m);
			enumUnitInfoMap[81] = new UnitInfo("Kilometers per Second", new string[1] { "km/s" }, UnitCategory.Speed, 0.001m);
			enumUnitInfoMap[82] = new UnitInfo("Kilometers per Minute", new string[1] { "km/min" }, UnitCategory.Speed, 0.06m);
			enumUnitInfoMap[83] = new UnitInfo("Kilometers per Hour", new string[1] { "km/h" }, UnitCategory.Speed, 3.6m);
			enumUnitInfoMap[84] = new UnitInfo("Centimeters per Second", new string[1] { "cm/s" }, UnitCategory.Speed, 100m);
			enumUnitInfoMap[85] = new UnitInfo("Centimeters per Minute", new string[1] { "cm/min" }, UnitCategory.Speed, 6000m);
			enumUnitInfoMap[86] = new UnitInfo("Centimeters per Hour", new string[1] { "cm/h" }, UnitCategory.Speed, 360000m);
			enumUnitInfoMap[87] = new UnitInfo("Millimeters per Second", new string[1] { "mm/s" }, UnitCategory.Speed, 1000m);
			enumUnitInfoMap[88] = new UnitInfo("Millimeters per Minute", new string[1] { "mm/min" }, UnitCategory.Speed, 60000m);
			enumUnitInfoMap[89] = new UnitInfo("Millimeters per Hour", new string[1] { "mm/h" }, UnitCategory.Speed, 3600000m);
			enumUnitInfoMap[90] = new UnitInfo("Feet per Second", new string[2] { "ft/s", "\"/s" }, UnitCategory.Speed, 3.280839895m);
			enumUnitInfoMap[91] = new UnitInfo("Feet per Minute", new string[2] { "ft/min", "\"/min" }, UnitCategory.Speed, 196.8503937m);
			enumUnitInfoMap[92] = new UnitInfo("Feet per Hour", new string[2] { "ft/h", "\"/h" }, UnitCategory.Speed, 11811.023622m);
			enumUnitInfoMap[93] = new UnitInfo("Yards per Second", new string[1] { "yd/s" }, UnitCategory.Speed, 1.0936132983m);
			enumUnitInfoMap[94] = new UnitInfo("Yards per Minute", new string[1] { "yd/min" }, UnitCategory.Speed, 65.616797m);
			enumUnitInfoMap[95] = new UnitInfo("Yards per Hour", new string[1] { "yd/h" }, UnitCategory.Speed, 3937.007874m);
			enumUnitInfoMap[96] = new UnitInfo("Miles per Second", new string[1] { "mi/s" }, UnitCategory.Speed, 0.0006213712m);
			enumUnitInfoMap[97] = new UnitInfo("Miles per Minute", new string[1] { "mi/min" }, UnitCategory.Speed, 0.0372822715m);
			enumUnitInfoMap[98] = new UnitInfo("Miles per Hour", new string[1] { "mi/h" }, UnitCategory.Speed, 2.2369362921m);
			enumUnitInfoMap[99] = new UnitInfo("Knots", new string[1] { "kn" }, UnitCategory.Speed, 1.9438444924m);
			enumUnitInfoMap[100] = new UnitInfo("Knots (UK)", new string[1] { "kt (UK)" }, UnitCategory.Speed, 1.9426025694m);
			enumUnitInfoMap[101] = new UnitInfo("Speed of light (Vacuum)", new string[1] { "c" }, UnitCategory.Speed, 0.000000003335640951m);
			enumUnitInfoMap[102] = new UnitInfo("Bit", new string[1] { "bit" }, UnitCategory.DataStorage, 8000000m);
			enumUnitInfoMap[103] = new UnitInfo("Kilobit", new string[1] { "kbit" }, UnitCategory.DataStorage, 8000m);
			enumUnitInfoMap[104] = new UnitInfo("Megabit", new string[1] { "Mbit" }, UnitCategory.DataStorage, 8m);
			enumUnitInfoMap[105] = new UnitInfo("Gigabit", new string[1] { "Gbit" }, UnitCategory.DataStorage, 0.008m);
			enumUnitInfoMap[106] = new UnitInfo("Terabit", new string[1] { "Tbit" }, UnitCategory.DataStorage, 0.000008m);
			enumUnitInfoMap[107] = new UnitInfo("Petabit", new string[1] { "Pbit" }, UnitCategory.DataStorage, 0.000000008m);
			enumUnitInfoMap[108] = new UnitInfo("Byte", new string[1] { "B" }, UnitCategory.DataStorage, 1000000m);
			enumUnitInfoMap[109] = new UnitInfo("Kilobyte", new string[1] { "kB" }, UnitCategory.DataStorage, 1000m);
			enumUnitInfoMap[111] = new UnitInfo("Megabyte", new string[1] { "MB" }, UnitCategory.DataStorage, 1m);
			enumUnitInfoMap[113] = new UnitInfo("Gigabyte", new string[1] { "GB" }, UnitCategory.DataStorage, 0.001m);
			enumUnitInfoMap[115] = new UnitInfo("Terabyte", new string[1] { "TB" }, UnitCategory.DataStorage, 0.000001m);
			enumUnitInfoMap[117] = new UnitInfo("Petabyte", new string[1] { "PB" }, UnitCategory.DataStorage, 0.000000001m);
			enumUnitInfoMap[110] = new UnitInfo("Kibibyte", new string[1] { "kiB" }, UnitCategory.DataStorage, 976.5625m);
			enumUnitInfoMap[112] = new UnitInfo("Mebibyte", new string[1] { "MiB" }, UnitCategory.DataStorage, 0.9536743164m);
			enumUnitInfoMap[114] = new UnitInfo("Gibibyte", new string[1] { "GiB" }, UnitCategory.DataStorage, 0.0009313226m);
			enumUnitInfoMap[116] = new UnitInfo("Tebibyte", new string[1] { "TiB" }, UnitCategory.DataStorage, 0.0000009094947017m);
			enumUnitInfoMap[118] = new UnitInfo("Pebibyte", new string[1] { "PiB" }, UnitCategory.DataStorage, 0.0000000008881784197m);
			enumUnitInfoMap[119] = new UnitInfo("Kilogram", new string[1] { "kg" }, UnitCategory.Weight, 1.0m);
			enumUnitInfoMap[120] = new UnitInfo("Hectogram", new string[1] { "hg" }, UnitCategory.Weight, 10m);
			enumUnitInfoMap[121] = new UnitInfo("Dekagram", new string[1] { "dag" }, UnitCategory.Weight, 100m);
			enumUnitInfoMap[122] = new UnitInfo("Gram", new string[1] { "g" }, UnitCategory.Weight, 1000m);
			enumUnitInfoMap[123] = new UnitInfo("Decigram", new string[1] { "dg" }, UnitCategory.Weight, 10000m);
			enumUnitInfoMap[124] = new UnitInfo("Centigram", new string[1] { "cg" }, UnitCategory.Weight, 100000m);
			enumUnitInfoMap[125] = new UnitInfo("Milligram", new string[1] { "mg" }, UnitCategory.Weight, 1000000m);
			enumUnitInfoMap[126] = new UnitInfo("Metric Ton", new string[2] { "t", "Mg" }, UnitCategory.Weight, 0.001m);
			enumUnitInfoMap[127] = new UnitInfo("Pounds", new string[1] { "lbs" }, UnitCategory.Weight, 2.20462m);
			enumUnitInfoMap[128] = new UnitInfo("Short Ton", new string[2] { "sh.tn.", "sh.t." }, UnitCategory.Weight, 0.00110231m);
			enumUnitInfoMap[129] = new UnitInfo("Long Ton", new string[2] { "l.tn.", "l.t." }, UnitCategory.Weight, 0.000984207m);
			enumUnitInfoMap[130] = new UnitInfo("Ounce", new string[1] { "oz" }, UnitCategory.Weight, 35.27396195m);
			enumUnitInfoMap[131] = new UnitInfo("Stone (US)", new string[1] { "stone (US)" }, UnitCategory.Weight, 0.1763698097m);
			enumUnitInfoMap[132] = new UnitInfo("Stone (UK)", new string[1] { "stone (UK)" }, UnitCategory.Weight, 0.1574730444m);
			enumUnitInfoMap[133] = new UnitInfo("Quarter (US)", new string[1] { "qr (US)" }, UnitCategory.Weight, 0.0881849049m);
			enumUnitInfoMap[134] = new UnitInfo("Quarter (UK)", new string[1] { "qr (UK)" }, UnitCategory.Weight, 0.0787365222m);
			enumUnitInfoMap[135] = new UnitInfo("Slug", new string[1] { "slug" }, UnitCategory.Weight, 0.0685217659m);
			enumUnitInfoMap[136] = new UnitInfo("Grain", new string[1] { "gr" }, UnitCategory.Weight, 15432.358353m);
			enumUnitInfoMap[139] = new UnitInfo("Kelvin", new string[2] { "°K", "K" }, UnitCategory.Temperature, 1m);
			enumUnitInfoMap[138] = new UnitInfo("Fahrenheit", new string[2] { "°F", "F" }, UnitCategory.Temperature, (decimal f) => (f - 32m) * 5m / 9m + 273.15m, (decimal k) => (k - 273.15m) * 9m / 5m + 32m);
			enumUnitInfoMap[137] = new UnitInfo("Celsius", new string[2] { "°C", "C" }, UnitCategory.Temperature, (decimal c) => c + 273.15m, (decimal k) => k - 273.15m);
			enumUnitInfoMap[140] = new UnitInfo("Pascal", new string[1] { "Pa" }, UnitCategory.Pressure, 1m);
			enumUnitInfoMap[141] = new UnitInfo("Decipascal", new string[1] { "dPa" }, UnitCategory.Pressure, 10m);
			enumUnitInfoMap[142] = new UnitInfo("Centipascal", new string[1] { "cPa" }, UnitCategory.Pressure, 100m);
			enumUnitInfoMap[143] = new UnitInfo("Millipascal", new string[1] { "mPa" }, UnitCategory.Pressure, 1000m);
			enumUnitInfoMap[144] = new UnitInfo("Micropascal", new string[2] { "µPa", "uPa" }, UnitCategory.Pressure, 1000000m);
			enumUnitInfoMap[145] = new UnitInfo("Kilopascal", new string[1] { "kPa" }, UnitCategory.Pressure, 0.001m);
			enumUnitInfoMap[146] = new UnitInfo("Megapascal", new string[1] { "MPa" }, UnitCategory.Pressure, 0.000001m);
			enumUnitInfoMap[147] = new UnitInfo("Gigapascal", new string[1] { "GPa" }, UnitCategory.Pressure, 0.000000001m);
			enumUnitInfoMap[148] = new UnitInfo("Bar", new string[1] { "bar" }, UnitCategory.Pressure, 0.00001m);
			enumUnitInfoMap[149] = new UnitInfo("Millibar", new string[1] { "mbar" }, UnitCategory.Pressure, 0.01m);
			enumUnitInfoMap[150] = new UnitInfo("Microbar", new string[2] { "µbar", "ubar" }, UnitCategory.Pressure, 10m);
			enumUnitInfoMap[151] = new UnitInfo("PSI", new string[1] { "psi" }, UnitCategory.Pressure, 0.0001450377m);
			enumUnitInfoMap[152] = new UnitInfo("KSI", new string[1] { "ksi" }, UnitCategory.Pressure, 0.0000001450377377m);
			enumUnitInfoMap[153] = new UnitInfo("Standard Atmosphere", new string[1] { "atm" }, UnitCategory.Pressure, 0.0000098692m);
			enumUnitInfoMap[154] = new UnitInfo("Watt", new string[1] { "W" }, UnitCategory.Power, 1m);
			enumUnitInfoMap[155] = new UnitInfo("Kilowatt", new string[1] { "kW" }, UnitCategory.Power, 0.001m);
			enumUnitInfoMap[156] = new UnitInfo("Megawatt", new string[1] { "MW" }, UnitCategory.Power, 0.000001m);
			enumUnitInfoMap[157] = new UnitInfo("Gigawatt", new string[1] { "GW" }, UnitCategory.Power, 0.000000001m);
			enumUnitInfoMap[158] = new UnitInfo("Terawatt", new string[1] { "TW" }, UnitCategory.Power, 0.000000000001m);
			enumUnitInfoMap[159] = new UnitInfo("Horsepower", new string[2] { "hp", "ft*lbf/s" }, UnitCategory.Power, 0.0013410221m);
			enumUnitInfoMap[160] = new UnitInfo("Joule/Second", new string[1] { "J/s" }, UnitCategory.Power, 1m);
			enumUnitInfoMap[161] = new UnitInfo("Joule/Minute", new string[1] { "J/min" }, UnitCategory.Power, 60m);
			enumUnitInfoMap[162] = new UnitInfo("Joule/Hour", new string[1] { "J/h" }, UnitCategory.Power, 3600m);
			enumUnitInfoMap[163] = new UnitInfo("Kilojoule/Second", new string[1] { "kJ/s" }, UnitCategory.Power, 0.001m);
			enumUnitInfoMap[164] = new UnitInfo("Kilojoule/Minute", new string[1] { "kJ/min" }, UnitCategory.Power, 0.06m);
			enumUnitInfoMap[165] = new UnitInfo("Kilojoule/Hour", new string[1] { "kJ/h" }, UnitCategory.Power, 3.6m);
			enumUnitInfoMap[166] = new UnitInfo("Second", new string[1] { "s" }, UnitCategory.Time, 1.0m);
			enumUnitInfoMap[167] = new UnitInfo("Millisecond", new string[1] { "ms" }, UnitCategory.Time, 1000m);
			enumUnitInfoMap[168] = new UnitInfo("Microsecond", new string[2] { "µs", "us" }, UnitCategory.Time, 1000000m);
			enumUnitInfoMap[169] = new UnitInfo("Nanosecond", new string[1] { "ns" }, UnitCategory.Time, 1000000000m);
			enumUnitInfoMap[170] = new UnitInfo("Minute", new string[1] { "min" }, UnitCategory.Time, 0.0166666666666666666666666667m);
			enumUnitInfoMap[171] = new UnitInfo("Hour", new string[1] { "h" }, UnitCategory.Time, 0.0002777777777777777777777778m);
			enumUnitInfoMap[172] = new UnitInfo("Day", new string[1] { "d" }, UnitCategory.Time, 0.0000115740740740740740740741m);
			enumUnitInfoMap[173] = new UnitInfo("Week", new string[1] { "week" }, UnitCategory.Time, 0.0000016534m);
			enumUnitInfoMap[175] = new UnitInfo("Degree", new string[2] { "°", "d" }, UnitCategory.Angle, 1.0m);
			enumUnitInfoMap[174] = new UnitInfo("Radian", new string[1] { "rad" }, UnitCategory.Angle, 0.0174532925199432777777777778m);
			enumUnitInfoMap[177] = new UnitInfo("Grad", new string[1] { "^g" }, UnitCategory.Angle, 1.1111111111m);
			enumUnitInfoMap[178] = new UnitInfo("Seconds of Angle", new string[1] { "\"" }, UnitCategory.Angle, 3600m);
			enumUnitInfoMap[179] = new UnitInfo("Minutes of Angle", new string[2] { "'", "MOA" }, UnitCategory.Angle, 60m);
			enumUnitInfoMap[180] = new UnitInfo("Mil", new string[1] { "mil" }, UnitCategory.Angle, 17.777777778m);
			enumUnitInfoMap[176] = new UnitInfo("Turn", new string[1] { "turns" }, UnitCategory.Angle, 0.0027777777777777777777777778m);
			enumUnitInfoMap[194] = new UnitInfo("Newton Meter", new string[1] { "N⋅m" }, UnitCategory.Torque, 1m);
			enumUnitInfoMap[195] = new UnitInfo("Newton Centimeter", new string[1] { "N⋅cm" }, UnitCategory.Torque, 100m);
			enumUnitInfoMap[196] = new UnitInfo("Newton Millimeter", new string[1] { "N⋅mm" }, UnitCategory.Torque, 1000m);
			enumUnitInfoMap[197] = new UnitInfo("Kilonewton Meter", new string[1] { "kN⋅m" }, UnitCategory.Torque, 0.001m);
			enumUnitInfoMap[198] = new UnitInfo("Kilogram-force Meter", new string[1] { "kgf⋅m" }, UnitCategory.Torque, 0.1019716213m);
			enumUnitInfoMap[199] = new UnitInfo("Kilogram-force Centimeter", new string[1] { "kgf⋅cm" }, UnitCategory.Torque, 10.19716213m);
			enumUnitInfoMap[200] = new UnitInfo("Kilogram-force Millimeter", new string[1] { "kgf⋅mm" }, UnitCategory.Torque, 101.9716213m);
			enumUnitInfoMap[201] = new UnitInfo("Gram-force Meter", new string[1] { "gf⋅m" }, UnitCategory.Torque, 101.9716213m);
			enumUnitInfoMap[202] = new UnitInfo("Gram-force Centimeter", new string[1] { "gf⋅cm" }, UnitCategory.Torque, 10197.16213m);
			enumUnitInfoMap[203] = new UnitInfo("Gram-force Millimeter", new string[1] { "gf⋅mm" }, UnitCategory.Torque, 101971.6213m);
			enumUnitInfoMap[204] = new UnitInfo("Pound-force Feet", new string[2] { "lb⋅ft", "lbf-ft" }, UnitCategory.Torque, 0.7375621212m);
			enumUnitInfoMap[205] = new UnitInfo("Pound-force Inch", new string[2] { "lb⋅in", "lbf-in" }, UnitCategory.Torque, 8.850745454m);
			enumUnitInfoMap[206] = new UnitInfo("Ounce-force Feet", new string[1] { "oz⋅ft" }, UnitCategory.Torque, 11.800994078m);
			enumUnitInfoMap[207] = new UnitInfo("Ounce-force Inch", new string[1] { "oz⋅in" }, UnitCategory.Torque, 141.61192894m);
			enumUnitInfoMap[181] = new UnitInfo("Meters per second squared", new string[2] { "m/s²", "m/s/s" }, UnitCategory.Acceleration, 1.0m);
			enumUnitInfoMap[182] = new UnitInfo("Decimeters per second squared", new string[2] { "dm/s²", "dm/s/s" }, UnitCategory.Acceleration, 10m);
			enumUnitInfoMap[183] = new UnitInfo("Centimeters per second squared", new string[2] { "cm/s²", "cm/s/s" }, UnitCategory.Acceleration, 100m);
			enumUnitInfoMap[184] = new UnitInfo("Millimeters per second squared", new string[2] { "mm/s²", "mm/s/s" }, UnitCategory.Acceleration, 1000m);
			enumUnitInfoMap[185] = new UnitInfo("Micrometers per second squared", new string[2] { "µm/s²", "µm/s/s" }, UnitCategory.Acceleration, 1000000m);
			enumUnitInfoMap[186] = new UnitInfo("Dekameters per second squared", new string[2] { "Dm/s²", "Dm/s/s" }, UnitCategory.Acceleration, 0.1m);
			enumUnitInfoMap[187] = new UnitInfo("Hectometers per second squared", new string[2] { "hm/s²", "hm/s/s" }, UnitCategory.Acceleration, 0.01m);
			enumUnitInfoMap[188] = new UnitInfo("Kilometers per second squared", new string[2] { "km/s²", "km/s/s" }, UnitCategory.Acceleration, 0.001m);
			enumUnitInfoMap[189] = new UnitInfo("Mile per second squared", new string[2] { "mi/s²", "mi/s/s" }, UnitCategory.Acceleration, 0.0006213712m);
			enumUnitInfoMap[190] = new UnitInfo("Yard per second squared", new string[2] { "yd/s²", "yd/s/s" }, UnitCategory.Acceleration, 1.0936132983m);
			enumUnitInfoMap[191] = new UnitInfo("Feet per second squared", new string[2] { "ft/s²", "ft/s/s" }, UnitCategory.Acceleration, 3.280839895m);
			enumUnitInfoMap[192] = new UnitInfo("Inch per second squared", new string[2] { "in/s²", "in/s/s" }, UnitCategory.Acceleration, 39.37007874m);
			enumUnitInfoMap[193] = new UnitInfo("G-Force", new string[1] { "g" }, UnitCategory.Acceleration, 0.1019716213m);
			enumUnitInfoMap[208] = new UnitInfo("Radians per Second", new string[2] { "rad/s", "r/s" }, UnitCategory.AngularVelocity, 1m);
			enumUnitInfoMap[209] = new UnitInfo("Radians per Minute", new string[2] { "rad/min", "r/min" }, UnitCategory.AngularVelocity, 60m);
			enumUnitInfoMap[210] = new UnitInfo("Radians per Hour", new string[2] { "rad/h", "r/h" }, UnitCategory.AngularVelocity, 3600m);
			enumUnitInfoMap[211] = new UnitInfo("Radians per Day", new string[2] { "rad/d", "r/d" }, UnitCategory.AngularVelocity, 86400m);
			enumUnitInfoMap[212] = new UnitInfo("Degrees per Second", new string[2] { "°/s", "d/s" }, UnitCategory.AngularVelocity, 57.295779513m);
			enumUnitInfoMap[213] = new UnitInfo("Degrees per Minute", new string[2] { "°/min", "d/min" }, UnitCategory.AngularVelocity, 3437.7467708m);
			enumUnitInfoMap[214] = new UnitInfo("Degrees per Hour", new string[2] { "°/h", "d/h" }, UnitCategory.AngularVelocity, 206264.80625m);
			enumUnitInfoMap[215] = new UnitInfo("Degrees per Day", new string[2] { "°/d", "d/d" }, UnitCategory.AngularVelocity, 4950355.3499m);
			enumUnitInfoMap[216] = new UnitInfo("Revolutions per Second", new string[1] { "rps" }, UnitCategory.AngularVelocity, 0.1591549431m);
			enumUnitInfoMap[217] = new UnitInfo("Revolutions per Minute", new string[1] { "rpm" }, UnitCategory.AngularVelocity, 9.5492965855m);
			enumUnitInfoMap[218] = new UnitInfo("Revolutions per Hour", new string[1] { "rph" }, UnitCategory.AngularVelocity, 572.95779513m);
			enumUnitInfoMap[219] = new UnitInfo("Revolutions per Day", new string[1] { "rpd" }, UnitCategory.AngularVelocity, 13750.987083m);
			enumUnitInfoMap[220] = new UnitInfo("Hertz", new string[1] { "Hz" }, UnitCategory.Frequency, 1.0m);
			enumUnitInfoMap[221] = new UnitInfo("Kilohertz", new string[1] { "kHz" }, UnitCategory.Frequency, 0.001m);
			enumUnitInfoMap[222] = new UnitInfo("Megahertz", new string[1] { "MHz" }, UnitCategory.Frequency, 0.000001m);
			enumUnitInfoMap[223] = new UnitInfo("Gigahertz", new string[1] { "GHz" }, UnitCategory.Frequency, 0.000000001m);
			enumUnitInfoMap[224] = new UnitInfo("Percent Multiplier", new string[1] { "%m" }, UnitCategory.Percent, 1.0m);
			enumUnitInfoMap[225] = new UnitInfo("Percent", new string[1] { "%" }, UnitCategory.Percent, 100.0m);
			enumUnitInfoMap[226] = new UnitInfo("Permille", new string[1] { "‰" }, UnitCategory.Percent, 1000.0m);
			enumUnitInfoMap[227] = new UnitInfo("Permyriad", new string[1] { "‱" }, UnitCategory.Percent, 10000.0m);
		}

		/// <summary>
		/// Gets all UnitInfo registered, both built-in and custom.
		/// </summary>
		/// <returns>Enumerable of both built-in and custom units.</returns>
		public static IEnumerable<UnitInfo> GetAllUnitInfos()
		{
			for (int i = 0; i < enumUnitInfoMap.Length; i++)
			{
				if (enumUnitInfoMap[i] != null)
				{
					yield return enumUnitInfoMap[i];
				}
			}
			for (int i = 0; i < customUnits.Count; i++)
			{
				yield return customUnits[i];
			}
		}

		/// <summary>
		/// Gets the UnitInfo for the given Units enum value.
		/// </summary>
		/// <param name="unit">Units enum value.</param>
		/// <returns>UnitInfo for the unit.</returns>
		/// <exception cref="T:System.Exception">Throws for invalid unit input.</exception>
		public static UnitInfo GetUnitInfo(Units unit)
		{
			if (TryGetUnitInfo(unit, out var unitInfo))
			{
				return unitInfo;
			}
			throw new Exception($"Failed to find unit info for '{unit}'.");
		}

		/// <summary>
		/// Gets the UnitInfo with the corrosponding name.
		/// </summary>
		/// <param name="unitName">The name of the unit.</param>
		/// <returns>UnitInfo for the name.</returns>
		/// <exception cref="T:System.Exception">Throws when no unit with the given name is found.</exception>
		public static UnitInfo GetUnitInfoByName(string unitName)
		{
			if (TryGetUnitInfoByName(unitName, out var unitInfo))
			{
				return unitInfo;
			}
			throw new Exception("Failed to find unit info by name for '" + unitName + "'.");
		}

		/// <summary>
		/// Finds the UnitInfo that best fits the given symbol within the given category.
		/// </summary>
		/// <param name="symbol">The symbol to find a unit for.</param>
		/// <param name="unitCategory">The category to look for units within.</param>
		/// <returns>The UnitInfo that best matches the given symbol.</returns>
		/// <exception cref="T:System.Exception">Throws when no match was found.</exception>
		public static UnitInfo MatchUnitInfoBySymbol(string symbol, string unitCategory)
		{
			if (TryMatchUnitInfoBySymbol(symbol, unitCategory, out var unitInfo))
			{
				return unitInfo;
			}
			throw new Exception("Could not match any UnitInfo to symbol '" + symbol + "'.");
		}

		/// <summary>
		/// Gets the UnitInfo for the given Units enum value.
		/// </summary>
		/// <param name="unit">Units enum value.</param>
		/// <param name="unitInfo">The UnitInfo matching the given unit value.</param>
		/// <returns><c>true</c> when a UnitInfo was found. Otherwise <c>false</c>.</returns>
		public static bool TryGetUnitInfo(Units unit, out UnitInfo unitInfo)
		{
			if (unit >= Units.Nanometer && (int)unit < enumUnitInfoMap.Length)
			{
				unitInfo = enumUnitInfoMap[(int)unit];
				return unitInfo != null;
			}
			unitInfo = null;
			return false;
		}

		/// <summary>
		/// Gets the UnitInfo with the given name.
		/// </summary>
		/// <param name="unitName">The name of the unit.</param>
		/// <param name="unitInfo">The UnitInfo matching the given name.</param>
		/// <returns><c>true</c> when a UnitInfo was found. Otherwise <c>false</c>.</returns>
		public static bool TryGetUnitInfoByName(string unitName, out UnitInfo unitInfo)
		{
			unitInfo = GetAllUnitInfos().FirstOrDefault((UnitInfo ui) => ui.Name == unitName) ?? enumUnitInfoMap.FirstOrDefault((UnitInfo ui) => ui.Name == unitName);
			return unitInfo != null;
		}

		/// <summary>
		/// Finds the UnitInfo that best fits the given symbol within the given category.
		/// </summary>
		/// <param name="symbol">The symbol to find a unit for.</param>
		/// <param name="unitCategory">The category to look for units within.</param>
		/// <param name="unitInfo">The UnitInfo that best matches the given symbol.</param>
		/// <returns><c>true</c> when a UnitInfo was found. Otherwise <c>false</c>.</returns>
		public static bool TryMatchUnitInfoBySymbol(string symbol, string unitCategory, out UnitInfo unitInfo)
		{
			unitInfo = null;
			if (string.IsNullOrEmpty(symbol))
			{
				return false;
			}
			int score = int.MinValue;
			string matchedOn = null;
			foreach (UnitInfo x in from unitInfo2 in GetAllUnitInfos()
				where unitInfo2.UnitCategory == unitCategory
				select unitInfo2)
			{
				if (string.Equals(x.Name, symbol, StringComparison.OrdinalIgnoreCase))
				{
					unitInfo = x;
					return true;
				}
				if (FuzzySearch.Contains(symbol, x.Name, out var s) && s > score)
				{
					unitInfo = x;
					score = s;
					matchedOn = x.Name;
				}
				if (x.Symbols == null)
				{
					continue;
				}
				for (int i = 0; i < x.Symbols.Length; i++)
				{
					if (!string.IsNullOrEmpty(x.Symbols[i]))
					{
						if (x.Symbols[i] == symbol)
						{
							unitInfo = x;
							return true;
						}
						if (FuzzySearch.Contains(symbol, x.Symbols[i], out s) && s > score)
						{
							unitInfo = x;
							score = s;
							matchedOn = x.Symbols[i];
						}
					}
				}
			}
			return unitInfo != null;
		}

		/// <summary>
		/// Converts between two units. The units must be of the same category.
		/// </summary>
		/// <param name="value">The value to convert. Should be in the <c>from</c> units.</param>
		/// <param name="from">The unit to convert the value from. <c>value</c> should be in this unit.</param>
		/// <param name="to">To unit to convert the value to. Must be the same category as <c>from</c>.</param>
		/// <returns>The <c>value</c> converted to <c>to</c> units.</returns>
		/// <exception cref="T:System.Exception">Throws when either 'from' or 'to' units are invalid, or when the units are of different categories.</exception>
		/// <example>
		/// <code>
		/// decimal meters = 5m;
		/// decimal centimeters = ConvertUnitsFromTo(meters, Units.Meter, Units.Centimeter);
		/// // centimeters = 500
		/// </code>
		/// </example>
		public static decimal ConvertUnitFromTo(decimal value, Units from, Units to)
		{
			if (TryConvertUnitFromTo(value, from, to, out var r))
			{
				return r;
			}
			throw new Exception($"Unable to convert '{from}' to '{to}'.");
		}

		/// <summary>
		/// Converts between two units. The units must be of the same category.
		/// </summary>
		/// <param name="value">The value to convert. Should be in the <c>fromUnitInfo</c> units.</param>
		/// <param name="fromUnitInfo">The unit to convert the value from. <c>value</c> should be in this unit.</param>
		/// <param name="toUnitInfo">To unit to convert the value to. Must be the same category as <c>fromUnitInfo</c>.</param>
		/// <returns>The <c>value</c> converted to <c>toUnitInfo</c> units.</returns>
		/// <exception cref="T:System.Exception">Throws when either 'fromUnitInfo' or 'toUnitInfo' units are invalid, or when the units are of different categories.</exception>
		/// <example>
		/// <code>
		/// decimal meters = 5m;
		/// decimal centimeters = ConvertUnitsFromTo(meters, meterUnitInfo, centimeterUnitInfo);
		/// // centimeters = 500
		/// </code>
		/// </example>
		public static decimal ConvertUnitFromTo(decimal value, UnitInfo fromUnitInfo, UnitInfo toUnitInfo)
		{
			if (TryConvertUnitFromTo(value, fromUnitInfo, toUnitInfo, out var r))
			{
				return r;
			}
			throw new Exception("Unable to convert '" + fromUnitInfo.Name + "' to '" + toUnitInfo.Name + "'.");
		}

		/// <summary>
		/// Converts between two units. The units must be of the same category.
		/// </summary>
		/// <param name="value">The value to convert. Should be in the <c>from</c> units.</param>
		/// <param name="from">The unit to convert the value from. <c>value</c> should be in this unit.</param>
		/// <param name="to">To unit to convert the value to. Must be the same category as <c>from</c>.</param>
		/// <param name="converted">The <c>value</c> converted to <c>to</c> units.</param>
		/// <returns><c>true</c> when the unit was successfully converted. Otherwise <c>false</c>.</returns>
		/// <example>
		/// <code>
		/// decimal meters = 5m;
		/// if (TryConvertUnitsFromTo(meters, Units.Meter, Units.Centimeter, out decimal centimeters)
		/// {
		///     // centimeters = 500
		/// }
		/// </code>
		/// </example>
		public static bool TryConvertUnitFromTo(decimal value, Units from, Units to, out decimal converted)
		{
			if (TryGetUnitInfo(from, out var fromUnitInfo) && TryGetUnitInfo(to, out var toUnitInfo))
			{
				return TryConvertUnitFromTo(value, fromUnitInfo, toUnitInfo, out converted);
			}
			converted = 0.0m;
			return false;
		}

		/// <summary>
		/// Converts between two units. The units must be of the same category.
		/// </summary>
		/// <param name="value">The value to convert. Should be in the <c>fromUnitInfo</c> units.</param>
		/// <param name="fromUnitInfo">The unit to convert the value from. <c>value</c> should be in this unit.</param>
		/// <param name="toUnitInfo">To unit to convert the value to. Must be the same category as <c>fromUnitInfo</c>.</param>
		/// <param name="converted">The <c>value</c> converted to <c>toUnitInfo</c> units.</param>
		/// <returns><c>true</c> when the unit was successfully converted. Otherwise <c>false</c>.</returns>
		/// <example>
		/// <code>
		/// decimal meters = 5m;
		/// if (TryConvertUnitsFromTo(meters, meterUnitInfo, centimeterUnitInfo, out decimal centimeters))
		/// {
		///     // centimeters = 500
		/// }
		/// </code>
		/// </example>
		/// <exception cref="T:System.ArgumentNullException">Throws if either fromUnitInfo or toUnitInfo is null.</exception>
		public static bool TryConvertUnitFromTo(decimal value, UnitInfo fromUnitInfo, UnitInfo toUnitInfo, out decimal converted)
		{
			if (fromUnitInfo == null)
			{
				throw new ArgumentNullException("fromUnitInfo");
			}
			if (toUnitInfo == null)
			{
				throw new ArgumentNullException("toUnitInfo");
			}
			if (fromUnitInfo == toUnitInfo)
			{
				converted = value;
				return true;
			}
			if (!CanConvertBetween(fromUnitInfo, toUnitInfo))
			{
				converted = 0.0m;
				return false;
			}
			if (fromUnitInfo.UsesCustomConversion || toUnitInfo.UsesCustomConversion)
			{
				converted = default(decimal);
				if (fromUnitInfo.ConvertFromBase != null)
				{
					converted = fromUnitInfo.ConvertToBase(value);
				}
				else
				{
					converted = value / fromUnitInfo.Multiplier;
				}
				if (toUnitInfo.ConvertFromBase != null)
				{
					converted = toUnitInfo.ConvertFromBase(converted);
				}
				else
				{
					converted *= toUnitInfo.Multiplier;
				}
			}
			else
			{
				decimal m = toUnitInfo.Multiplier / fromUnitInfo.Multiplier;
				converted = value * m;
			}
			return true;
		}

		public static decimal ConvertUnitFromToWithError(decimal value, UnitInfo fromUnitInfo, UnitInfo toUnitInfo, out string error)
		{
			if (fromUnitInfo == null)
			{
				throw new ArgumentNullException("fromUnitInfo");
			}
			if (toUnitInfo == null)
			{
				throw new ArgumentNullException("toUnitInfo");
			}
			if (fromUnitInfo == toUnitInfo)
			{
				error = null;
				return value;
			}
			if (!CanConvertBetween(fromUnitInfo, toUnitInfo))
			{
				error = "Invalid conversion";
				return value;
			}
			try
			{
				decimal converted;
				if (fromUnitInfo.UsesCustomConversion || toUnitInfo.UsesCustomConversion)
				{
					converted = default(decimal);
					converted = ((fromUnitInfo.ConvertFromBase == null) ? (value / fromUnitInfo.Multiplier) : fromUnitInfo.ConvertToBase(value));
					if (toUnitInfo.ConvertFromBase != null)
					{
						converted = toUnitInfo.ConvertFromBase(converted);
					}
					else
					{
						converted *= toUnitInfo.Multiplier;
					}
				}
				else
				{
					decimal m = toUnitInfo.Multiplier / fromUnitInfo.Multiplier;
					converted = value * m;
				}
				error = null;
				return converted;
			}
			catch (OverflowException)
			{
				error = "Overflow";
				return value;
			}
		}

		/// <summary>
		/// Indicates whether or not a value can be converted between the given <c>a</c> and <c>b</c> units.
		/// </summary>
		/// <param name="a">Unit a.</param>
		/// <param name="b">Unit b.</param>
		/// <returns><c>true</c> if both units have the same category. Otherwise <c>false</c>.</returns>
		public static bool CanConvertBetween(Units a, Units b)
		{
			if (!TryGetUnitInfo(a, out var unitA))
			{
				throw new ArgumentException($"Unit not found for '{a}'.", "a");
			}
			if (!TryGetUnitInfo(b, out var unitB))
			{
				throw new ArgumentException($"Unit not found for '{b}'.", "b");
			}
			return CanConvertBetween(unitA, unitB);
		}

		/// <summary>
		/// Indicates whether or not a value can be converted between the given <c>a</c> and <c>b</c> units.
		/// </summary>
		/// <param name="a">Unit a.</param>
		/// <param name="b">Unit b.</param>
		/// <returns><c>true</c> if both units have the same category. Otherwise <c>false</c>.</returns>
		public static bool CanConvertBetween(UnitInfo a, UnitInfo b)
		{
			if (a == null)
			{
				throw new ArgumentNullException("a");
			}
			if (b == null)
			{
				throw new ArgumentNullException("b");
			}
			return a.UnitCategory == b.UnitCategory;
		}

		/// <summary>
		/// Adds a custom unit to the UnitNumberUtility, that can also be used with the <see cref="!:&gt;UnitAttribute" />.
		/// Call this using InitializeOnLoad or InitializeOnLoadMethod.
		/// </summary>
		/// <param name="name">The name of the unit. Duplicate names are not allowed.</param>
		/// <param name="symbols">Symbols used for the unit. First value in the array will be used as the primary symbol. Atleast 1 value required. Duplicate symbols are not allowed within the same category.</param>
		/// <param name="unitCategory">The category of the unit. Units can only be converted to another of the same category. Custom categories are allowed.</param>
		/// <param name="multiplier">The multiplier to convert the unit from the base value. For example, meters are the base unit of the distance category, therefore centimeters have a multipler of 100.</param>
		/// <example>
		/// <para>Example of adding centimeters as a custom unit.</para>
		/// <code>
		/// UnitNumberUtility.AddCustomUnit("Centimeter", new string[]{ "cm" }, "Distance", 100m);
		/// </code>
		/// </example>
		public static void AddCustomUnit(string name, string[] symbols, string unitCategory, decimal multiplier)
		{
			InternalAddCustomUnit(name, symbols, unitCategory, useCustomConversion: false, multiplier, null, null);
		}

		/// <summary>
		/// Adds a custom unit to the UnitNumberUtility, that can also be used with the <see cref="!:&gt;UnitAttribute" />.
		/// Call this using InitializeOnLoad or InitializeOnLoadMethod.
		/// </summary>
		/// <param name="name">The name of the unit. Duplicate names are not allowed.</param>
		/// <param name="symbols">Symbols used for the unit. First value in the array will be used as the primary symbol. Atleast 1 value required. Duplicate symbols are not allowed within the same category.</param>
		/// <param name="unitCategory">The category of the unit. Units can only be converted to another of the same category. Custom categories are allowed.</param>
		/// <param name="multiplier">The multiplier to convert the unit from the base value. For example, meters are the base unit of the distance category, therefore centimeters have a multipler of 100.</param>
		/// <example>
		/// <para>Example of adding centimeters as a custom unit.</para>
		/// <code>
		/// UnitNumberUtility.AddCustomUnit("Centimeter", new string[]{ "cm" }, UnitCategory.Distance, 100m);
		/// </code>
		/// </example>
		public static void AddCustomUnit(string name, string[] symbols, UnitCategory unitCategory, decimal multiplier)
		{
			InternalAddCustomUnit(name, symbols, unitCategory.ToString(), useCustomConversion: false, multiplier, null, null);
		}

		/// <summary>
		/// Adds a custom unit to the UnitNumberUtility, that can also be used with the <see cref="!:&gt;UnitAttribute" />.
		/// This overload allows for custom conversion methods but, if possible, the multiplier overloads should be prefered.
		/// Call this using InitializeOnLoad or InitializeOnLoadMethod.
		/// </summary>
		/// <param name="name">The name of the unit. Duplicate names are not allowed.</param>
		/// <param name="symbols">Symbols used for the unit. First value in the array will be used as the primary symbol. Atleast 1 value required. Duplicate symbols are not allowed within the same category.</param>
		/// <param name="unitCategory">The category of the unit. Units can only be converted to another of the same category. Custom categories are allowed.</param>
		/// <param name="convertToBase">Method for converting a given value of the custom unit to the base unit. For example, for centimeter, use: <c>x =&gt; x / 100m;</c>.</param>
		/// <param name="convertFromBase">Method for converting a given value of the base unit to the custom unit. For example, for centimeter, use: <c>x =&gt; x * 100m;</c>.</param>
		/// <example>
		/// <para>Example of adding centimeters as a custom unit.</para>
		/// <code>
		/// UnitNumberUtility.AddCustomUnit("Centimeter", new string[]{ "cm" }, "Distance", x =&gt; x / 100m, x = &gt; x * 100m);
		/// </code>
		/// </example>
		public static void AddCustomUnit(string name, string[] symbols, string unitCategory, Func<decimal, decimal> convertToBase, Func<decimal, decimal> convertFromBase)
		{
			InternalAddCustomUnit(name, symbols, unitCategory, useCustomConversion: true, 0m, convertToBase, convertFromBase);
		}

		/// <summary>
		/// Adds a custom unit to the UnitNumberUtility, that can also be used with the <see cref="!:&gt;UnitAttribute" />.
		/// This overload allows for custom conversion methods but, if possible, the multiplier overloads should be prefered.
		/// Call this using InitializeOnLoad or InitializeOnLoadMethod.
		/// </summary>
		/// <param name="name">The name of the unit. Duplicate names are not allowed.</param>
		/// <param name="symbols">Symbols used for the unit. First value in the array will be used as the primary symbol. Atleast 1 value required. Duplicate symbols are not allowed within the same category.</param>
		/// <param name="unitCategory">The category of the unit. Units can only be converted to another of the same category. Custom categories are allowed.</param>
		/// <param name="convertToBase">Method for converting a given value of the custom unit to the base unit. For example, for centimeter, use: <c>x =&gt; x / 100m;</c>.</param>
		/// <param name="convertFromBase">Method for converting a given value of the base unit to the custom unit. For example, for centimeter, use: <c>x =&gt; x * 100m;</c>.</param>
		/// /// <example>
		/// <para>Example of adding centimeters as a custom unit.</para>
		/// <code>
		/// UnitNumberUtility.AddCustomUnit("Centimeter", new string[]{ "cm" }, UnitCategory.Distance, x =&gt; x / 100m, x = &gt; x * 100m);
		/// </code>
		/// </example>
		public static void AddCustomUnit(string name, string[] symbols, UnitCategory unitCategory, Func<decimal, decimal> convertToBase, Func<decimal, decimal> convertFromBase)
		{
			InternalAddCustomUnit(name, symbols, unitCategory.ToString(), useCustomConversion: true, 0m, convertToBase, convertFromBase);
		}

		private static void InternalAddCustomUnit(string name, string[] symbols, string unitCategory, bool useCustomConversion, decimal multiplier, Func<decimal, decimal> convertToBase, Func<decimal, decimal> convertFromBase)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (symbols == null)
			{
				throw new ArgumentNullException("symbols");
			}
			if (symbols.Length == 0)
			{
				throw new ArgumentException("Atleast 1 symbol is required.", "symbols");
			}
			if (string.IsNullOrEmpty(unitCategory))
			{
				throw new ArgumentNullException("unitCategory");
			}
			if (useCustomConversion)
			{
				if (convertToBase == null)
				{
					throw new ArgumentNullException("convertToBase");
				}
				if (convertFromBase == null)
				{
					throw new ArgumentNullException("convertFromBase");
				}
			}
			else if (multiplier == 0m)
			{
				throw new ArgumentException("multiplier");
			}
			if (TryGetUnitInfoByName(name, out var _))
			{
				throw new Exception("Duplicate unit name '" + name + "' is not allowed.");
			}
			foreach (string symbol in symbols)
			{
				if (string.IsNullOrEmpty(symbol))
				{
					throw new ArgumentNullException("Elements of symbols are not allowed to be null or empty.", "symbols");
				}
				foreach (UnitInfo x in from unitInfo2 in GetAllUnitInfos()
					where unitInfo2.UnitCategory == unitCategory
					select unitInfo2)
				{
					if (x.Symbols.Contains<string>(symbol, StringComparer.Ordinal))
					{
						throw new Exception("Duplicate unit symbol '" + symbol + "' is not allowed in within type '" + unitCategory + "'.");
					}
				}
			}
			if (useCustomConversion)
			{
				customUnits.Add(new UnitInfo(name, symbols, unitCategory, convertToBase, convertFromBase));
			}
			else
			{
				customUnits.Add(new UnitInfo(name, symbols, unitCategory, multiplier));
			}
		}
	}
}
