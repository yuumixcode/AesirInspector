using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(OnValueChangedAttribute), "OnValueChanged is used here to create a material for a shader, when the shader is changed.")]
	internal class OnValueChangedExamples
	{
		[OnValueChanged("CreateMaterial", false)]
		public Shader Shader;

		[ReadOnly]
		[InlineEditor(InlineEditorModes.LargePreview, InlineEditorObjectFieldModes.Boxed)]
		public Material Material;

		private void CreateMaterial()
		{
			if (Material != null)
			{
				Object.DestroyImmediate(Material);
			}
			if (Shader != null)
			{
				Material = new Material(Shader);
			}
		}
	}
}
