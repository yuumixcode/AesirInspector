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
	/// Unity property attribute drawer.
	/// </summary>
	[OdinDontRegister]
	[DrawerPriority(0.0, 0.0, 999.5)]
	public sealed class UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint> : OdinAttributeDrawer<TAttribute>, IDisposable, IUnityPropertyFieldDrawer where TDrawer : PropertyDrawer, new() where TAttribute : TAttributeConstraint where TAttributeConstraint : PropertyAttribute
	{
		private static readonly FieldInfo InternalAttributeFieldInfo;

		private static readonly FieldInfo InternalFieldInfoFieldInfo;

		private static MethodInfo createPropertyGUIMethod;

		private static readonly ValueSetter<TDrawer, Attribute> SetAttribute;

		private static readonly ValueSetter<TDrawer, FieldInfo> SetFieldInfo;

		private TDrawer drawer;

		private object propertyHandler;

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
		/// Initializes the drawer.
		/// </summary>
		public UnityPropertyAttributeDrawer()
		{
			drawer = new TDrawer();
			if (UnityPropertyHandlerUtility.IsAvailable)
			{
				propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(drawer);
			}
		}

		static UnityPropertyAttributeDrawer()
		{
			InternalAttributeFieldInfo = typeof(TDrawer).GetField("m_Attribute", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			InternalFieldInfoFieldInfo = typeof(TDrawer).GetField("m_FieldInfo", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (InternalAttributeFieldInfo == null)
			{
				Debug.LogError("Could not find the internal Unity field 'PropertyDrawer.m_Attribute'; UnityPropertyAttributeDrawer alias '" + typeof(UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
			}
			else
			{
				SetAttribute = EmitUtilities.CreateInstanceFieldSetter<TDrawer, Attribute>(InternalAttributeFieldInfo);
			}
			if (InternalFieldInfoFieldInfo == null)
			{
				Debug.LogError("Could not find the internal Unity field 'PropertyDrawer.m_FieldInfo'; UnityPropertyAttributeDrawer alias '" + typeof(UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.");
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

		protected override void Initialize()
		{
			if (base.Property.ChildResolver is ICollectionResolver)
			{
				base.SkipWhenDrawing = true;
			}
		}

		/// <summary>
		/// Draws the proprety.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty property = base.Property;
			IPropertyValueEntry entry = property.ValueEntry;
			if (SetAttribute == null || SetFieldInfo == null)
			{
				SirenixEditorGUI.MessageBox("Could not find the internal Unity fields 'PropertyDrawer.m_Attribute' or 'PropertyDrawer.m_FieldInfo'; UnityPropertyAttributeDrawer alias '" + typeof(UnityPropertyAttributeDrawer<TDrawer, TAttribute, TAttributeConstraint>).GetNiceName() + "' has been disabled.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			if (entry == null)
			{
				SirenixEditorGUI.MessageBox("Cannot put the attribute '" + typeof(TAttribute)?.ToString() + "' on a property of type '" + property.Info.PropertyType.ToString() + "'.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			FieldInfo fieldInfo;
			SerializedProperty unityProperty = property.Tree.GetUnityPropertyForPath(property.Path, out fieldInfo);
			if (unityProperty == null)
			{
				if (UnityVersion.IsVersionOrGreater(2017, 1))
				{
					CallNextDrawer(label);
					return;
				}
				SirenixEditorGUI.MessageBox("Could not get a Unity SerializedProperty for the property '" + property.NiceName + "' of type '" + entry.TypeOfValue.GetNiceName() + "' at path '" + entry.Property.Path + "'.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				return;
			}
			SetFieldInfo(ref drawer, fieldInfo);
			SetAttribute(ref drawer, base.Attribute);
			label = label ?? GUIContent.none;
			if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
			{
				UnityEngine.Object[] targetObjects = unityProperty.serializedObject.targetObjects;
				for (int i = 0; i < targetObjects.Length; i++)
				{
					EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
					target.SetWeakValue(entry.WeakValues[i]);
				}
				unityProperty.serializedObject.Update();
				unityProperty = unityProperty.serializedObject.FindProperty(unityProperty.propertyPath);
			}
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport && createPropertyGUIMethod != null && element == null)
			{
				element = ImguiElementUtils.CreatePropertyFieldElement(unityProperty, delegate(SerializedProperty prop)
				{
					if (prop.serializedObject.targetObject is EmittedScriptableObject)
					{
						ApplyValueWeak(base.Property.ValueEntry, prop);
					}
				});
			}
			if (GlobalConfig<GeneralDrawerConfig>.Instance.EnableUIToolkitSupport && element != null)
			{
				ImguiElementUtils.EmbedVisualElementAndDrawItHere(element, label);
			}
			else
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
				bool changed = EditorGUI.EndChangeCheck();
				if (unityProperty.serializedObject.targetObject is EmittedScriptableObject)
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
						for (int j = 0; j < baseValueEntry.ValueCount; j++)
						{
							baseValueEntry.TriggerOnValueChanged(j);
						}
					});
				}
			}
			if (label == GUIContent.none && label.text != "")
			{
				label.text = "";
			}
		}

		private static void ApplyValueWeak(IPropertyValueEntry entry, SerializedProperty unityProperty)
		{
			if (entry.IsEditable)
			{
				UnityEngine.Object[] targetObjects = unityProperty.serializedObject.targetObjects;
				for (int i = 0; i < targetObjects.Length; i++)
				{
					EmittedScriptableObject target = (EmittedScriptableObject)targetObjects[i];
					entry.WeakValues[i] = target.GetWeakValue();
				}
				entry.WeakValues.ForceMarkDirty();
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
