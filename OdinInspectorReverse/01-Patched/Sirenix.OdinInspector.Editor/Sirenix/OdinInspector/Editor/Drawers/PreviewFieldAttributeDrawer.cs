using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.PreviewFieldAttribute" /> as a square ObjectField which renders a preview for UnityEngine.Object types.
	/// This object field also adds support for drag and drop, dragging an object to another square object field, swaps the values.
	/// If you hold down control while letting go it will replace the value, And you can control + click the object field to quickly delete the value it holds.
	/// </summary>
	[AllowGUIEnabledForReadonly]
	public sealed class PreviewFieldAttributeDrawer<T> : OdinAttributeDrawer<PreviewFieldAttribute, T> where T : Object
	{
		private ValueResolver<Object> previewResolver;

		private bool allowSceneObjects;

		protected override void Initialize()
		{
			previewResolver = ValueResolver.Get<Object>(base.Property, base.Attribute.PreviewGetter);
			allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(base.Property);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (base.Attribute.PreviewGetterHasValue && previewResolver.HasError)
			{
				previewResolver.DrawError();
				CallNextDrawer(label);
				return;
			}
			EditorGUI.BeginChangeCheck();
			Sirenix.Utilities.Editor.ObjectFieldAlignment alignment = ((!base.Attribute.AlignmentHasValue) ? GlobalConfig<GeneralDrawerConfig>.Instance.SquareUnityObjectAlignment : ((Sirenix.Utilities.Editor.ObjectFieldAlignment)base.Attribute.Alignment));
			float previewHeight = ((base.Attribute.Height == 0f) ? GlobalConfig<GeneralDrawerConfig>.Instance.SquareUnityObjectFieldHeight : base.Attribute.Height);
			Texture previewTexture;
			if (base.Attribute.PreviewGetterHasValue)
			{
				Object resolvedPreview = previewResolver.GetValue();
				previewTexture = ((resolvedPreview == null) ? null : GUIHelper.GetPreviewTexture(resolvedPreview));
			}
			else
			{
				Object value = base.ValueEntry.WeakSmartValue as Object;
				previewTexture = ((value == null) ? null : GUIHelper.GetPreviewTexture(value));
			}
			FilterMode lastFilterMode = FilterMode.Bilinear;
			if (previewTexture != null)
			{
				lastFilterMode = previewTexture.filterMode;
				previewTexture.filterMode = base.Attribute.FilterMode;
			}
			try
			{
				Rect rect = OdinInternalEditorFields.UnityPreviewFieldArgs.GetDefaultRect(label, previewHeight);
				OdinInternalEditorFields.UnityPreviewFieldArgs previewArgs = OdinInternalEditorFields.UnityPreviewFieldArgs.CreateForProperty(base.Property, rect, alignment, 0, label);
				previewArgs.Preview = previewTexture;
				previewArgs.AllowSceneObjects = allowSceneObjects;
				if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldUnityPreviewField)
				{
					base.ValueEntry.WeakSmartValue = OdinInternalEditorFields.UnityPreviewObjectField(in previewArgs);
				}
				else
				{
					OdinInternalEditorFields.UnityPreviewObjectField(in previewArgs);
				}
			}
			finally
			{
				if (previewTexture != null)
				{
					previewTexture.filterMode = lastFilterMode;
				}
			}
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.Values.ForceMarkDirty();
			}
		}
	}
}
