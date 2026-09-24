using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	public class MyInlineScriptableObject : ScriptableObject
	{
		[ShowInInlineEditors]
		public string ShownInInlineEditor;

		[HideInInlineEditors]
		public string HiddenInInlineEditor;
	}
}
