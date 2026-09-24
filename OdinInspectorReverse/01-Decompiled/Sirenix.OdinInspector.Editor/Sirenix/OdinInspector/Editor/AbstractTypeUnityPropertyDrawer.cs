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
	/// Unity property drawer for abstract types.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.4999)]
	[OdinDontRegister]
	public sealed class AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T> : OdinValueDrawer<T>, IDisposable, IUnityPropertyFieldDrawer where TDrawer : PropertyDrawer, new() where T : TDrawnType
	{
		private static readonly FieldInfo InternalFieldInfoFieldInfo;

		private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

		private TDrawer drawer;

		private object propertyHandler;

		private OdinImGuiElement element;

		private static MethodInfo createPropertyGUIMethod;

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
		public AbstractTypeUnityPropertyDrawer()
		{
			drawer = new TDrawer();
			if (UnityPropertyHandlerUtility.IsAvailable)
			{
				propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(drawer);
			}
		}

		static AbstractTypeUnityPropertyDrawer()
		{
			InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (InternalFieldInfoFieldInfo == null)
			{
				Debug.LogError("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T>).GetNiceName() + "' has been disabled.");
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
			IPropertyValueEntry<T> entry = base.ValueEntry;
			if (SetFieldInfo == null)
			{
				SirenixEditorGUI.MessageBox("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyDrawer alias '" + typeof(AbstractTypeUnityPropertyDrawer<TDrawer, TDrawnType, T>).GetNiceName() + "' has been disabled.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			FieldInfo fieldInfo;
			SerializedProperty unityProperty = entry.Property.Tree.GetUnityPropertyForPath(entry.Property.Path, out fieldInfo);
			if (unityProperty == null)
			{
				SirenixEditorGUI.MessageBox("Could not get a Unity SerializedProperty for the property '" + entry.Property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'. Legacy Unity drawing compatibility is broken for this property; falling back to normal Odin drawing. Please report an issue on Odin's issue tracker with details.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				CallNextDrawer(label);
				return;
			}
			label = label ?? GUIContent.none;
			SetFieldInfo(ref drawer, fieldInfo);
			if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
			{
				UnityEngine.Object[] targetObjects = unityProperty.serializedObject.targetObjects;
				for (int i = 0; i < targetObjects.Length; i++)
				{
					EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
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
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport && createPropertyGUIMethod != null && element == null)
			{
				element = ImguiElementUtils.CreatePropertyFieldElement(unityProperty, delegate(SerializedProperty prop)
				{
					if (base.ValueEntry.IsEditable && prop.serializedObject.targetObject is EmittedScriptableObject<T>)
					{
						UnityEngine.Object[] targetObjects3 = prop.serializedObject.targetObjects;
						for (int k = 0; k < targetObjects3.Length; k++)
						{
							EmittedScriptableObject<T> emittedScriptableObject = (EmittedScriptableObject<T>)targetObjects3[k];
							base.ValueEntry.Values[k] = emittedScriptableObject.GetValue();
						}
						base.ValueEntry.Values.ForceMarkDirty();
					}
				});
			}
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport && element != null)
			{
				ImguiElementUtils.EmbedVisualElementAndDrawItHere(element, label);
			}
			else
			{
				bool changed;
				try
				{
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
					changed = EditorGUI.EndChangeCheck();
				}
				finally
				{
					if (label == GUIContent.none && label.text != "")
					{
						label.text = "";
					}
				}
				if (unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>)
				{
					if (unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo() || changed)
					{
						ApplyValueStrong(entry, unityProperty);
					}
				}
				else if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
				{
					if (unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo() || changed)
					{
						ApplyValueWeak(entry, unityProperty);
					}
				}
				else if (changed)
				{
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
			if (label == GUIContent.none && label.text != "")
			{
				label.text = "";
			}
		}

		private static void ApplyValueWeak(IPropertyValueEntry<T> entry, SerializedProperty unityProperty)
		{
			UnityEngine.Object[] targetObjects = unityProperty.serializedObject.targetObjects;
			for (int i = 0; i < targetObjects.Length; i++)
			{
				EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
				entry.Values[i] = (T)target.GetWeakValue();
			}
			entry.Values.ForceMarkDirty();
		}

		private static void ApplyValueStrong(IPropertyValueEntry<T> entry, SerializedProperty unityProperty)
		{
			UnityEngine.Object[] targetObjects = unityProperty.serializedObject.targetObjects;
			for (int i = 0; i < targetObjects.Length; i++)
			{
				EmittedScriptableObject<T> target = (EmittedScriptableObject<T>)targetObjects[i];
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
