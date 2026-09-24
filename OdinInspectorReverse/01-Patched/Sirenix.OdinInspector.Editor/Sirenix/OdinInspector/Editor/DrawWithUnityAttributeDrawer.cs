using System.Reflection;
using Sirenix.OdinInspector.Editor.Internal.UIToolkitIntegration;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Draws properties marked with <see cref="T:Sirenix.OdinInspector.DrawWithUnityAttribute" />.
	/// </summary>
	/// <seealso cref="T:UnityEngine.RequireComponent" />
	/// <seealso cref="T:Sirenix.OdinInspector.OnInspectorGUIAttribute" />
	/// <seealso cref="T:Sirenix.OdinInspector.InlineEditorAttribute" />
	/// <seealso cref="T:UnityEngine.HideInInspector" />
	[DrawerPriority(0.0, 0.0, 6000.0)]
	public class DrawWithUnityAttributeDrawer<T> : OdinAttributeDrawer<DrawWithUnityAttribute, T>, IUnityPropertyFieldDrawer
	{
		private OdinImGuiElement element;

		public bool WillDrawPropertyField => element != null;

		protected override void Initialize()
		{
			if (base.Attribute.PreferImGUI || !GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport)
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
					Object[] targetObjects = prop.serializedObject.targetObjects;
					for (int i = 0; i < targetObjects.Length; i++)
					{
						EmittedScriptableObject<T> emittedScriptableObject = (EmittedScriptableObject<T>)targetObjects[i];
						base.ValueEntry.Values[i] = emittedScriptableObject.GetValue();
					}
					base.ValueEntry.Values.ForceMarkDirty();
				}
			});
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			FieldInfo fieldInfo;
			SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);
			if (unityProperty == null)
			{
				SirenixEditorGUI.MessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
			{
				Object[] targetObjects = unityProperty.serializedObject.targetObjects;
				for (int i = 0; i < targetObjects.Length; i++)
				{
					EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
					target.SetValue(entry.Values[i]);
				}
				unityProperty.serializedObject.Update();
				unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
			}
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport && element != null)
			{
				ImguiElementUtils.EmbedVisualElementAndDrawItHere(element, label);
				return;
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(unityProperty, label ?? GUIContent.none, true);
			bool changed = EditorGUI.EndChangeCheck();
			if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
			{
				unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				Object[] targetObjects2 = unityProperty.serializedObject.targetObjects;
				for (int j = 0; j < targetObjects2.Length; j++)
				{
					EmittedScriptableObject<T> target2 = (EmittedScriptableObject<T>)targetObjects2[j];
					entry.Values[j] = target2.GetValue();
				}
				if (changed)
				{
					entry.Values.ForceMarkDirty();
				}
			}
			else
			{
				if (!changed)
				{
					return;
				}
				base.Property.Tree.DelayActionUntilRepaint(delegate
				{
					PropertyValueEntry baseValueEntry = base.Property.BaseValueEntry;
					for (int k = 0; k < baseValueEntry.ValueCount; k++)
					{
						baseValueEntry.TriggerOnValueChanged(k);
					}
				});
			}
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
