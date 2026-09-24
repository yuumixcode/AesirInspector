using System;
using System.Text;
using Sirenix.Reflection.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public static class SirenixObjectPickerUtilities
	{
		private static readonly Type ObjectSelectorWindowType;

		static SirenixObjectPickerUtilities()
		{
			ObjectSelectorWindowType = TwoWaySerializationBinder.Default.BindToType("UnityEditor.ObjectSelector, UnityEditor");
		}

		public static string GetSearchFilterForPolymorphicType(Type type)
		{
			if (type == typeof(object))
			{
				return string.Empty;
			}
			StringBuilder buffer = new StringBuilder();
			TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(type);
			for (int i = 0; i < types.Count; i++)
			{
				if (types[i].IsSubclassOf(typeof(UnityEngine.Object)))
				{
					buffer.Append('t');
					buffer.Append(':');
					buffer.Append(types[i].Name);
					buffer.Append(' ');
				}
			}
			return buffer.ToString();
		}

		public static void MoveCaretToEndOfSearchFilter()
		{
			if (ObjectSelectorWindowType == null)
			{
				return;
			}
			UnityEngine.Object[] selectorWindows = Resources.FindObjectsOfTypeAll(ObjectSelectorWindowType);
			if (selectorWindows.Length == 0)
			{
				return;
			}
			UnityEngine.Object window = selectorWindows[0];
			EditorApplication.delayCall = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.delayCall, (EditorApplication.CallbackFunction)delegate
			{
				if (!(EditorWindow.focusedWindow != window))
				{
					TextEditor recycledEditor = EditorGUI_Internals.RecycledEditor;
					int selectIndex = (recycledEditor.cursorIndex = recycledEditor.text.Length);
					recycledEditor.selectIndex = selectIndex;
				}
			});
		}
	}
}
