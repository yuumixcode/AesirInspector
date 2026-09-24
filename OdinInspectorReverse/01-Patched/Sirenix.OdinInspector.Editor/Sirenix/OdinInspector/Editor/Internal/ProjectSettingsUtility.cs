using System;
using System.Reflection;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public static class ProjectSettingsUtility
	{
		public static void InitAllProjectSettingFieldsFromAttributes(UnityEngine.Object instance)
		{
			Type type = instance.GetType();
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo field in fields)
			{
				if (!typeof(IProjectSetting).IsAssignableFrom(field.FieldType))
				{
					continue;
				}
				ProjectSettingKeyAttribute attr = field.GetAttribute<ProjectSettingKeyAttribute>();
				if (attr != null)
				{
					IProjectSetting settings = field.GetValue(instance) as IProjectSetting;
					if (settings == null)
					{
						settings = Activator.CreateInstance(field.FieldType) as IProjectSetting;
						field.SetValue(instance, settings);
					}
					settings.SetInitData(attr.Key, attr.DefaultValue, instance);
				}
			}
		}
	}
}
