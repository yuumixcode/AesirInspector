using System;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(10.0, 0.0, 0.0)]
	public sealed class FixUnityNullDrawer<T> : OdinValueDrawer<T> where T : class
	{
		public override bool CanDrawTypeFilter(Type type)
		{
			return !typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<T> entry = base.ValueEntry;
			if (entry.ValueState == PropertyValueState.NullReference && !entry.SerializationBackend.SupportsPolymorphism)
			{
				bool possibleRecursion = false;
				for (InspectorProperty prop = entry.Property.Parent; prop != null; prop = prop.Parent)
				{
					if (prop.ValueEntry != null)
					{
						if (prop.ValueEntry.SerializationBackend.SupportsPolymorphism)
						{
							break;
						}
						if (prop.ValueEntry.TypeOfValue == typeof(T) || prop.ValueEntry.BaseValueType == typeof(T))
						{
							possibleRecursion = true;
							break;
						}
					}
				}
				if (possibleRecursion)
				{
					SirenixEditorGUI.MessageBox("Possible Unity serialization recursion detected; cutting off drawing pre-emptively.", MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
					return;
				}
				if (Event.current.type == EventType.Layout)
				{
					SerializedObject serializedObject = null;
					if (base.Property.Info.IsUnityPropertyOnly)
					{
						serializedObject = base.Property.Tree.UnitySerializedObject;
					}
					for (int i = 0; i < entry.ValueCount; i++)
					{
						object value = UnitySerializationUtility.CreateDefaultUnityInitializedObject(typeof(T));
						entry.WeakValues.ForceSetValue(i, value);
					}
					base.Property.RecordForUndo("Odin fixing null Unity-backed values");
					entry.ApplyChanges();
					PropertyTree tree = base.Property.Tree;
					if (base.Property.Info.IsUnityPropertyOnly)
					{
						serializedObject?.ApplyModifiedPropertiesWithoutUndo();
					}
					base.Property.Update(forceUpdate: true);
				}
			}
			CallNextDrawer(label);
		}
	}
}
