using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class FontUtility
	{
		private static List<string> _proportionalFonts;

		private static List<string> _monospaceFonts;

		private static bool initialized;

		public static IReadOnlyList<string> ProportionalFonts
		{
			get
			{
				if (!initialized)
				{
					LoadFonts();
				}
				return _proportionalFonts;
			}
		}

		public static IReadOnlyList<string> MonospaceFonts
		{
			get
			{
				if (!initialized)
				{
					LoadFonts();
				}
				return _monospaceFonts;
			}
		}

		public static Font GetFont(string fontName)
		{
			return Font.CreateDynamicFontFromOSFont(fontName, 12);
		}

		public static Font GetMonospaceFont()
		{
			if (MonospaceFonts.Count != 0)
			{
				return Font.CreateDynamicFontFromOSFont(MonospaceFonts[0], 12);
			}
			return null;
		}

		private static void LoadFonts()
		{
			_proportionalFonts = new List<string>();
			_monospaceFonts = new List<string>();
			string[] oSInstalledFontNames = Font.GetOSInstalledFontNames();
			foreach (string fontName in oSInstalledFontNames)
			{
				if (fontName == ".LastResort")
				{
					continue;
				}
				Font font = Font.CreateDynamicFontFromOSFont(fontName, 12);
				if (!(font == null))
				{
					if (HasUniformAdvance(font))
					{
						_monospaceFonts.Add(fontName);
					}
					else
					{
						_proportionalFonts.Add(fontName);
					}
					Object.DestroyImmediate(font);
				}
			}
			initialized = true;
		}

		private static bool HasUniformAdvance(Font font)
		{
			font.RequestCharactersInTexture("[](){}<>`'\",.:;_-—~/\\|!+*=?#%^&@0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", 12, FontStyle.Normal);
			int advance = -1;
			string text = "[](){}<>`'\",.:;_-—~/\\|!+*=?#%^&@0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			foreach (char ch in text)
			{
				if (font.GetCharacterInfo(ch, out var characterInfo, 12, FontStyle.Normal))
				{
					if (advance == -1)
					{
						advance = characterInfo.advance;
					}
					else if (advance != characterInfo.advance)
					{
						return false;
					}
				}
			}
			return true;
		}
	}
}
