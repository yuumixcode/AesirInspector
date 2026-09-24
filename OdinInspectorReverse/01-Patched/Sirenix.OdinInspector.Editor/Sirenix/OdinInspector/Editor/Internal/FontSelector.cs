using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class FontSelector : OdinSelector<string>
	{
		private readonly Action<string> onFontSelected;

		private readonly Func<int> getFontSize;

		private readonly Action<int> setFontSize;

		private readonly IEnumerable<string> monospaceFonts;

		private readonly IEnumerable<string> proportionalFonts;

		public FontSelector(IEnumerable<string> monospaceFonts, IEnumerable<string> proportionalFonts, Action<string> onFontSelected, Func<int> getFontSize, Action<int> setFontSize)
		{
			this.monospaceFonts = monospaceFonts;
			this.proportionalFonts = proportionalFonts;
			this.onFontSelected = onFontSelected;
			base.SelectionChanged += delegate(IEnumerable<string> selectedFonts)
			{
				string text = selectedFonts.FirstOrDefault();
				if (text != null)
				{
					onFontSelected(text);
				}
			};
			this.getFontSize = getFontSize;
			this.setFontSize = setFontSize;
		}

		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			tree.AddRange(monospaceFonts, (string fontName) => "Monospace/" + fontName);
			tree.AddRange(proportionalFonts, (string fontName) => "Proportional/" + fontName);
		}

		protected override void DrawSelectionTree()
		{
			base.DrawSelectionTree();
			EditorGUI.BeginChangeCheck();
			int newSize = EditorGUILayout.IntSlider("Font Size", getFontSize(), 10, 50);
			if (EditorGUI.EndChangeCheck())
			{
				setFontSize(newSize);
			}
		}
	}
}
