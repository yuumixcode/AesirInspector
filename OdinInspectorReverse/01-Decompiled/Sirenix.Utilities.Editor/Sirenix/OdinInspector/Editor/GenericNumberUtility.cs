using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public static class GenericNumberUtility
	{
		private static HashSet<Type> Numbers = new HashSet<Type>(FastTypeComparer.Instance)
		{
			typeof(sbyte),
			typeof(byte),
			typeof(short),
			typeof(ushort),
			typeof(int),
			typeof(uint),
			typeof(long),
			typeof(ulong),
			typeof(float),
			typeof(double),
			typeof(decimal),
			typeof(IntPtr),
			typeof(UIntPtr)
		};

		private static HashSet<Type> Vectors = new HashSet<Type>(FastTypeComparer.Instance)
		{
			typeof(Vector2),
			typeof(Vector2Int),
			typeof(Vector3),
			typeof(Vector3Int),
			typeof(Vector4)
		};

		public static bool IsNumber(Type type)
		{
			return Numbers.Contains(type);
		}

		public static bool IsVector(Type type)
		{
			return Vectors.Contains(type);
		}

		public static bool NumberIsInRange(object number, double min, double max)
		{
			if (number is sbyte n)
			{
				if ((double)n >= min)
				{
					return (double)n <= max;
				}
				return false;
			}
			if (number is byte n2)
			{
				if ((double)(int)n2 >= min)
				{
					return (double)(int)n2 <= max;
				}
				return false;
			}
			if (number is short n3)
			{
				if ((double)n3 >= min)
				{
					return (double)n3 <= max;
				}
				return false;
			}
			if (number is ushort n4)
			{
				if ((double)(int)n4 >= min)
				{
					return (double)(int)n4 <= max;
				}
				return false;
			}
			if (number is int n5)
			{
				if ((double)n5 >= min)
				{
					return (double)n5 <= max;
				}
				return false;
			}
			if (number is uint n6)
			{
				if ((double)n6 >= min)
				{
					return (double)n6 <= max;
				}
				return false;
			}
			if (number is long n7)
			{
				if ((double)n7 >= min)
				{
					return (double)n7 <= max;
				}
				return false;
			}
			if (number is ulong n8)
			{
				if ((double)n8 >= min)
				{
					return (double)n8 <= max;
				}
				return false;
			}
			if (number is float n9)
			{
				return IsFloatInRange(n9, min, max);
			}
			if (number is double n10)
			{
				return IsDoubleInRange(n10, min, max);
			}
			if (number is decimal n11)
			{
				if (n11 >= (decimal)min)
				{
					return n11 <= (decimal)max;
				}
				return false;
			}
			if (number is Vector2 n12)
			{
				if (IsFloatInRange(n12.x, min, max))
				{
					return IsFloatInRange(n12.y, min, max);
				}
				return false;
			}
			if (number is Vector2Int n13)
			{
				if ((double)n13.x >= min && (double)n13.x <= max && (double)n13.y >= min)
				{
					return (double)n13.y <= max;
				}
				return false;
			}
			if (number is Vector3 n14)
			{
				if (IsFloatInRange(n14.x, min, max) && IsFloatInRange(n14.y, min, max))
				{
					return IsFloatInRange(n14.z, min, max);
				}
				return false;
			}
			if (number is Vector3Int n15)
			{
				if ((double)n15.x >= min && (double)n15.x <= max && (double)n15.y >= min && (double)n15.y <= max && (double)n15.z >= min)
				{
					return (double)n15.z <= max;
				}
				return false;
			}
			if (number is Vector4 n16)
			{
				if (IsFloatInRange(n16.x, min, max) && IsFloatInRange(n16.y, min, max) && IsFloatInRange(n16.z, min, max))
				{
					return IsFloatInRange(n16.w, min, max);
				}
				return false;
			}
			if (number is IntPtr)
			{
				long n17 = (long)(IntPtr)number;
				if ((double)n17 >= min)
				{
					return (double)n17 <= max;
				}
				return false;
			}
			if (number is UIntPtr)
			{
				ulong n18 = (ulong)(UIntPtr)number;
				if ((double)n18 >= min)
				{
					return (double)n18 <= max;
				}
				return false;
			}
			return false;
		}

		internal static bool NumberIsInRange(object number, double min, double max, out string errorMessage)
		{
			errorMessage = null;
			if (number is float f)
			{
				errorMessage = FormatFloatIssue("Value", f);
				return errorMessage == null;
			}
			if (number is double d)
			{
				errorMessage = FormatDoubleIssue("Value", d);
				return errorMessage == null;
			}
			if (number is decimal dec)
			{
				if (dec < (decimal)min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {dec}.";
				}
				else if (dec > (decimal)max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {dec}.";
				}
				return errorMessage == null;
			}
			if (number is sbyte sb)
			{
				if ((double)sb < min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {sb}.";
				}
				else if ((double)sb > max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {sb}.";
				}
				return errorMessage == null;
			}
			if (number is byte b)
			{
				if ((double)(int)b < min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {b}.";
				}
				else if ((double)(int)b > max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {b}.";
				}
				return errorMessage == null;
			}
			if (number is short s)
			{
				if ((double)s < min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {s}.";
				}
				else if ((double)s > max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {s}.";
				}
				return errorMessage == null;
			}
			if (number is ushort us)
			{
				if ((double)(int)us < min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {us}.";
				}
				else if ((double)(int)us > max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {us}.";
				}
				return errorMessage == null;
			}
			if (number is int i)
			{
				if ((double)i < min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {i}.";
				}
				else if ((double)i > max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {i}.";
				}
				return errorMessage == null;
			}
			if (number is uint ui)
			{
				if ((double)ui < min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {ui}.";
				}
				else if ((double)ui > max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {ui}.";
				}
				return errorMessage == null;
			}
			if (number is long l)
			{
				if ((double)l < min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {l}.";
				}
				else if ((double)l > max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {l}.";
				}
				return errorMessage == null;
			}
			if (number is ulong ul)
			{
				if ((double)ul < min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {ul}.";
				}
				else if ((double)ul > max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {ul}.";
				}
				return errorMessage == null;
			}
			if (number is IntPtr ip)
			{
				long val = (long)ip;
				if ((double)val < min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {val}.";
				}
				else if ((double)val > max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {val}.";
				}
				return errorMessage == null;
			}
			if (number is UIntPtr uip)
			{
				ulong val2 = (ulong)uip;
				if ((double)val2 < min)
				{
					errorMessage = $"Value must be ≥ {min}, but is {val2}.";
				}
				else if ((double)val2 > max)
				{
					errorMessage = $"Value must be ≤ {max}, but is {val2}.";
				}
				return errorMessage == null;
			}
			if (number is Vector2 v2)
			{
				List<string> issues = new List<string>();
				string x = FormatFloatIssue("x", v2.x);
				string y = FormatFloatIssue("y", v2.y);
				if (x != null)
				{
					issues.Add(x);
				}
				if (y != null)
				{
					issues.Add(y);
				}
				errorMessage = ((issues.Count > 0) ? string.Join("\n", issues.ToArray()) : null);
				return errorMessage == null;
			}
			if (number is Vector3 v3)
			{
				List<string> issues2 = new List<string>();
				string x2 = FormatFloatIssue("x", v3.x);
				string y2 = FormatFloatIssue("y", v3.y);
				string z = FormatFloatIssue("z", v3.z);
				if (x2 != null)
				{
					issues2.Add(x2);
				}
				if (y2 != null)
				{
					issues2.Add(y2);
				}
				if (z != null)
				{
					issues2.Add(z);
				}
				errorMessage = ((issues2.Count > 0) ? string.Join("\n", issues2.ToArray()) : null);
				return errorMessage == null;
			}
			if (number is Vector4 v4)
			{
				List<string> issues3 = new List<string>();
				string x3 = FormatFloatIssue("x", v4.x);
				string y3 = FormatFloatIssue("y", v4.y);
				string z2 = FormatFloatIssue("z", v4.z);
				string w = FormatFloatIssue("w", v4.w);
				if (x3 != null)
				{
					issues3.Add(x3);
				}
				if (y3 != null)
				{
					issues3.Add(y3);
				}
				if (z2 != null)
				{
					issues3.Add(z2);
				}
				if (w != null)
				{
					issues3.Add(w);
				}
				errorMessage = ((issues3.Count > 0) ? string.Join("\n", issues3.ToArray()) : null);
				return errorMessage == null;
			}
			if (number is Vector2Int v2i)
			{
				List<string> issues4 = new List<string>();
				if ((double)v2i.x < min)
				{
					issues4.Add($"x must be ≥ {min}, but is {v2i.x}.");
				}
				if ((double)v2i.x > max)
				{
					issues4.Add($"x must be ≤ {max}, but is {v2i.x}.");
				}
				if ((double)v2i.y < min)
				{
					issues4.Add($"y must be ≥ {min}, but is {v2i.y}.");
				}
				if ((double)v2i.y > max)
				{
					issues4.Add($"y must be ≤ {max}, but is {v2i.y}.");
				}
				errorMessage = ((issues4.Count > 0) ? string.Join("\n", issues4.ToArray()) : null);
				return errorMessage == null;
			}
			if (number is Vector3Int v3i)
			{
				List<string> issues5 = new List<string>();
				if ((double)v3i.x < min)
				{
					issues5.Add($"x must be ≥ {min}, but is {v3i.x}.");
				}
				if ((double)v3i.x > max)
				{
					issues5.Add($"x must be ≤ {max}, but is {v3i.x}.");
				}
				if ((double)v3i.y < min)
				{
					issues5.Add($"y must be ≥ {min}, but is {v3i.y}.");
				}
				if ((double)v3i.y > max)
				{
					issues5.Add($"y must be ≤ {max}, but is {v3i.y}.");
				}
				if ((double)v3i.z < min)
				{
					issues5.Add($"z must be ≥ {min}, but is {v3i.z}.");
				}
				if ((double)v3i.z > max)
				{
					issues5.Add($"z must be ≤ {max}, but is {v3i.z}.");
				}
				errorMessage = ((issues5.Count > 0) ? string.Join("\n", issues5.ToArray()) : null);
				return errorMessage == null;
			}
			errorMessage = "Unsupported type or not a numeric or vector value.";
			return false;
			string FormatDoubleIssue(string label, double num)
			{
				if (double.IsNaN(num))
				{
					return $"{label} must be a number between {min} and {max}, but is NaN.";
				}
				if (double.IsPositiveInfinity(num))
				{
					if (double.IsPositiveInfinity(max))
					{
						return null;
					}
					return $"{label} must be ≤ {max}, but is Infinity.";
				}
				if (double.IsNegativeInfinity(num))
				{
					if (double.IsNegativeInfinity(min))
					{
						return null;
					}
					return $"{label} must be ≥ {min}, but is -Infinity.";
				}
				if (num < min)
				{
					return $"{label} must be ≥ {min}, but is {num}.";
				}
				if (num > max)
				{
					return $"{label} must be ≤ {max}, but is {num}.";
				}
				return null;
			}
			string FormatFloatIssue(string label, float num)
			{
				if (float.IsNaN(num))
				{
					return $"{label} must be a number between {min} and {max}, but is NaN.";
				}
				if (float.IsPositiveInfinity(num))
				{
					if (double.IsPositiveInfinity(max))
					{
						return null;
					}
					return $"{label} must be ≤ {max}, but is Infinity.";
				}
				if (float.IsNegativeInfinity(num))
				{
					if (double.IsNegativeInfinity(min))
					{
						return null;
					}
					return $"{label} must be ≥ {min}, but is -Infinity.";
				}
				if ((double)num < min)
				{
					return $"{label} must be ≥ {min}, but is {num}.";
				}
				if ((double)num > max)
				{
					return $"{label} must be ≤ {max}, but is {num}.";
				}
				return null;
			}
		}

		internal static bool IsFloatInRange(float value, double min, double max)
		{
			if (!float.IsNaN(value))
			{
				if (value != float.NegativeInfinity)
				{
					if (value == float.PositiveInfinity)
					{
						return double.IsPositiveInfinity(max);
					}
					if ((double)value >= min)
					{
						return (double)value <= max;
					}
					return false;
				}
				return double.IsNegativeInfinity(min);
			}
			return false;
		}

		internal static bool IsDoubleInRange(double value, double min, double max)
		{
			if (!double.IsNaN(value))
			{
				if (value != double.NegativeInfinity)
				{
					if (value == double.PositiveInfinity)
					{
						return double.IsPositiveInfinity(max);
					}
					if (value >= min)
					{
						return value <= max;
					}
					return false;
				}
				return double.IsNegativeInfinity(min);
			}
			return false;
		}

		public static T Clamp<T>(T number, double min, double max)
		{
			if (number is sbyte n)
			{
				if ((double)n < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)n > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is byte n2)
			{
				if ((double)(int)n2 < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)(int)n2 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is short n3)
			{
				if ((double)n3 < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)n3 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is ushort n4)
			{
				if ((double)(int)n4 < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)(int)n4 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is int n5)
			{
				if ((double)n5 < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)n5 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is uint n6)
			{
				if ((double)n6 < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)n6 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is long n7)
			{
				if ((double)n7 < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)n7 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is ulong n8)
			{
				if ((double)n8 < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)n8 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is float n9)
			{
				if ((double)n9 < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)n9 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is double n10)
			{
				if (n10 < min)
				{
					return ConvertNumber<T>(min);
				}
				if (n10 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is decimal n11)
			{
				if (n11 < (decimal)min)
				{
					return ConvertNumber<T>(min);
				}
				if (n11 > (decimal)max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is Vector2 n12)
			{
				if ((double)n12.x < min)
				{
					n12.x = (float)min;
				}
				else if ((double)n12.x > max)
				{
					n12.x = (float)max;
				}
				if ((double)n12.y < min)
				{
					n12.y = (float)min;
				}
				else if ((double)n12.y > max)
				{
					n12.y = (float)max;
				}
				return (T)(object)n12;
			}
			if (number is Vector2Int n13)
			{
				if ((double)n13.x < min)
				{
					n13.x = (int)min;
				}
				else if ((double)n13.x > max)
				{
					n13.x = (int)max;
				}
				if ((double)n13.y < min)
				{
					n13.y = (int)min;
				}
				else if ((double)n13.y > max)
				{
					n13.y = (int)max;
				}
				return (T)(object)n13;
			}
			if (number is Vector3 n14)
			{
				if ((double)n14.x < min)
				{
					n14.x = (float)min;
				}
				else if ((double)n14.x > max)
				{
					n14.x = (float)max;
				}
				if ((double)n14.y < min)
				{
					n14.y = (float)min;
				}
				else if ((double)n14.y > max)
				{
					n14.y = (float)max;
				}
				if ((double)n14.z < min)
				{
					n14.z = (float)min;
				}
				else if ((double)n14.z > max)
				{
					n14.z = (float)max;
				}
				return (T)(object)n14;
			}
			if (number is Vector3Int n15)
			{
				if ((double)n15.x < min)
				{
					n15.x = (int)min;
				}
				else if ((double)n15.x > max)
				{
					n15.x = (int)max;
				}
				if ((double)n15.y < min)
				{
					n15.y = (int)min;
				}
				else if ((double)n15.y > max)
				{
					n15.y = (int)max;
				}
				if ((double)n15.z < min)
				{
					n15.z = (int)min;
				}
				else if ((double)n15.z > max)
				{
					n15.z = (int)max;
				}
				return (T)(object)n15;
			}
			if (number is Vector4 n16)
			{
				if ((double)n16.x < min)
				{
					n16.x = (float)min;
				}
				else if ((double)n16.x > max)
				{
					n16.x = (float)max;
				}
				if ((double)n16.y < min)
				{
					n16.y = (float)min;
				}
				else if ((double)n16.y > max)
				{
					n16.y = (float)max;
				}
				if ((double)n16.z < min)
				{
					n16.z = (float)min;
				}
				else if ((double)n16.z > max)
				{
					n16.z = (float)max;
				}
				if ((double)n16.w < min)
				{
					n16.w = (float)min;
				}
				else if ((double)n16.w > max)
				{
					n16.w = (float)max;
				}
				return (T)(object)n16;
			}
			if (number is IntPtr)
			{
				long n17 = (long)(IntPtr)(object)number;
				if ((double)n17 < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)n17 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			if (number is UIntPtr)
			{
				ulong n18 = (ulong)(long)(IntPtr)(object)number;
				if ((double)n18 < min)
				{
					return ConvertNumber<T>(min);
				}
				if ((double)n18 > max)
				{
					return ConvertNumber<T>(max);
				}
				return number;
			}
			return number;
		}

		public static T ConvertNumber<T>(object value)
		{
			return (T)Convert.ChangeType(value, typeof(T));
		}

		public static object ConvertNumberWeak(object value, Type to)
		{
			return Convert.ChangeType(value, to);
		}
	}
}
