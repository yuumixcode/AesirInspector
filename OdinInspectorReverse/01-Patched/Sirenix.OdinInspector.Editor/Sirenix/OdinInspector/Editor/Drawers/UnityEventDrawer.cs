using System;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Unity event drawer.
	/// </summary>
	[DrawerPriority(0.0, 0.0, 0.45)]
	public sealed class UnityEventDrawer<T> : UnityPropertyDrawer<UnityEventDrawer, T> where T : UnityEventBase
	{
		protected override void Initialize()
		{
			base.Initialize();
			delayApplyValueUntilRepaint = true;
			dontUseVisualElements = true;
			drawer = new UnityEventDrawer();
			if (UnityPropertyHandlerUtility.IsAvailable)
			{
				propertyHandler = UnityPropertyHandlerUtility.CreatePropertyHandler(drawer);
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			FieldInfo fieldInfo;
			SerializedProperty unityProperty = base.Property.Tree.GetUnityPropertyForPath(base.Property.Path, out fieldInfo);
			if (unityProperty == null)
			{
				if (UnityVersion.IsVersionOrGreater(2017, 1))
				{
					CallNextDrawer(label);
					return;
				}
				if (!typeof(T).IsDefined<SerializableAttribute>())
				{
					SirenixEditorGUI.MessageBox("You have likely forgotten to mark your custom UnityEvent class '" + typeof(T).GetNiceName() + "' with the [Serializable] attribute! Could not get a Unity SerializedProperty for the property '" + base.Property.NiceName + "' of type '" + base.ValueEntry.TypeOfValue.GetNiceName() + "' at path '" + base.Property.Path + "'.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
					return;
				}
			}
			base.DrawPropertyLayout(label);
		}
	}
}
