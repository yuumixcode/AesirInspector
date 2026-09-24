using System;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	[Serializable]
	[InlineProperty]
	public abstract class ProjectSetting<T> : IProjectSetting
	{
		[SerializeField]
		[HideInInspector]
		private T serializedValue;

		[SerializeField]
		[HideInInspector]
		private bool changedFromDefault;

		[NonSerialized]
		private string key;

		[NonSerialized]
		private bool localOverride;

		[NonSerialized]
		private T currentValue;

		[NonSerialized]
		private bool initialized;

		[NonSerialized]
		private bool hasInitData;

		[NonSerialized]
		private T defaultValue;

		[NonSerialized]
		private UnityEngine.Object serializedContainer;

		[NonSerialized]
		private static Func<T, T, bool> comparer;

		[HideLabel]
		[ShowInInspector]
		[HorizontalGroup(20f, 0, 0, 0f)]
		public bool LocalOverride
		{
			get
			{
				EnsureInitialized();
				return localOverride;
			}
			set
			{
				EnsureInitialized();
				if (localOverride != value)
				{
					localOverride = value;
					EditorPrefs.SetBool(key + "_OVERRIDE", value);
					if (localOverride)
					{
						currentValue = GetLocalValue(key, defaultValue);
					}
					else
					{
						currentValue = serializedValue;
					}
				}
			}
		}

		public string Key => key;

		[InlineProperty]
		[HideLabel]
		[HorizontalGroup(0f, 0, 0, 0f)]
		[ShowInInspector]
		[SuppressInvalidAttributeError]
		public T Value
		{
			get
			{
				EnsureInitialized();
				return currentValue;
			}
			set
			{
				EnsureInitialized();
				if (!Equals(currentValue, value))
				{
					currentValue = value;
					if (localOverride)
					{
						SetLocalValue(Key, value);
						return;
					}
					changedFromDefault = true;
					serializedValue = value;
				}
			}
		}

		public bool IsDefault
		{
			get
			{
				if (LocalOverride)
				{
					return false;
				}
				if (changedFromDefault)
				{
					return false;
				}
				return true;
			}
		}

		public ProjectSetting()
		{
		}

		public ProjectSetting(string key, T defaultValue, UnityEngine.Object serializedContainer)
		{
			((IProjectSetting)this).SetInitData(key, (object)defaultValue, serializedContainer);
		}

		void IProjectSetting.SetInitData(string key, object defaultValue, UnityEngine.Object serializedContainer)
		{
			this.key = key;
			this.defaultValue = (T)defaultValue;
			hasInitData = true;
			this.serializedContainer = serializedContainer;
		}

		public void Reset()
		{
			ThrowIfNoInitData();
			DeleteLocalValue();
			LocalOverride = false;
			currentValue = defaultValue;
			serializedValue = default(T);
			changedFromDefault = false;
		}

		public void DeleteLocalValue()
		{
			ThrowIfNoInitData();
			EditorPrefs.DeleteKey(key);
			initialized = false;
		}

		private void ThrowIfNoInitData()
		{
			if (!hasInitData)
			{
				throw new InvalidOperationException("A project setting of type '" + GetType().GetNiceName() + "' was used before init data was set. Did you forget to declare it in a class derived from ProjectSettingsConfig<T>, or did you forget to decorate its field with a [ProjectSettingKey(\"SOME_KEY\", some_default_value)]?");
			}
		}

		private void EnsureInitialized()
		{
			if (!initialized)
			{
				ThrowIfNoInitData();
				localOverride = EditorPrefs.GetBool(key + "_OVERRIDE", defaultValue: false);
				if (localOverride)
				{
					currentValue = GetLocalValue(key, default(T));
				}
				else
				{
					currentValue = (changedFromDefault ? serializedValue : defaultValue);
				}
				initialized = true;
			}
		}

		protected virtual bool Equals(T a, T b)
		{
			if (comparer == null)
			{
				comparer = TypeExtensions.GetEqualityComparerDelegate<T>();
			}
			return comparer(a, b);
		}

		protected abstract T GetLocalValue(string key, T defaultValue);

		protected abstract void SetLocalValue(string key, T value);

		public static implicit operator T(ProjectSetting<T> projectSetting)
		{
			return projectSetting.Value;
		}

		public void Draw(Rect rect, string label, string tooltip, bool drawLocalOverride, bool allowBold = true)
		{
			if (Event.current.OnContextClick(rect))
			{
				GenericMenu menu = new GenericMenu();
				if (LocalOverride)
				{
					menu.AddItem(new GUIContent("Remove local override"), on: false, delegate
					{
						LocalOverride = false;
						EditorUtility.SetDirty(serializedContainer);
					});
				}
				else
				{
					menu.AddDisabledItem(new GUIContent("Remove local override"));
				}
				if (changedFromDefault)
				{
					menu.AddItem(new GUIContent("Reset to default"), on: false, delegate
					{
						Reset();
						EditorUtility.SetDirty(serializedContainer);
					});
				}
				else
				{
					menu.AddDisabledItem(new GUIContent("Reset to default"));
				}
				menu.ShowAsContext();
			}
			bool disableGUI = LocalOverride && !drawLocalOverride;
			bool boldLabel = !disableGUI && ((LocalOverride == drawLocalOverride && changedFromDefault) || LocalOverride);
			GUIHelper.PushGUIEnabled(GUI.enabled && !disableGUI);
			GUIHelper.PushIsBoldLabel(boldLabel && allowBold);
			EditorGUI.BeginChangeCheck();
			if (boldLabel && SirenixEditorGUI.SDFIconButton(rect.TakeFromRight(20f), GUIContent.none, SdfIconType.ArrowCounterclockwise, IconAlignment.LeftEdge, SirenixGUIStyles.IconButton))
			{
				if (drawLocalOverride)
				{
					EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
					{
						LocalOverride = false;
						EditorUtility.SetDirty(serializedContainer);
					});
				}
				else
				{
					Reset();
					EditorUtility.SetDirty(serializedContainer);
				}
				GUI.changed = true;
				GUIHelper.RemoveFocusControl();
				GUIHelper.RequestRepaint();
			}
			T newValue = Draw(rect, Value, new GUIContent(label, tooltip));
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(serializedContainer);
				if (drawLocalOverride)
				{
					LocalOverride = true;
					Value = newValue;
				}
				else
				{
					LocalOverride = false;
					Value = newValue;
				}
				GUI.changed = true;
			}
			GUIHelper.PopIsBoldLabel();
			GUIHelper.PopGUIEnabled();
		}

		protected abstract T Draw(Rect rect, T value, GUIContent label);
	}
}
