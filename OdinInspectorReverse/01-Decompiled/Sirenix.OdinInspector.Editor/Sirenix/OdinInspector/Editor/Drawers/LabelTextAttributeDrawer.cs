using System;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.LabelTextAttribute" />.
	/// Creates a new GUIContent, with the provided label text, before calling further down in the drawer chain.
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.LabelTextAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.HideLabelAttribute" />
	/// <seealso cref="T:UnityEngine.TooltipAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.LabelWidthAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.TitleAttribute" />
	/// <seealso cref="T:UnityEngine.HeaderAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.GUIColorAttribute" />
	[DrawerPriority(DrawerPriorityLevel.SuperPriority)]
	public sealed class LabelTextAttributeDrawer : OdinAttributeDrawer<LabelTextAttribute>, IDisposable
	{
		public Texture2D IconTexture;

		private ValueResolver<string> textProvider;

		private ValueResolver<Color> iconColorResolver;

		private GUIContent overrideLabel;

		protected override void Initialize()
		{
			textProvider = ValueResolver.GetForString(base.Property, base.Attribute.Text);
			iconColorResolver = ValueResolver.Get(base.Property, base.Attribute.IconColor, EditorStyles.label.normal.textColor);
			overrideLabel = new GUIContent();
			if (base.Attribute.Icon != SdfIconType.None)
			{
				Color iconColor = iconColorResolver.GetValue();
				IconTexture = SdfIcons.CreateTransparentIconTexture(base.Attribute.Icon, iconColor, 16, 16, 0);
			}
		}

		/// <summary>
		/// Draws the attribute.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (textProvider.HasError)
			{
				SirenixEditorGUI.MessageBox(textProvider.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				CallNextDrawer(label);
				return;
			}
			if (iconColorResolver.HasError)
			{
				SirenixEditorGUI.MessageBox(iconColorResolver.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				CallNextDrawer(label);
				return;
			}
			string str = textProvider.GetValue();
			GUIContent useLabel;
			if (str == null && base.Attribute.Icon == SdfIconType.None)
			{
				useLabel = label;
			}
			else
			{
				string lbl = str ?? label?.text ?? "";
				if (base.Attribute.NicifyText)
				{
					lbl = ObjectNames.NicifyVariableName(lbl);
				}
				overrideLabel.text = lbl;
				useLabel = overrideLabel;
				if (base.Attribute.Icon != SdfIconType.None)
				{
					useLabel.image = IconTexture;
				}
			}
			CallNextDrawer(useLabel);
		}

		public void Dispose()
		{
			if (IconTexture != null)
			{
				UnityEngine.Object.DestroyImmediate(IconTexture);
				IconTexture = null;
			}
		}
	}
}
