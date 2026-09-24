using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(InlineEditorAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class InlineEditorExamples
	{
		[InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.Boxed)]
		public ExampleTransform InlineComponent;

		[InlineEditor(InlineEditorModes.FullEditor, InlineEditorObjectFieldModes.Boxed)]
		public Material FullInlineEditor;

		[InlineEditor(InlineEditorModes.GUIAndHeader, InlineEditorObjectFieldModes.Boxed)]
		public Material InlineMaterial;

		[InlineEditor(InlineEditorModes.SmallPreview, InlineEditorObjectFieldModes.Boxed)]
		public Material[] InlineMaterialList;

		[InlineEditor(InlineEditorModes.LargePreview, InlineEditorObjectFieldModes.Boxed)]
		public Mesh InlineMeshPreview;

		[OnInspectorInit]
		private void CreateData()
		{
			InlineComponent = ExampleHelper.GetScriptableObject<ExampleTransform>("Inline Component");
			FullInlineEditor = ExampleHelper.GetMaterial();
			InlineMaterial = ExampleHelper.GetMaterial();
			InlineMaterialList = new Material[3]
			{
				ExampleHelper.GetMaterial(),
				ExampleHelper.GetMaterial(),
				ExampleHelper.GetMaterial()
			};
			InlineMeshPreview = ExampleHelper.GetMesh();
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			if (InlineComponent != null)
			{
				Object.DestroyImmediate(InlineComponent);
			}
		}
	}
}
