using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	public class DisabledInInlineEditorScriptableObject : ScriptableObject
	{
		public string AlwaysEnabled;

		[DisableInInlineEditors]
		public string DisabledInInlineEditor;
	}
}
