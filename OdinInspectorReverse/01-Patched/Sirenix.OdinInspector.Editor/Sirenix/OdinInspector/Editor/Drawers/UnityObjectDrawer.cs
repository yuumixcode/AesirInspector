using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Unity object drawer.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.25)]
	public sealed class UnityObjectDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : Object
	{
		private bool drawAsPreview;

		private PolymorphicDrawerSettingsAttribute polymorphicDrawerSettings;

		private bool allowSceneObjects;

		private bool ShowBaseType
		{
			get
			{
				if (polymorphicDrawerSettings == null)
				{
					return true;
				}
				if (polymorphicDrawerSettings.ShowBaseTypeIsSet)
				{
					return polymorphicDrawerSettings.ShowBaseType;
				}
				return true;
			}
		}

		private bool ReadOnlyIfNotNullReference => polymorphicDrawerSettings?.ReadOnlyIfNotNullReference ?? false;

		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			return !property.IsTreeRoot;
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		protected override void Initialize()
		{
			drawAsPreview = false;
			GeneralDrawerConfig.UnityObjectType flags = GlobalConfig<GeneralDrawerConfig>.Instance.SquareUnityObjectEnableFor;
			drawAsPreview = flags != 0 && (((flags & GeneralDrawerConfig.UnityObjectType.Components) != 0 && typeof(Component).IsAssignableFrom(typeof(T))) || ((flags & GeneralDrawerConfig.UnityObjectType.GameObjects) != 0 && typeof(GameObject).IsAssignableFrom(typeof(T))) || ((flags & GeneralDrawerConfig.UnityObjectType.Materials) != 0 && typeof(Material).IsAssignableFrom(typeof(T))) || ((flags & GeneralDrawerConfig.UnityObjectType.Sprites) != 0 && typeof(Sprite).IsAssignableFrom(typeof(T))) || ((flags & GeneralDrawerConfig.UnityObjectType.Textures) != 0 && typeof(Texture).IsAssignableFrom(typeof(T))));
			if (!drawAsPreview && (flags & GeneralDrawerConfig.UnityObjectType.Others) != 0 && !typeof(Component).IsAssignableFrom(typeof(T)) && !typeof(GameObject).IsAssignableFrom(typeof(T)) && !typeof(Material).IsAssignableFrom(typeof(T)) && !typeof(Sprite).IsAssignableFrom(typeof(T)) && !typeof(Texture).IsAssignableFrom(typeof(T)))
			{
				drawAsPreview = true;
			}
			polymorphicDrawerSettings = base.Property.GetAttribute<PolymorphicDrawerSettingsAttribute>();
			allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(base.Property);
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			bool isPolymorphic = entry.BaseValueType == typeof(object) || !typeof(Object).IsAssignableFrom(entry.BaseValueType) || entry.BaseValueType.IsInterface;
			if (!drawAsPreview)
			{
				if (isPolymorphic)
				{
					Rect position = EditorGUILayout.GetControlRect();
					bool isReadOnly = ReadOnlyIfNotNullReference && base.ValueEntry.WeakSmartValue != null;
					OdinInternalEditorFields.PolymorphicFieldArgs polymorphicArgs = OdinInternalEditorFields.PolymorphicFieldArgs.CreateForProperty(base.Property, position, 0);
					polymorphicArgs.AllowSceneObjects = allowSceneObjects;
					polymorphicArgs.ReadOnly = isReadOnly;
					polymorphicArgs.ShowBaseType = ShowBaseType;
					if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldPolymorphicField)
					{
						if (label != null)
						{
							position = EditorGUI.PrefixLabel(position, label);
						}
						EditorGUI.BeginChangeCheck();
						bool prev = EditorGUI.showMixedValue;
						if (base.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
						{
							EditorGUI.showMixedValue = true;
						}
						polymorphicArgs.Rect = position;
						object newValue = OdinInternalEditorFields.PolymorphicObjectField(in polymorphicArgs);
						EditorGUI.showMixedValue = prev;
						if (!EditorGUI.EndChangeCheck())
						{
							return;
						}
						base.ValueEntry.Property.Tree.DelayActionUntilRepaint(delegate
						{
							base.ValueEntry.WeakValues[0] = newValue;
							for (int i = 1; i < base.ValueEntry.ValueCount; i++)
							{
								base.ValueEntry.WeakValues[i] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
							}
						});
					}
					else
					{
						polymorphicArgs.Label = label;
						OdinInternalEditorFields.PolymorphicObjectField(in polymorphicArgs);
					}
				}
				else
				{
					OdinInternalEditorFields.UnityObjectFieldArgs drawArgs = OdinInternalEditorFields.UnityObjectFieldArgs.CreateForProperty(base.Property, EditorGUILayout.GetControlRect(), allowSceneObjects, label);
					if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldUnityObjectField)
					{
						base.ValueEntry.WeakSmartValue = OdinInternalEditorFields.UnityObjectField(in drawArgs);
					}
					else
					{
						OdinInternalEditorFields.UnityObjectField(in drawArgs);
					}
				}
			}
			else
			{
				Rect rect = OdinInternalEditorFields.UnityPreviewFieldArgs.GetDefaultRect(label, GlobalConfig<GeneralDrawerConfig>.Instance.SquareUnityObjectFieldHeight);
				OdinInternalEditorFields.UnityPreviewFieldArgs previewArgs = OdinInternalEditorFields.UnityPreviewFieldArgs.CreateForProperty(base.Property, rect, GlobalConfig<GeneralDrawerConfig>.Instance.SquareUnityObjectAlignment, 0, label);
				previewArgs.AllowSceneObjects = allowSceneObjects;
				if (GlobalConfig<GeneralDrawerConfig>.Instance.useOldUnityPreviewField)
				{
					entry.WeakSmartValue = OdinInternalEditorFields.UnityPreviewObjectField(in previewArgs);
				}
				else
				{
					OdinInternalEditorFields.UnityPreviewObjectField(in previewArgs);
				}
			}
		}

		void IDefinesGenericMenuItems.PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			Object unityObj = property.ValueEntry.WeakSmartValue as Object;
			if ((bool)unityObj)
			{
				genericMenu.AddItem(new GUIContent("Open in new inspector"), on: false, delegate
				{
					GUIHelper.OpenInspectorWindow(unityObj);
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Open in new inspector"));
			}
		}
	}
}
