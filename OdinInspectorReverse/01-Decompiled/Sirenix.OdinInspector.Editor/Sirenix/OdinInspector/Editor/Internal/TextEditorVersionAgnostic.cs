using System;
using System.Reflection;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal static class TextEditorVersionAgnostic
	{
		private static readonly Action<TextEditor, bool> SetMultilineImpl = CreateSetMultiline();

		public static void SetMultiline(TextEditor editor, bool value)
		{
			if (editor != null)
			{
				SetMultilineImpl(editor, value);
			}
		}

		private static Action<TextEditor, bool> CreateSetMultiline()
		{
			Type t = typeof(TextEditor);
			PropertyInfo isMultilineProp = t.GetProperty("isMultiline", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (isMultilineProp != null && isMultilineProp.CanWrite && isMultilineProp.PropertyType == typeof(bool))
			{
				return delegate(TextEditor te, bool v)
				{
					isMultilineProp.SetValue(te, v, null);
				};
			}
			FieldInfo multilineField = t.GetField("multiline", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (multilineField != null && multilineField.FieldType == typeof(bool))
			{
				return delegate(TextEditor te, bool v)
				{
					multilineField.SetValue(te, v);
				};
			}
			return delegate
			{
			};
		}
	}
}
