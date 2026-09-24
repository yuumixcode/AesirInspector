using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Base class to derive from for value drawers that merely wish to cause a value to be drawn by Unity.
	/// </summary>
	public abstract class DrawWithUnityBaseDrawer<T> : OdinValueDrawer<T>
	{
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
			bool isEmittedProperty = unityProperty.serializedObject.targetObject is EmittedScriptableObject<T>;
			if (isEmittedProperty)
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
			if (label == null)
			{
				label = GUIContent.none;
			}
			if (!isEmittedProperty)
			{
				EditorGUI.BeginChangeCheck();
			}
			EditorGUILayout.PropertyField(unityProperty, label, true);
			if (!isEmittedProperty && EditorGUI.EndChangeCheck())
			{
				entry.Values.ForceMarkDirty();
			}
			if (isEmittedProperty)
			{
				unityProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				Object[] targetObjects2 = unityProperty.serializedObject.targetObjects;
				for (int j = 0; j < targetObjects2.Length; j++)
				{
					EmittedScriptableObject<T> target2 = (EmittedScriptableObject<T>)targetObjects2[j];
					entry.Values[j] = target2.GetValue();
				}
			}
			if (label == GUIContent.none && label.text != "")
			{
				label.text = "";
			}
		}
	}
}
