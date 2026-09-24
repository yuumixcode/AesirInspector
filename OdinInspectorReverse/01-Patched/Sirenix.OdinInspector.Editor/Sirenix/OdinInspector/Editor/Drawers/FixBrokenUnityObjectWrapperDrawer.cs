using System;
using System.Linq;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.001, 0.0, 0.0)]
	public class FixBrokenUnityObjectWrapperDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems where T : Component
	{
		[TypeInfoBox("This asset reference is temporarily broken until the next reload, because of an error in Unity where the C# wrapper object of a prefab asset is destroyed when changes are made to that prefab asset. This error has been reported to Unity.\n\nMeanwhile, Odin can fix this for you by getting a new, valid wrapper object from the asset database and replacing the broken wrapper instance with the new one.")]
		private class FixBrokenUnityObjectWrapperPopup
		{
			private IPropertyValueEntry<T> valueEntry;

			public FixBrokenUnityObjectWrapperPopup(IPropertyValueEntry<T> valueEntry)
			{
				this.valueEntry = valueEntry;
			}

			[HorizontalGroup(0f, 0, 0, 0f)]
			[Button(ButtonSizes.Large)]
			public void FixItThisTime()
			{
				for (int i = 0; i < valueEntry.ValueCount; i++)
				{
					int localI = i;
					T fixedComponent = null;
					if (FixBrokenUnityObjectWrapperDrawer<T>.ComponentIsBroken(valueEntry.Values[i], ref fixedComponent) && (bool)fixedComponent)
					{
						valueEntry.Property.Tree.DelayActionUntilRepaint(delegate
						{
							(valueEntry as IValueEntryActualValueSetter<T>).SetActualValue(localI, fixedComponent);
						});
					}
				}
				if ((bool)GUIHelper.CurrentWindow)
				{
					EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(GUIHelper.CurrentWindow.Close));
				}
			}

			[Button(ButtonSizes.Large)]
			[HorizontalGroup(0f, 0, 0, 0f)]
			public void FixItAlways()
			{
				EditorPrefs.SetBool("TemporarilyBrokenUnityObjectWrapperDrawer.autoFix", value: true);
				FixBrokenUnityObjectWrapperDrawer<T>.autoFix = true;
				if ((bool)GUIHelper.CurrentWindow)
				{
					EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, new EditorApplication.CallbackFunction(GUIHelper.CurrentWindow.Close));
				}
			}
		}

		private const string AUTO_FIX_PREFS_KEY = "TemporarilyBrokenUnityObjectWrapperDrawer.autoFix";

		private bool isBroken;

		private T realWrapperInstance;

		private bool allowSceneViewObjects;

		private static bool autoFix;

		protected override void Initialize()
		{
			allowSceneViewObjects = base.ValueEntry.Property.GetAttribute<AssetsOnlyAttribute>() == null;
			autoFix = EditorPrefs.HasKey("TemporarilyBrokenUnityObjectWrapperDrawer.autoFix");
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (base.ValueEntry.ValueState != PropertyValueState.NullReference && base.ValueEntry.ValueState != PropertyValueState.ReferenceValueConflict)
			{
				CallNextDrawer(label);
				return;
			}
			if (Event.current.type == EventType.Layout)
			{
				isBroken = false;
				int count = base.ValueEntry.ValueCount;
				for (int i = 0; i < count; i++)
				{
					T component = base.ValueEntry.Values[i];
					if (ComponentIsBroken(component, ref realWrapperInstance))
					{
						isBroken = true;
						break;
					}
				}
				if (isBroken && autoFix)
				{
					isBroken = false;
					for (int j = 0; j < base.ValueEntry.ValueCount; j++)
					{
						T fixedComponent = null;
						if (ComponentIsBroken(base.ValueEntry.Values[j], ref fixedComponent) && (bool)fixedComponent)
						{
							(base.ValueEntry as IValueEntryActualValueSetter<T>).SetActualValue(j, fixedComponent);
						}
					}
					base.ValueEntry.Update();
				}
			}
			if (!isBroken)
			{
				CallNextDrawer(label);
				return;
			}
			Rect rect = EditorGUILayout.GetControlRect(label != null);
			Rect btnRect = rect.AlignRight(20f);
			Rect controlRect = rect.SetXMax(btnRect.xMin - 5f);
			object newInstance = null;
			EditorGUI.BeginChangeCheck();
			newInstance = ((!base.ValueEntry.BaseValueType.IsInterface) ? (SirenixEditorFields.UnityObjectField(controlRect, label, realWrapperInstance, base.ValueEntry.BaseValueType, allowSceneViewObjects) as Component) : SirenixEditorFields.PolymorphicObjectField(controlRect, label, realWrapperInstance, base.ValueEntry.BaseValueType, allowSceneViewObjects));
			if (EditorGUI.EndChangeCheck())
			{
				base.ValueEntry.WeakSmartValue = newInstance;
			}
			if (GUI.Button(btnRect, " ", EditorStyles.miniButton))
			{
				FixBrokenUnityObjectWrapperPopup popup = new FixBrokenUnityObjectWrapperPopup(base.ValueEntry);
				OdinEditorWindow.InspectObjectInDropDown(popup, 300f);
			}
			if (Event.current.type == EventType.Repaint)
			{
				GUI.DrawTexture(btnRect, EditorIcons.ConsoleWarnicon, ScaleMode.ScaleToFit);
			}
		}

		private static bool ComponentIsBroken(T component, ref T realInstance)
		{
			if ((object)component != null && component == null)
			{
				OdinEntityId entityId = OdinEntityId.FromObject(component);
				if (entityId.IsInAssetDatabase())
				{
					string path = entityId.GetAssetPath();
					T realWrapper = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault((UnityEngine.Object n) => OdinEntityId.FromObject(n) == entityId) as T;
					if ((bool)realWrapper)
					{
						realInstance = realWrapper;
						return true;
					}
				}
			}
			return false;
		}

		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			if (EditorPrefs.HasKey("TemporarilyBrokenUnityObjectWrapperDrawer.autoFix"))
			{
				genericMenu.AddItem(new GUIContent("Disable auto-fix of broken prefab instance references"), on: false, delegate
				{
					EditorPrefs.DeleteKey("TemporarilyBrokenUnityObjectWrapperDrawer.autoFix");
					autoFix = false;
				}, null);
			}
		}
	}
}
