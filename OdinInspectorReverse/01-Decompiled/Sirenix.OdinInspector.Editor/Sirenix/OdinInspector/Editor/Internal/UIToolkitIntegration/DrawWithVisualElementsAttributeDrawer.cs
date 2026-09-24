using System;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration
{
	public class DrawWithVisualElementsAttributeDrawer<T> : OdinAttributeDrawer<DrawWithVisualElementsAttribute, T>, IDisposable, IUnityPropertyFieldDrawer
	{
		private OdinImGuiElement element;

		public bool WillDrawPropertyField => element != null;

		protected override bool CanDrawAttributeValueProperty(InspectorProperty property)
		{
			if (property.GetAttribute<DrawWithVisualElementsAttribute>().DrawCollectionWithImGUI && property.ChildResolver is ICollectionResolver)
			{
				return false;
			}
			return true;
		}

		protected override void Initialize()
		{
			if (!ImguiElementUtils.IsSupported)
			{
				return;
			}
			FieldInfo backingField;
			SerializedProperty unityProperty = base.Property.Tree.GetUnityPropertyForPath(base.Property.Path, out backingField);
			if (unityProperty == null)
			{
				return;
			}
			element = ImguiElementUtils.CreatePropertyFieldElement(unityProperty, delegate(SerializedProperty prop)
			{
				if (base.ValueEntry.IsEditable && prop.serializedObject.targetObject is EmittedScriptableObject<T>)
				{
					UnityEngine.Object[] targetObjects = prop.serializedObject.targetObjects;
					for (int i = 0; i < targetObjects.Length; i++)
					{
						EmittedScriptableObject<T> emittedScriptableObject = (EmittedScriptableObject<T>)targetObjects[i];
						base.ValueEntry.Values[i] = emittedScriptableObject.GetValue();
					}
					base.ValueEntry.Values.ForceMarkDirty();
				}
			});
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (!ImguiElementUtils.IsSupported)
			{
				SirenixEditorGUI.MessageBox("Unable to draw visual element for this property since UIToolkit is not supported by Odin in this version of Unity (requires Unity 2020.2+).", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				CallNextDrawer(label);
				return;
			}
			if (element == null)
			{
				string path = base.Property.UnityPropertyPath;
				SirenixEditorGUI.MessageBox("Unable to draw visual element for " + path + " of type " + typeof(T).GetNiceFullName() + " because no Unity SerializedProperty existed for path and no property wrapper could be emitted.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			FieldInfo fieldInfo;
			SerializedProperty unityProperty = base.Property.Tree.GetUnityPropertyForPath(base.Property.Path, out fieldInfo);
			if (unityProperty == null)
			{
				SirenixEditorGUI.MessageBox("Could not get a Unity SerializedProperty for the property '" + base.Property.NiceName + "' of type '" + base.Property.ValueEntry.TypeOfValue.GetNiceName() + "' at path '" + base.Property.Path + "'.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T> && !unityProperty.isArray)
			{
				UnityEngine.Object[] targetObjects = unityProperty.serializedObject.targetObjects;
				for (int i = 0; i < targetObjects.Length; i++)
				{
					EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
					target.SetValue(base.ValueEntry.Values[i]);
				}
				unityProperty.serializedObject.Update();
			}
			ImguiElementUtils.EmbedVisualElementAndDrawItHere(element, label);
		}

		public void Dispose()
		{
			if (element != null)
			{
				element.Unbind();
				element.RemoveFromHierarchy();
				element = null;
			}
		}
	}
}
