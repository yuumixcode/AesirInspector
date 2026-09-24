using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.ValueResolvers
{
	public class ColorValueResolverCreator : ValueResolverCreator
	{
		private static Dictionary<string, Color> _colorMap;

		private static string _colorStrings;

		public static Dictionary<string, Color> colorMap
		{
			get
			{
				if (_colorMap == null)
				{
					_colorMap = new Dictionary<string, Color>
					{
						{
							"white",
							Color.white
						},
						{
							"black",
							Color.black
						},
						{
							"clear",
							Color.clear
						},
						{
							"transparent",
							new Color(0f, 0f, 0f, 0f)
						},
						{
							"transparentBlack",
							new Color(0f, 0f, 0f, 0f)
						},
						{
							"transparentWhite",
							new Color(1f, 1f, 1f, 0f)
						}
					};
					bool darkSkin = EditorGUIUtility.isProSkin;
					Color blue = (darkSkin ? new Color(0.184f, 0.593f, 1f, 1f) : new Color(0f, 0.301f, 1f, 1f));
					Color green = (darkSkin ? new Color(0.223f, 0.83f, 0.223f, 1f) : new Color(0f, 0.642f, 0f, 1f));
					Color purple = (darkSkin ? new Color(0.74f, 0.46f, 0.73f, 1f) : new Color(0.613f, 0f, 0.588f, 1f));
					Color red = (darkSkin ? new Color(1f, 0.222f, 0.245f, 1f) : new Color(1f, 0.111f, 0.125f, 1f));
					Color orange = (darkSkin ? new Color(0.97f, 0.55f, 0.02f, 1f) : new Color(1f, 0.391f, 0f, 1f));
					(string, Color)[] brightColors = new(string, Color)[10]
					{
						("red", red),
						("yellow", new Color(0.888f, 0.837f, 0.19f, 1f)),
						("green", green),
						("blue", blue),
						("gray", Color.gray),
						("grey", Color.grey),
						("cyan", Color.cyan),
						("magenta", Color.magenta),
						("orange", orange),
						("purple", purple)
					};
					(string, Color)[] array = brightColors;
					for (int i = 0; i < array.Length; i++)
					{
						(string, Color) color = array[i];
						Color col = color.Item2;
						Color.RGBToHSV(color.Item2, out var h, out var s, out var v);
						_colorMap.Add("dark" + color.Item1, Color.HSVToRGB(h, s, v * 0.7f));
						_colorMap.Add(color.Item1, col);
						_colorMap.Add("light" + color.Item1, Color.HSVToRGB(h, s, v + 0.7f));
					}
				}
				return _colorMap;
			}
		}

		private static string colorStrings
		{
			get
			{
				if (_colorStrings == null)
				{
					List<string> colors = new List<string>();
					colors.AddRange(colorMap.Keys);
					colors.Sort();
					StringBuilder sb = new StringBuilder();
					foreach (string colorStr in colors)
					{
						if (sb.Length > 0)
						{
							sb.Append(", ");
						}
						Color color = colorMap[colorStr];
						sb.Append("<color=#");
						sb.Append(ColorUtility.ToHtmlStringRGB(color).ToLower());
						sb.Append("ff>");
						sb.Append(colorStr);
						sb.Append("</color>");
					}
					_colorStrings = sb.ToString();
				}
				return _colorStrings;
			}
		}

		public override string GetPossibleMatchesString(ref ValueResolverContext context)
		{
			if (context.ResultType == typeof(Color) || context.ResultType == typeof(Color?))
			{
				return "rgba(1, 1, 1, 1)\nrgb(1, 1, 1)\n#FFFFFFFF\n#FFFFFF\nOne of these color names (note: transparency not displayed here): " + colorStrings;
			}
			return null;
		}

		public override ValueResolverFunc<TResult> TryCreateResolverFunc<TResult>(ref ValueResolverContext context)
		{
			if (string.IsNullOrWhiteSpace(context.ResolvedString) || (context.ResultType != typeof(Color) && context.ResultType != typeof(Color?)))
			{
				return null;
			}
			string trimmed = context.ResolvedString.Trim();
			if (trimmed.Length == 0)
			{
				return null;
			}
			if (colorMap.TryGetValue(trimmed.ToLower(), out var result))
			{
				return GetResultFunc<TResult>(ref context, result);
			}
			if (trimmed[0] == '#')
			{
				if (trimmed.Length == 7)
				{
					if (!TryParseHex(trimmed[1], trimmed[2], out var r))
					{
						context.ErrorMessage = "Invalid red color hex code '" + trimmed + "'; '" + trimmed.Substring(1, 2) + "' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
						return ValueResolverCreator.GetFailedResolverFunc<TResult>();
					}
					if (!TryParseHex(trimmed[3], trimmed[4], out var g))
					{
						context.ErrorMessage = "Invalid green color hex code '" + trimmed + "'; '" + trimmed.Substring(3, 2) + "' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
						return ValueResolverCreator.GetFailedResolverFunc<TResult>();
					}
					if (!TryParseHex(trimmed[5], trimmed[6], out var b))
					{
						context.ErrorMessage = "Invalid blue color hex code '" + trimmed + "'; '" + trimmed.Substring(5, 2) + "' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
						return ValueResolverCreator.GetFailedResolverFunc<TResult>();
					}
					Color result2 = new Color((float)r / 255f, (float)g / 255f, (float)b / 255f, 1f);
					return GetResultFunc<TResult>(ref context, result2);
				}
				if (trimmed.Length == 9)
				{
					if (!TryParseHex(trimmed[1], trimmed[2], out var r2))
					{
						context.ErrorMessage = "Invalid red color hex code '" + trimmed + "'; '" + trimmed.Substring(1, 2) + "' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
						return ValueResolverCreator.GetFailedResolverFunc<TResult>();
					}
					if (!TryParseHex(trimmed[3], trimmed[4], out var g2))
					{
						context.ErrorMessage = "Invalid green color hex code '" + trimmed + "'; '" + trimmed.Substring(3, 2) + "' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
						return ValueResolverCreator.GetFailedResolverFunc<TResult>();
					}
					if (!TryParseHex(trimmed[5], trimmed[6], out var b2))
					{
						context.ErrorMessage = "Invalid blue color hex code '" + trimmed + "'; '" + trimmed.Substring(5, 2) + "' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
						return ValueResolverCreator.GetFailedResolverFunc<TResult>();
					}
					if (!TryParseHex(trimmed[7], trimmed[8], out var a))
					{
						context.ErrorMessage = "Invalid alpha color hex code '" + trimmed + "'; '" + trimmed.Substring(7, 2) + "' is not a hex number. Valid hex number characters are 0-9, a-f and A-F.";
						return ValueResolverCreator.GetFailedResolverFunc<TResult>();
					}
					Color result3 = new Color((float)r2 / 255f, (float)g2 / 255f, (float)b2 / 255f, (float)a / 255f);
					return GetResultFunc<TResult>(ref context, result3);
				}
				context.ErrorMessage = "Invalid color hex code '" + trimmed + "'; expected 6 or 8 hex characters.";
				return ValueResolverCreator.GetFailedResolverFunc<TResult>();
			}
			if (FastStartsWithIgnoreCase(trimmed, "rgba("))
			{
				if (trimmed[trimmed.Length - 1] != ')')
				{
					context.ErrorMessage = "Expected rgba statement '" + trimmed + "' to end with ')' (for example, 'rgba(1,1,1,1)').";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				string colorsStr = trimmed.Substring(5, trimmed.Length - 6);
				string[] colors = colorsStr.Split(new char[1] { ',' });
				if (colors.Length != 4)
				{
					context.ErrorMessage = "Expected rgba statement '" + trimmed + "' to contain four color components numbers separated by commas (for example, 'rgba(1,1,1,1)').";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				Color result4 = default(Color);
				if (!float.TryParse(colors[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result4.r))
				{
					context.ErrorMessage = "Could not parse red '" + colors[0] + "' to a float value when parsing rgba statement '" + trimmed + "'.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				if (!float.TryParse(colors[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result4.g))
				{
					context.ErrorMessage = "Could not parse green '" + colors[1] + "' to a float value when parsing rgba statement '" + trimmed + "'.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				if (!float.TryParse(colors[2], NumberStyles.Float, CultureInfo.InvariantCulture, out result4.b))
				{
					context.ErrorMessage = "Could not parse blue '" + colors[2] + "' to a float value when parsing rgba statement '" + trimmed + "'.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				if (!float.TryParse(colors[3], NumberStyles.Float, CultureInfo.InvariantCulture, out result4.a))
				{
					context.ErrorMessage = "Could not parse alpha '" + colors[3] + "' to a float value when parsing rgba statement '" + trimmed + "'.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				return GetResultFunc<TResult>(ref context, result4);
			}
			if (FastStartsWithIgnoreCase(trimmed, "rgb("))
			{
				if (trimmed[trimmed.Length - 1] != ')')
				{
					context.ErrorMessage = "Expected rgb statement '" + trimmed + "' to end with ')'. (for example, 'rgba(1,1,1,1)')";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				string colorsStr2 = trimmed.Substring(4, trimmed.Length - 5);
				string[] colors2 = colorsStr2.Split(new char[1] { ',' });
				if (colors2.Length != 3)
				{
					context.ErrorMessage = "Expected rgb statement '" + trimmed + "' to contain three color components numbers separated by commas (for example, 'rgb(1,1,1)').";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				Color result5 = default(Color);
				if (!float.TryParse(colors2[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result5.r))
				{
					context.ErrorMessage = "Could not parse red '" + colors2[0] + "' to a float value when parsing rgb statement '" + trimmed + "'.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				if (!float.TryParse(colors2[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result5.g))
				{
					context.ErrorMessage = "Could not parse green '" + colors2[1] + "' to a float value when parsing rgb statement '" + trimmed + "'.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				if (!float.TryParse(colors2[2], NumberStyles.Float, CultureInfo.InvariantCulture, out result5.b))
				{
					context.ErrorMessage = "Could not parse blue '" + colors2[2] + "' to a float value when parsing rgb statement '" + trimmed + "'.";
					return ValueResolverCreator.GetFailedResolverFunc<TResult>();
				}
				result5.a = 1f;
				return GetResultFunc<TResult>(ref context, result5);
			}
			return null;
		}

		private static ValueResolverFunc<TResult> GetResultFunc<TResult>(ref ValueResolverContext context, Color result)
		{
			if (context.ResultType == typeof(Color))
			{
				return (ValueResolverFunc<TResult>)(object)CreateColorResultFunc(result);
			}
			return (ValueResolverFunc<TResult>)(object)CreateNullableColorResultFunc(result);
		}

		private static bool TryParseHex(char a, char b, out uint result)
		{
			if (TryParseSingleChar(a, 16u, out var aResult) && TryParseSingleChar(b, 1u, out var bResult))
			{
				result = aResult + bResult;
				return true;
			}
			result = 0u;
			return false;
		}

		private static bool TryParseSingleChar(char c, uint multiplier, out uint result)
		{
			if (c >= '0' && c <= '9')
			{
				result = (uint)(c - 48) * multiplier;
				return true;
			}
			if (c >= 'A' && c <= 'F')
			{
				result = (uint)(c - 65 + 10) * multiplier;
				return true;
			}
			if (c >= 'a' && c <= 'f')
			{
				result = (uint)(c - 97 + 10) * multiplier;
				return true;
			}
			result = 0u;
			return false;
		}

		private static ValueResolverFunc<Color> CreateColorResultFunc(Color color)
		{
			return delegate
			{
				return color;
			};
		}

		private static ValueResolverFunc<Color?> CreateNullableColorResultFunc(Color? color)
		{
			return delegate
			{
				return color;
			};
		}

		private static bool FastStartsWithIgnoreCase(string str, string startsWithLower)
		{
			if (startsWithLower.Length > str.Length)
			{
				return false;
			}
			for (int i = 0; i < startsWithLower.Length; i++)
			{
				if (char.ToLower(str[i]) != startsWithLower[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}
