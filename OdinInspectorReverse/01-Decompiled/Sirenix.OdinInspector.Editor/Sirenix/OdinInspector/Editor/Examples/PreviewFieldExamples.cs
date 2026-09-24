using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(PreviewFieldAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class PreviewFieldExamples
	{
		[PreviewField]
		public Object RegularPreviewField;

		[VerticalGroup("row1/left", 0f)]
		public string A;

		[VerticalGroup("row1/left", 0f)]
		public string B;

		[VerticalGroup("row1/left", 0f)]
		public string C;

		[HideLabel]
		[PreviewField(50f, ObjectFieldAlignment.Right)]
		[HorizontalGroup("row1", 50f, 0, 0, 0f)]
		[VerticalGroup("row1/right", 0f)]
		public Object D;

		[HideLabel]
		[HorizontalGroup("row2", 50f, 0, 0, 0f)]
		[PreviewField(50f, ObjectFieldAlignment.Left)]
		[VerticalGroup("row2/left", 0f)]
		public Object E;

		[VerticalGroup("row2/right", 0f)]
		[LabelWidth(-54f)]
		public string F;

		[VerticalGroup("row2/right", 0f)]
		[LabelWidth(-54f)]
		public string G;

		[LabelWidth(-54f)]
		[VerticalGroup("row2/right", 0f)]
		public string H;

		[PreviewField("preview", FilterMode.Bilinear)]
		public Object I;

		private Texture preview;

		[OnInspectorInit]
		private void CreateData()
		{
			RegularPreviewField = ExampleHelper.GetTexture(0);
			D = ExampleHelper.GetTexture(1);
			E = ExampleHelper.GetTexture(2);
			I = ExampleHelper.GetMesh();
			preview = ExampleHelper.GetTexture(3);
		}

		[PropertyOrder(-1f)]
		[InfoBox("These object fields can also be selectively enabled and customized globally from the Odin preferences window.\n\n - Hold Ctrl + Click = Delete Instance\n - Drag and drop = Move / Swap.\n - Ctrl + Drag = Replace.\n - Ctrl + drag and drop = Move and override.", InfoMessageType.Info, null)]
		[Button(ButtonSizes.Large)]
		private void ConfigureGlobalPreviewFieldSettings()
		{
			GlobalConfig<GeneralDrawerConfig>.Instance.OpenInEditor();
		}
	}
}
