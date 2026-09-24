using System.Globalization;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// <para>The style settings used by <see cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />.</para>
	/// <para>
	/// A nice trick to style your menu is to add the tree.DefaultMenuStyle to the tree itself,
	/// and style it live. Once you are happy, you can hit the Copy CSharp Snippet button,
	/// remove the style from the menu tree, and paste the style directly into your code.
	/// </para>
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTree" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuItem" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeSelection" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuTreeExtensions" />
	/// <seealso cref="T:Sirenix.OdinInspector.Editor.OdinMenuEditorWindow" />
	public class OdinMenuStyle
	{
		private GUIStyle defaultLabelStyle;

		private GUIStyle selectedLabelStyle;

		/// <summary>
		/// The height of the menu item.
		/// </summary>
		[BoxGroup("General", true, false, 0f)]
		public int Height = 30;

		/// <summary>
		/// The global offset of the menu item content
		/// </summary>
		[BoxGroup("General", true, false, 0f)]
		public float Offset = 16f;

		/// <summary>
		/// The vertical offset of the menu item label
		/// </summary>
		[BoxGroup("General", true, false, 0f)]
		public float LabelVerticalOffset;

		/// <summary>
		/// The number of pixels to indent per level indent level.
		/// </summary>
		[BoxGroup("General", true, false, 0f)]
		public float IndentAmount = 15f;

		/// <summary>
		/// The size of the icon.
		/// </summary>
		[BoxGroup("Icons", true, false, 0f)]
		public float IconSize = 16f;

		/// <summary>
		/// The size of the icon.
		/// </summary>
		[BoxGroup("Icons", true, false, 0f)]
		public float IconOffset;

		/// <summary>
		/// The transparency of icons when the menu item is not selected.
		/// </summary>
		[Range(0f, 1f)]
		[BoxGroup("Icons", true, false, 0f)]
		public float NotSelectedIconAlpha = 0.85f;

		/// <summary>
		/// The padding between the icon and other content.
		/// </summary>
		[BoxGroup("Icons", true, false, 0f)]
		public float IconPadding = 3f;

		/// <summary>
		/// Whether to draw the a foldout triangle for menu items with children.
		/// </summary>
		[BoxGroup("Triangle", true, false, 0f)]
		public bool DrawFoldoutTriangle = true;

		/// <summary>
		/// The size of the foldout triangle icon.
		/// </summary>
		[BoxGroup("Triangle", true, false, 0f)]
		public float TriangleSize = 17f;

		/// <summary>
		/// The padding between the foldout triangle icon and other content.
		/// </summary>
		[BoxGroup("Triangle", true, false, 0f)]
		public float TrianglePadding = 8f;

		/// <summary>
		/// Whether or not to align the triangle left or right of the content.
		/// If right, then the icon is pushed all the way to the right at a fixed position ignoring the indent level.
		/// </summary>
		[BoxGroup("Triangle", true, false, 0f)]
		public bool AlignTriangleLeft;

		/// <summary>
		/// Whether to draw borders between menu items.
		/// </summary>
		[BoxGroup("Borders", true, false, 0f)]
		public bool Borders = true;

		/// <summary>
		/// The horizontal border padding.
		/// </summary>
		[BoxGroup("Borders", true, false, 0f)]
		[EnableIf("Borders")]
		public float BorderPadding = 13f;

		/// <summary>
		/// The border alpha.
		/// </summary>
		[Range(0f, 1f)]
		[EnableIf("Borders")]
		[BoxGroup("Borders", true, false, 0f)]
		public float BorderAlpha = 0.5f;

		/// <summary>
		/// The background color for when a menu item is selected.
		/// </summary>
		[BoxGroup("Colors", true, false, 0f)]
		public Color SelectedColorDarkSkin = SirenixGUIStyles.DefaultSelectedMenuTreeColorDarkSkin;

		/// <summary>
		/// The background color for when a menu item is selected.
		/// </summary>
		[BoxGroup("Colors", true, false, 0f)]
		public Color SelectedInactiveColorDarkSkin = SirenixGUIStyles.DefaultSelectedInactiveMenuTreeColorDarkSkin;

		/// <summary>
		/// The background color for when a menu item is selected.
		/// </summary>
		[BoxGroup("Colors", true, false, 0f)]
		public Color SelectedColorLightSkin = SirenixGUIStyles.DefaultSelectedMenuTreeColorLightSkin;

		/// <summary>
		/// The background color for when a menu item is selected.
		/// </summary>
		[BoxGroup("Colors", true, false, 0f)]
		public Color SelectedInactiveColorLightSkin = SirenixGUIStyles.DefaultSelectedInactiveMenuTreeColorLightSkin;

		/// <summary>
		/// Gets or sets the default selected style.
		/// </summary>
		public GUIStyle DefaultLabelStyle
		{
			get
			{
				if (defaultLabelStyle == null)
				{
					defaultLabelStyle = SirenixGUIStyles.Label;
				}
				return defaultLabelStyle;
			}
			set
			{
				defaultLabelStyle = value;
			}
		}

		/// <summary>
		/// Gets or sets the selected label style.
		/// </summary>
		public GUIStyle SelectedLabelStyle
		{
			get
			{
				if (selectedLabelStyle == null)
				{
					selectedLabelStyle = SirenixGUIStyles.WhiteLabel;
				}
				return selectedLabelStyle;
			}
			set
			{
				selectedLabelStyle = value;
			}
		}

		/// <summary>
		/// The background color for when a menu item is selected.
		/// </summary>
		public Color SelectedColor
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return SelectedColorLightSkin;
				}
				return SelectedColorDarkSkin;
			}
		}

		/// <summary>
		/// The background color for when a menu item is selected.
		/// </summary>
		public Color SelectedInactiveColor
		{
			get
			{
				if (!EditorGUIUtility.isProSkin)
				{
					return SelectedInactiveColorLightSkin;
				}
				return SelectedInactiveColorDarkSkin;
			}
		}

		/// <summary>
		/// Creates and returns an instance of a menu style that makes it look like Unity's project window.
		/// </summary>
		public static OdinMenuStyle TreeViewStyle => new OdinMenuStyle
		{
			BorderPadding = 0f,
			AlignTriangleLeft = true,
			TriangleSize = 16f,
			TrianglePadding = 0f,
			Offset = 20f,
			Height = 23,
			IconPadding = 0f,
			BorderAlpha = 0.323f
		};

		/// <summary>
		/// Sets the height of the menu item.
		/// </summary>
		public OdinMenuStyle SetHeight(int value)
		{
			Height = value;
			return this;
		}

		/// <summary>
		/// Sets the global offset of the menu item content
		/// </summary>
		public OdinMenuStyle SetOffset(float value)
		{
			Offset = value;
			return this;
		}

		/// <summary>
		/// Sets the number of pixels to indent per level indent level.
		/// </summary>
		public OdinMenuStyle SetIndentAmount(float value)
		{
			IndentAmount = value;
			return this;
		}

		/// <summary>
		/// Sets the size of the icon.
		/// </summary>
		public OdinMenuStyle SetIconSize(float value)
		{
			IconSize = value;
			return this;
		}

		/// <summary>
		/// Sets the size of the icon.
		/// </summary>
		public OdinMenuStyle SetIconOffset(float value)
		{
			IconOffset = value;
			return this;
		}

		/// <summary>
		/// Sets the transparency of icons when the menu item is not selected.
		/// </summary>
		public OdinMenuStyle SetNotSelectedIconAlpha(float value)
		{
			NotSelectedIconAlpha = value;
			return this;
		}

		/// <summary>
		/// Sets the padding between the icon and other content.
		/// </summary>
		public OdinMenuStyle SetIconPadding(float value)
		{
			IconPadding = value;
			return this;
		}

		/// <summary>
		/// Sets whether to draw the a foldout triangle for menu items with children.
		/// </summary>
		public OdinMenuStyle SetDrawFoldoutTriangle(bool value)
		{
			DrawFoldoutTriangle = value;
			return this;
		}

		/// <summary>
		/// Sets the size of the foldout triangle icon.
		/// </summary>
		public OdinMenuStyle SetTriangleSize(float value)
		{
			TriangleSize = value;
			return this;
		}

		/// <summary>
		/// Sets the padding between the foldout triangle icon and other content.
		/// </summary>
		public OdinMenuStyle SetTrianglePadding(float value)
		{
			TrianglePadding = value;
			return this;
		}

		/// <summary>
		/// Sets whether or not to align the triangle left or right of the content.
		/// If right, then the icon is pushed all the way to the right at a fixed position ignoring the indent level.
		/// </summary>
		public OdinMenuStyle SetAlignTriangleLeft(bool value)
		{
			AlignTriangleLeft = value;
			return this;
		}

		/// <summary>
		/// Sets whether to draw borders between menu items.
		/// </summary>
		public OdinMenuStyle SetBorders(bool value)
		{
			Borders = value;
			return this;
		}

		/// <summary>
		/// Sets the border alpha.
		/// </summary>
		public OdinMenuStyle SetBorderPadding(float value)
		{
			BorderPadding = value;
			return this;
		}

		/// <summary>
		/// Sets the border alpha.
		/// </summary>
		public OdinMenuStyle SetBorderAlpha(float value)
		{
			BorderAlpha = value;
			return this;
		}

		/// <summary>
		/// Sets the background color for when a menu item is selected.
		/// </summary>
		public OdinMenuStyle SetSelectedColorDarkSkin(Color value)
		{
			SelectedColorDarkSkin = value;
			return this;
		}

		/// <summary>
		/// Sets the background color for when a menu item is selected.
		/// </summary>
		public OdinMenuStyle SetSelectedColorLightSkin(Color value)
		{
			SelectedColorLightSkin = value;
			return this;
		}

		public OdinMenuStyle Clone()
		{
			return new OdinMenuStyle
			{
				Height = Height,
				Offset = Offset,
				IndentAmount = IndentAmount,
				IconSize = IconSize,
				IconOffset = IconOffset,
				NotSelectedIconAlpha = NotSelectedIconAlpha,
				IconPadding = IconPadding,
				TriangleSize = TriangleSize,
				TrianglePadding = TrianglePadding,
				AlignTriangleLeft = AlignTriangleLeft,
				Borders = Borders,
				BorderPadding = BorderPadding,
				BorderAlpha = BorderAlpha,
				SelectedColorDarkSkin = SelectedColorDarkSkin,
				SelectedColorLightSkin = SelectedColorLightSkin
			};
		}

		[Button("Copy C# Snippet", ButtonSizes.Large)]
		private void CopyCSharpSnippet()
		{
			Clipboard.Copy("new OdinMenuStyle()\r\n{\r\n    Height = " + Height + ",\r\n    Offset = " + Offset.ToString("F2", CultureInfo.InvariantCulture) + "f,\r\n    IndentAmount = " + IndentAmount.ToString("F2", CultureInfo.InvariantCulture) + "f,\r\n    IconSize = " + IconSize.ToString("F2", CultureInfo.InvariantCulture) + "f,\r\n    IconOffset = " + IconOffset.ToString("F2", CultureInfo.InvariantCulture) + "f,\r\n    NotSelectedIconAlpha = " + NotSelectedIconAlpha.ToString("F2", CultureInfo.InvariantCulture) + "f,\r\n    IconPadding = " + IconPadding.ToString("F2", CultureInfo.InvariantCulture) + "f,\r\n    TriangleSize = " + TriangleSize.ToString("F2", CultureInfo.InvariantCulture) + "f,\r\n    TrianglePadding = " + TrianglePadding.ToString("F2", CultureInfo.InvariantCulture) + "f,\r\n    AlignTriangleLeft = " + AlignTriangleLeft.ToString().ToLower() + ",\r\n    Borders = " + Borders.ToString().ToLower() + ",\r\n    BorderPadding = " + BorderPadding.ToString("F2", CultureInfo.InvariantCulture) + "f,\r\n    BorderAlpha = " + BorderAlpha.ToString("F2", CultureInfo.InvariantCulture) + "f,\r\n    SelectedColorDarkSkin = new Color(" + SelectedColorDarkSkin.r.ToString("F3", CultureInfo.InvariantCulture) + "f, " + SelectedColorDarkSkin.g.ToString("F3", CultureInfo.InvariantCulture) + "f, " + SelectedColorDarkSkin.b.ToString("F3", CultureInfo.InvariantCulture) + "f, " + SelectedColorDarkSkin.a.ToString("F3", CultureInfo.InvariantCulture) + "f),\r\n    SelectedColorLightSkin = new Color(" + SelectedColorLightSkin.r.ToString("F3", CultureInfo.InvariantCulture) + "f, " + SelectedColorLightSkin.g.ToString("F3", CultureInfo.InvariantCulture) + "f, " + SelectedColorLightSkin.b.ToString("F3", CultureInfo.InvariantCulture) + "f, " + SelectedColorLightSkin.a.ToString("F3", CultureInfo.InvariantCulture) + "f)};");
		}
	}
}
