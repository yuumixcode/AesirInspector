using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws all properties grouped together with the <see cref="T:Sirenix.OdinInspector.ButtonGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.ButtonGroupAttribute" />
	public class ButtonGroupAttributeDrawer : OdinGroupDrawer<ButtonGroupAttribute>
	{
		private int buttonHeight;

		private float[] buttonAlignment;

		private IconAlignment[] buttonIconAlignment;

		private bool[] stretch;

		protected override void Initialize()
		{
			int childCount = base.Property.Children.Count;
			buttonHeight = base.Attribute.ButtonHeight;
			buttonAlignment = new float[childCount];
			buttonIconAlignment = new IconAlignment[childCount];
			stretch = new bool[childCount];
			for (int i = 0; i < childCount; i++)
			{
				ButtonAttribute button = base.Property.Children[i].GetAttribute<ButtonAttribute>();
				bool hasButton = button != null;
				buttonHeight = Mathf.Max(buttonHeight, hasButton ? button.ButtonHeight : 0);
				buttonAlignment[i] = ((hasButton && button.HasDefinedButtonAlignment) ? button.ButtonAlignment : (base.Attribute.HasDefinedButtonAlignment ? ((float)base.Attribute.ButtonAlignment) : GlobalConfig<GeneralDrawerConfig>.Instance.ButtonAlignment));
				buttonIconAlignment[i] = ((hasButton && button.HasDefinedButtonIconAlignment) ? button.IconAlignment : (base.Attribute.HasDefinedButtonIconAlignment ? base.Attribute.IconAlignment : GlobalConfig<GeneralDrawerConfig>.Instance.ButtonIconAlignment));
				stretch[i] = ((hasButton && button.HasDefinedStretch) ? button.Stretch : (base.Attribute.HasDefinedStretch ? base.Attribute.Stretch : GlobalConfig<GeneralDrawerConfig>.Instance.StretchButtons));
			}
			if (buttonHeight == 0)
			{
				buttonHeight = GlobalConfig<GeneralDrawerConfig>.Instance.ButtonHeight;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty property = base.Property;
			SirenixEditorGUI.BeginIndentedHorizontal();
			for (int i = 0; i < property.Children.Count; i++)
			{
				GUIStyle style = null;
				if (property.Children.Count != 1)
				{
					style = ((i == 0) ? SirenixGUIStyles.ButtonLeft : ((i != property.Children.Count - 1) ? SirenixGUIStyles.ButtonMid : SirenixGUIStyles.ButtonRight));
				}
				InspectorProperty child = property.Children[i];
				if (style != null)
				{
					child.Context.GetGlobal("ButtonStyle", style).Value = style;
				}
				child.Context.GetGlobal("ButtonHeight", buttonHeight).Value = buttonHeight;
				child.Context.GetGlobal("ButtonAlignment", buttonAlignment[i]).Value = buttonAlignment[i];
				child.Context.GetGlobal("IconAlignment", buttonIconAlignment[i]).Value = buttonIconAlignment[i];
				child.Context.GetGlobal("StretchButton", stretch[i]).Value = stretch[i];
				child.Context.GetGlobal("DrawnByGroup", defaultValue: false).Value = true;
				DefaultMethodDrawer.DontDrawMethodParameters = true;
				child.Draw(child.Label);
				DefaultMethodDrawer.DontDrawMethodParameters = false;
			}
			SirenixEditorGUI.EndIndentedHorizontal();
		}
	}
}
