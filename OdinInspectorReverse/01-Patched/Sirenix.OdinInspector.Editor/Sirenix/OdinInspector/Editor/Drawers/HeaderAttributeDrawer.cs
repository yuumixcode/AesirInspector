using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:UnityEngine.HeaderAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.HeaderAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.TitleAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.HideLabelAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.LabelTextAttribute" />
	/// <seealso cref="T:UnityEngine.SpaceAttribute" />
	[DrawerPriority(1.0, 0.0, 0.0)]
	public sealed class HeaderAttributeDrawer : OdinAttributeDrawer<HeaderAttribute>
	{
		private ValueResolver<string> textResolver;

		protected override void Initialize()
		{
			if (base.Property.Parent != null && base.Property.Parent.ChildResolver is ICollectionResolver)
			{
				base.SkipWhenDrawing = true;
			}
			else
			{
				textResolver = ValueResolver.GetForString(base.Property, base.Attribute.header);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (base.Property != base.Property.Tree.GetRootProperty(0))
			{
				EditorGUILayout.Space();
			}
			if (textResolver.HasError)
			{
				SirenixEditorGUI.MessageBox(textResolver.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
			}
			else
			{
				EditorGUILayout.LabelField(textResolver.GetValue(), EditorStyles.boldLabel);
			}
			CallNextDrawer(label);
		}
	}
}
