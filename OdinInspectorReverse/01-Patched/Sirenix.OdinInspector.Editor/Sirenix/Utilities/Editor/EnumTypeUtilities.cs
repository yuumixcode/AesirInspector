using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Utilities.Editor
{
	public static class EnumTypeUtilities<T>
	{
		public struct EnumMember
		{
			public T Value;

			public string Name;

			public string NiceName;

			public bool IsObsolete;

			public string Message;

			public bool Hide;

			public SdfIconType Icon;

			public string Tooltip;
		}

		private static readonly string[] enumNames;

		private static readonly string[] niceNames;

		private static readonly EnumMember[] allMembers;

		private static readonly EnumMember[] visibleMembers;

		private static readonly Dictionary<T, int> enumValueIndexLookup;

		private static readonly Type InspectorNameAttribute_Type;

		private static readonly FieldInfo InspectorNameAttribute_displayName;

		private static readonly bool isFlagEnum;

		public static bool IsFlagEnum => isFlagEnum;

		public static string[] Names => enumNames;

		public static string[] NiceNames => niceNames;

		public static EnumMember[] AllEnumMemberInfos => allMembers;

		public static EnumMember[] VisibleEnumMemberInfos => visibleMembers;

		static EnumTypeUtilities()
		{
			enumValueIndexLookup = new Dictionary<T, int>();
			if (!typeof(T).IsEnum)
			{
				throw new InvalidCastException(typeof(T)?.ToString() + " Is not an enum type");
			}
			InspectorNameAttribute_Type = typeof(UnityEngine.Object).Assembly.GetType("UnityEngine.InspectorNameAttribute");
			if (InspectorNameAttribute_Type != null)
			{
				InspectorNameAttribute_displayName = InspectorNameAttribute_Type.GetField("displayName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			FieldInfo[] fields = typeof(T).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
			enumNames = new string[fields.Length];
			niceNames = new string[fields.Length];
			allMembers = new EnumMember[fields.Length];
			List<EnumMember> visibleMembersList = new List<EnumMember>(fields.Length);
			for (int i = 0; i < fields.Length; i++)
			{
				FieldInfo field = fields[i];
				EnumMember info = default(EnumMember);
				try
				{
					info.Value = (T)Enum.Parse(typeof(T), field.Name);
					info.Name = field.Name;
					info.NiceName = info.Name.SplitPascalCase();
					ObsoleteAttribute obs = field.GetAttribute<ObsoleteAttribute>(inherit: true);
					InfoBoxAttribute msg = field.GetAttribute<InfoBoxAttribute>(inherit: true);
					HideInInspector hide = field.GetAttribute<HideInInspector>();
					LabelTextAttribute lblText = field.GetAttribute<LabelTextAttribute>(inherit: true);
					string tooltip = field.GetAttribute<TooltipAttribute>()?.tooltip ?? field.GetAttribute<PropertyTooltipAttribute>()?.Tooltip;
					info.IsObsolete = obs != null;
					StringBuilder message = new StringBuilder();
					if (obs != null)
					{
						message.Append(obs.Message);
					}
					if (msg != null)
					{
						if (message.Length > 0)
						{
							message.Append("\n\n");
						}
						message.Append(msg.Message);
					}
					if (tooltip != null)
					{
						if (message.Length > 0)
						{
							message.Append("\n\n");
						}
						message.Append(tooltip);
					}
					info.Message = message.ToString();
					info.Hide = hide != null;
					info.Tooltip = tooltip ?? "";
					if (lblText != null)
					{
						info.NiceName = (string.IsNullOrEmpty(lblText.Text) ? info.NiceName : lblText.Text);
						if (lblText.NicifyText)
						{
							info.NiceName = ObjectNames.NicifyVariableName(info.NiceName);
						}
						info.Icon = lblText.Icon;
					}
					if (InspectorNameAttribute_displayName != null)
					{
						object[] inspectorNames = field.GetCustomAttributes(InspectorNameAttribute_Type, inherit: false);
						if (inspectorNames.Length != 0)
						{
							info.NiceName = ((string)InspectorNameAttribute_displayName.GetValue(inspectorNames[0])) ?? "";
						}
					}
				}
				catch (Exception ex)
				{
					info.Message = ex.Message;
				}
				info.Message = info.Message ?? "";
				allMembers[i] = info;
				enumNames[i] = info.Name;
				niceNames[i] = info.NiceName;
				enumValueIndexLookup[info.Value] = i;
				if (!info.Hide)
				{
					visibleMembersList.Add(info);
				}
			}
			visibleMembers = visibleMembersList.ToArray();
			isFlagEnum = typeof(T).IsDefined<FlagsAttribute>();
		}

		public static T[] DecomposeEnumFlagValues(T enumFlagValue)
		{
			if (!typeof(T).IsEnum)
			{
				throw new InvalidCastException();
			}
			List<T> decomposedEnumValues = new List<T>();
			Array values = Enum.GetValues(typeof(T));
			long enumFlagValueInt = Convert.ToInt64(enumFlagValue);
			for (int i = 0; i < values.Length; i++)
			{
				T column = (T)values.GetValue(i);
				if ((enumFlagValueInt & Convert.ToInt64(column)) != 0L)
				{
					decomposedEnumValues.Add(column);
				}
			}
			return decomposedEnumValues.ToArray();
		}

		public static int GetIndexOfEnumValue(T enumValue)
		{
			if (enumValueIndexLookup.TryGetValue(enumValue, out var index))
			{
				return index;
			}
			throw new Exception("No member with the value " + enumValue.ToString() + " was found on the Enum " + typeof(T).GetNiceFullName());
		}

		public static EnumMember GetEnumMemberInfo(T value)
		{
			int index = GetIndexOfEnumValue(value);
			return allMembers[index];
		}
	}
}
