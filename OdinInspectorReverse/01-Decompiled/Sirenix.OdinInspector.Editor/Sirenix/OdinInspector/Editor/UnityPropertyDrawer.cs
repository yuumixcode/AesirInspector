using System;
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
	/// Unity property drawer.
	/// </summary>
	[OdinDontRegister]
	[DrawerPriority(0.0, 0.0, 0.5)]
	public class UnityPropertyDrawer<TDrawer, TDrawnType> : OdinValueDrawer<TDrawnType>, IDisposable, IUnityPropertyFieldDrawer where TDrawer : PropertyDrawer, new()
	{
		private static readonly FieldInfo InternalFieldInfoFieldInfo;

		private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

		private static MethodInfo createPropertyGUIMethod;

		protected TDrawer drawer;

		protected object propertyHandler;

		protected bool delayApplyValueUntilRepaint;

		protected bool dontUseVisualElements;

		private OdinImGuiElement element;

		public bool WillDrawPropertyField
		{
			get
			{
				if (!(createPropertyGUIMethod != null))
				{
					return propertyHandler != null;
				}
				return true;
			}
		}

		/// <summary>
		/// Initializes the property drawer.
		/// </summary>
		public UnityPropertyDrawer()
		{
			drawer = new TDrawer();
			if (UnityPropertyHandlerUtility.IsAvailable)
			{
				propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(drawer);
			}
		}

		static UnityPropertyDrawer()
		{
			InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (InternalFieldInfoFieldInfo == null)
			{
				Debug.LogError("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(UnityPropertyDrawer<TDrawer, TDrawnType>).GetNiceName() + "' has been disabled.");
			}
			else
			{
				SetFieldInfo = EmitUtilities.CreateInstanceFieldSetter<TDrawer, FieldInfo>(InternalFieldInfoFieldInfo);
			}
			MethodInfo method = typeof(TDrawer).GetMethod("CreatePropertyGUI", BindingFlags.Instance | BindingFlags.Public, null, new Type[1] { typeof(SerializedProperty) }, null);
			if (method.DeclaringType != typeof(PropertyDrawer))
			{
				createPropertyGUIMethod = method;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<TDrawnType> entry = base.ValueEntry;
			if (SetFieldInfo == null)
			{
				SirenixEditorGUI.MessageBox("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(UnityPropertyDrawer<TDrawer, TDrawnType>).GetNiceName() + "' has been disabled.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			FieldInfo fieldInfo;
			SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);
			if (unityProperty == null)
			{
				if (UnityVersion.IsVersionOrGreater(2017, 1))
				{
					CallNextDrawer(label);
					return;
				}
				SirenixEditorGUI.MessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			label = label ?? GUIContent.none;
			SetFieldInfo(ref drawer, fieldInfo);
			if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<TDrawnType>)
			{
				UnityEngine.Object[] targetObjects = unityProperty.serializedObject.targetObjects;
				for (int i = 0; i < targetObjects.Length; i++)
				{
					EmittedScriptableObject<TDrawnType> target = (EmittedScriptableObject<TDrawnType>)targetObjects[i];
					target.SetValue(entry.Values[i]);
				}
				unityProperty.serializedObject.Update();
			}
			else if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
			{
				UnityEngine.Object[] targetObjects2 = unityProperty.serializedObject.targetObjects;
				for (int j = 0; j < targetObjects2.Length; j++)
				{
					EmittedScriptableObject target2 = (EmittedScriptableObject)targetObjects2[j];
					target2.SetWeakValue(entry.Values[j]);
				}
				unityProperty.serializedObject.Update();
				unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
			}
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport && !dontUseVisualElements && createPropertyGUIMethod != null && element == null)
			{
				element = ImguiElementUtils.CreatePropertyFieldElement(unityProperty, delegate(SerializedProperty prop)
				{
					if (prop.serializedObject.targetObject is EmittedScriptableObject)
					{
						ApplyValueWeak(base.ValueEntry, prop);
					}
				});
			}
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport && element != null)
			{
				ImguiElementUtils.EmbedVisualElementAndDrawItHere(element, label);
				return;
			}
			float height = ((propertyHandler == null) ? drawer.GetPropertyHeight(unityProperty.Copy(), label) : UnityPropertyHandlerUtility.PropertyHandlerGetHeight(propertyHandler, unityProperty.Copy(), label, includeChildren: false));
			Rect position = EditorGUILayout.GetControlRect(false, height);
			EditorGUI.BeginChangeCheck();
			if (propertyHandler != null)
			{
				UnityPropertyHandlerUtility.PropertyHandlerOnGUI(propertyHandler, position, unityProperty, label, includeChildren: false);
			}
			else
			{
				drawer.OnGUI(position, unityProperty, label);
			}
			bool changed = EditorGUI.EndChangeCheck();
			if (label == GUIContent.none && label.text != "")
			{
				label.text = "";
			}
			if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<TDrawnType>)
			{
				if (!(unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo() || changed))
				{
					return;
				}
				if (delayApplyValueUntilRepaint)
				{
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						ApplyValueStrong(entry, unityProperty);
					});
				}
				else
				{
					ApplyValueStrong(entry, unityProperty);
				}
			}
			else if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
			{
				if (!(unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo() || changed))
				{
					return;
				}
				if (delayApplyValueUntilRepaint)
				{
					base.Property.Tree.DelayActionUntilRepaint(delegate
					{
						ApplyValueWeak(entry, unityProperty);
					});
				}
				else
				{
					ApplyValueWeak(entry, unityProperty);
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

		private static void ApplyValueWeak(IPropertyValueEntry<TDrawnType> entry, SerializedProperty unityProperty)
		{
			UnityEngine.Object[] targetObjects = unityProperty.serializedObject.targetObjects;
			for (int i = 0; i < targetObjects.Length; i++)
			{
				EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
				entry.Values[i] = (TDrawnType)target.GetWeakValue();
			}
			entry.Values.ForceMarkDirty();
		}

		private static void ApplyValueStrong(IPropertyValueEntry<TDrawnType> entry, SerializedProperty unityProperty)
		{
			UnityEngine.Object[] targetObjects = unityProperty.serializedObject.targetObjects;
			for (int i = 0; i < targetObjects.Length; i++)
			{
				EmittedScriptableObject<TDrawnType> target = (EmittedScriptableObject<TDrawnType>)targetObjects[i];
				entry.Values[i] = target.GetValue();
			}
			entry.Values.ForceMarkDirty();
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
