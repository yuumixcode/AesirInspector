using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public static class EditorGUI_Internals
	{
		private static int? _objectFieldHash;

		private static readonly Func<double> get_s_DragStartValue;

		private static readonly Func<long> get_s_DragStartIntValue;

		private static readonly FieldInfo EditorGUI_s_RecycledEditor_Field;

		private static readonly PropertyInfo EditorGUI_s_RecycledEditor_Property;

		public static int ObjectFieldHash
		{
			get
			{
				if (_objectFieldHash.HasValue)
				{
					return _objectFieldHash.Value;
				}
				FieldInfo field = typeof(EditorGUI).GetField("s_ObjectFieldHash", BindingFlags.Static | BindingFlags.NonPublic);
				if (field != null)
				{
					_objectFieldHash = (int)field.GetValue(null);
				}
				else
				{
					_objectFieldHash = "s_ObjectFieldHash".GetHashCode();
				}
				return _objectFieldHash.Value;
			}
		}

		public static string kFloatFieldFormatString => EditorGUI.kFloatFieldFormatString;

		public static string kDoubleFieldFormatString => EditorGUI.kDoubleFieldFormatString;

		public static string kIntFieldFormatString => EditorGUI.kIntFieldFormatString;

		public static string s_AllowedCharactersForFloat => EditorGUI.s_AllowedCharactersForFloat;

		public static string s_AllowedCharactersForInt => EditorGUI.s_AllowedCharactersForInt;

		public static TextEditor RecycledEditor
		{
			get
			{
				if (EditorGUI_s_RecycledEditor_Field != null)
				{
					return EditorGUI_s_RecycledEditor_Field.GetValue(null) as TextEditor;
				}
				if (EditorGUI_s_RecycledEditor_Property != null)
				{
					return EditorGUI_s_RecycledEditor_Property.GetValue(null) as TextEditor;
				}
				return null;
			}
		}

		static EditorGUI_Internals()
		{
			FieldInfo dragStartValueField = typeof(EditorGUI).GetField("s_DragStartValue", BindingFlags.Static | BindingFlags.NonPublic);
			FieldInfo dragStartValueIntField = typeof(EditorGUI).GetField("s_DragStartIntValue", BindingFlags.Static | BindingFlags.NonPublic);
			if (dragStartValueField != null)
			{
				get_s_DragStartValue = () => (double)dragStartValueField.GetValue(null);
			}
			else
			{
				Debug.LogWarning("Odin: Failed to find UnityEditor.EditorGUI.s_DragStartkFloatFieldFormatStringValue field.");
				get_s_DragStartValue = () => 0.0;
			}
			if (dragStartValueIntField != null)
			{
				get_s_DragStartIntValue = () => (long)dragStartValueIntField.GetValue(null);
			}
			else
			{
				Debug.LogWarning("Odin: Failed to find UnityEditor.EditorGUI.s_DragStartIntValue field.");
				get_s_DragStartIntValue = () => 0L;
			}
			EditorGUI_s_RecycledEditor_Field = typeof(EditorGUI).GetField("s_RecycledEditor", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (EditorGUI_s_RecycledEditor_Field == null)
			{
				EditorGUI_s_RecycledEditor_Property = typeof(EditorGUI).GetProperty("s_RecycledEditor", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (EditorGUI_s_RecycledEditor_Property == null)
				{
					Debug.LogError("Odin could not find a UnityEditor.EditorGUI.s_RecycledEditor field or property in this version of Unity. Text history control features in smart number fields will be impacted, but the rest of Odin will continue to function normally. Please update Odin to the latest version, or if this is the latest version of Odin, please report this issue to the developers.");
				}
			}
		}

		public static bool RecycledEditor_IsEditingControl(int id)
		{
			return ((EditorGUI.RecycledTextEditor)RecycledEditor)?.IsEditingControl(id) ?? false;
		}

		public static string DoTextField(int id, Rect position, string text, GUIStyle style, string allowedLetters, out bool changed, bool reset, bool multiline, bool passwordField)
		{
			return EditorGUI.DoTextField((EditorGUI.RecycledTextEditor)RecycledEditor, id, position, text, style, allowedLetters, out changed, reset, multiline, passwordField);
		}

		public static void DragNumberValue(Rect dragHotZone, int id, bool isDouble, ref double doubleVal, ref long longVal, double dragSensitivity)
		{
			EditorGUI.DragNumberValue(dragHotZone, id, isDouble, ref doubleVal, ref longVal, dragSensitivity);
		}

		public static double GetFloatDragSensitivity()
		{
			return CalculateFloatDragSensitivity(get_s_DragStartValue());
		}

		public static double GetIntDragSensitivity()
		{
			return CalculateIntDragSensitivity(get_s_DragStartIntValue());
		}

		private static double CalculateFloatDragSensitivity(double value)
		{
			if (double.IsInfinity(value) || double.IsNaN(value))
			{
				return 0.0;
			}
			return Math.Max(1.0, Math.Pow(Math.Abs(value), 0.5)) * 0.029999999329447746;
		}

		private static double CalculateIntDragSensitivity(long value)
		{
			if (value == long.MinValue)
			{
				value = long.MaxValue;
			}
			return Math.Max(1.0, Math.Pow(Math.Abs(value), 0.5) * 0.029999999329447746);
		}
	}
}
