using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	[AttributeExample(typeof(DisableInInlineEditorsAttribute))]
	internal class DisableInInlineEditorExample
	{
		[InfoBox("Click the pen icon to open a new inspector window for the InlineObject too see the difference this attribute makes.", InfoMessageType.Info, null)]
		[InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.Boxed, Expanded = true)]
		public DisabledInInlineEditorScriptableObject InlineObject;

		[OnInspectorInit]
		private void CreateData()
		{
			InlineObject = ExampleHelper.GetScriptableObject<DisabledInInlineEditorScriptableObject>("Inline Object");
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			if (InlineObject != null)
			{
				Object.DestroyImmediate(InlineObject);
			}
		}
	}
}
