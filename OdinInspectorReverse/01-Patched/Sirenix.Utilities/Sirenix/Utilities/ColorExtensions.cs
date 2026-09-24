using System;
using System.Globalization;
using UnityEngine;

namespace Sirenix.Utilities
{
	/// <summary>
	/// Extension methods for the UnityEngine.Color type.
	/// </summary>
	public static class ColorExtensions
	{
		private static readonly char[] trimRGBStart = new char[9] { 'R', 'r', 'G', 'g', 'B', 'b', 'A', 'a', '(' };

		/// <summary>
		/// Lerps between multiple colors.
		/// </summary>
		/// <param name="colors">The colors.</param>
		/// <param name="t">The t.</param>
		/// <returns></returns>
		public static Color Lerp(this Color[] colors, float t)
		{
			t = Mathf.Clamp(t, 0f, 1f) * (float)(colors.Length - 1);
			int a = (int)t;
			int b = Mathf.Min((int)t + 1, colors.Length - 1);
			return Color.Lerp(colors[a], colors[b], t - (float)(int)t);
		}

		/// <summary>
		/// Moves the towards implementation for Color.
		/// </summary>
		/// <param name="from">From color.</param>
		/// <param name="to">To color.</param>
		/// <param name="maxDelta">The maximum delta.</param>
		public static Color MoveTowards(this Color from, Color to, float maxDelta)
		{
			Color result = new Color
			{
				r = Mathf.MoveTowards(from.r, to.r, maxDelta),
				g = Mathf.MoveTowards(from.g, to.g, maxDelta),
				b = Mathf.MoveTowards(from.b, to.b, maxDelta),
				a = Mathf.MoveTowards(from.a, to.a, maxDelta)
			};
			from.r = result.r;
			from.g = result.g;
			from.b = result.b;
			from.a = result.a;
			return result;
		}

		/// <summary>
		/// Tries to parse a string to a Color. The following formats are supported:
		/// "new Color(0.4, 0, 0, 1)", "#FFEEBBFF", "#FFEECC", "FFEEBBFF", "FFEECC"
		/// </summary>
		/// <param name="colorStr">The color string.</param>
		/// <param name="color">The color.</param>
		/// <returns>Returns true if the parse was a success.</returns>
		public static bool TryParseString(string colorStr, out Color color)
		{
			color = default(Color);
			if (colorStr == null || colorStr.Length < 2 || colorStr.Length > 100)
			{
				return false;
			}
			if (colorStr.StartsWith("new Color", StringComparison.InvariantCulture))
			{
				colorStr = colorStr.Substring("new Color".Length, colorStr.Length - "new Color".Length).Replace("f", "");
			}
			bool couldBeHex = colorStr[0] == '#' || char.IsLetter(colorStr[0]) || char.IsNumber(colorStr[0]);
			bool couldBeRGB = colorStr[0] == 'R' || colorStr[0] == '(' || char.IsNumber(colorStr[0]);
			if (!couldBeHex && !couldBeRGB)
			{
				return false;
			}
			bool didConvert = false;
			if (couldBeRGB || (couldBeHex && !(didConvert = ColorUtility.TryParseHtmlString(colorStr, out color)) && couldBeRGB))
			{
				colorStr = colorStr.TrimStart(trimRGBStart).TrimEnd(new char[1] { ')' });
				string[] components = colorStr.Split(new char[1] { ',' });
				if (components.Length < 2 || components.Length > 4)
				{
					return false;
				}
				Color result = new Color(0f, 0f, 0f, 1f);
				for (int i = 0; i < components.Length; i++)
				{
					if (!float.TryParse(components[i], out var component))
					{
						return false;
					}
					if (i == 0)
					{
						result.r = component;
					}
					if (i == 1)
					{
						result.g = component;
					}
					if (i == 2)
					{
						result.b = component;
					}
					if (i == 3)
					{
						result.a = component;
					}
				}
				color = result;
				return true;
			}
			if (didConvert)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Converts a color to a string formatted to c#
		/// </summary>
		/// <param name="color">The color.</param>
		/// <returns>new Color(r, g, b, a)</returns>
		public static string ToCSharpColor(this Color color)
		{
			return "new Color(" + TrimFloat(color.r) + "f, " + TrimFloat(color.g) + "f, " + TrimFloat(color.b) + "f, " + TrimFloat(color.a) + "f)";
		}

		/// <summary>
		/// Pows the color with the specified factor.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <param name="factor">The factor.</param>
		public static Color Pow(this Color color, float factor)
		{
			color.r = Mathf.Pow(color.r, factor);
			color.g = Mathf.Pow(color.g, factor);
			color.b = Mathf.Pow(color.b, factor);
			color.a = Mathf.Pow(color.a, factor);
			return color;
		}

		/// <summary>
		/// Normalizes the RGB values of the color ignoring the alpha value.
		/// </summary>
		/// <param name="color">The color.</param>
		public static Color NormalizeRGB(this Color color)
		{
			Vector3 c = new Vector3(color.r, color.g, color.b).normalized;
			color.r = c.x;
			color.g = c.y;
			color.b = c.z;
			return color;
		}

		/// <summary>
		/// Gets the perceived luminosity of a given <see cref="T:UnityEngine.Color" />.
		/// </summary>
		/// <param name="color">The current <see cref="T:UnityEngine.Color" />.</param>
		/// <param name="includeAlpha">Determines if the <see cref="F:UnityEngine.Color.a">Color.a</see> value should impact the result.</param>
		/// <returns>The <see cref="T:System.Single">float</see> value representing the luminosity.</returns>
		public static float PerceivedLuminosity(this Color color, bool includeAlpha = true)
		{
			if (includeAlpha && color.a <= 0f)
			{
				return 0f;
			}
			float luminosity = 0.3f * color.r + 0.59f * color.g + 0.11f * color.b;
			if (includeAlpha && color.a < 1f)
			{
				luminosity *= color.a;
			}
			return luminosity;
		}

		private static string TrimFloat(float value)
		{
			string str = value.ToString("F3", CultureInfo.InvariantCulture).TrimEnd(new char[1] { '0' });
			char lastChar = str[str.Length - 1];
			if (lastChar == '.' || lastChar == ',')
			{
				str = str.Substring(0, str.Length - 1);
			}
			return str;
		}
	}
}
