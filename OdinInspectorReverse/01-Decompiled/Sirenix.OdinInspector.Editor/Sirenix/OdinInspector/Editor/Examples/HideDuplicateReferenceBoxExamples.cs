using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(HideDuplicateReferenceBoxAttribute), "Indicates that Odin should hide the reference box, if this property would otherwise be drawn as a reference to another property, due to duplicate reference values being encountered.")]
	[ShowOdinSerializedPropertiesInInspector]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.Utilities.Editor" })]
	internal class HideDuplicateReferenceBoxExamples
	{
		public class ReferenceTypeClass
		{
			[HideDuplicateReferenceBox]
			public ReferenceTypeClass recursiveReference;

			[OnInspectorGUI]
			[PropertyOrder(-1f)]
			private void MessageBox()
			{
				SirenixEditorGUI.WarningMessageBox("Recursively drawn references will always show the reference box regardless, to prevent infinite depth draw loops.");
			}
		}

		[PropertyOrder(1f)]
		public ReferenceTypeClass firstObject;

		[PropertyOrder(3f)]
		public ReferenceTypeClass withReferenceBox;

		[HideDuplicateReferenceBox]
		[PropertyOrder(5f)]
		public ReferenceTypeClass withoutReferenceBox;

		[OnInspectorInit]
		public void CreateData()
		{
			firstObject = new ReferenceTypeClass();
			withReferenceBox = firstObject;
			withoutReferenceBox = firstObject;
			firstObject.recursiveReference = firstObject;
		}

		[OnInspectorGUI]
		[PropertyOrder(0f)]
		private void MessageBox1()
		{
			SirenixEditorGUI.Title("The first reference will always be drawn normally", null, TextAlignment.Left, horizontalLine: true);
		}

		[OnInspectorGUI]
		[PropertyOrder(2f)]
		private void MessageBox2()
		{
			GUILayout.Space(20f);
			SirenixEditorGUI.Title("All subsequent references will be wrapped in a reference box", null, TextAlignment.Left, horizontalLine: true);
		}

		[PropertyOrder(4f)]
		[OnInspectorGUI]
		private void MessageBox3()
		{
			GUILayout.Space(20f);
			SirenixEditorGUI.Title("With the [HideDuplicateReferenceBox] attribute, this box is hidden", null, TextAlignment.Left, horizontalLine: true);
		}
	}
}
