using UnityEditor;
using UnityEngine;

namespace Sirenix.Reflection.Editor
{
	public static class GUIView_Internals
	{
		public static class Current
		{
			public static bool HasFocus => UnityEditor.GUIView.current.hasFocus;

			public static int GetInstanceId()
			{
				return UnityEditor.GUIView.current.GetInstanceID();
			}
		}

		public static Object CurrentAsObject => UnityEditor.GUIView.current;
	}
}
