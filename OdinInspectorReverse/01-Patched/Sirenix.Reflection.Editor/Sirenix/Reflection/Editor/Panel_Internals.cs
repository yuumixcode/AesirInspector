using UnityEngine;
using UnityEngine.UIElements;

namespace Sirenix.Reflection.Editor
{
	public static class Panel_Internals
	{
		public static Panel_Internal CreateEditorPanel(ScriptableObject owner)
		{
			return new Panel_Internal(UnityEngine.UIElements.Panel.CreateEditorPanel(owner));
		}
	}
}
