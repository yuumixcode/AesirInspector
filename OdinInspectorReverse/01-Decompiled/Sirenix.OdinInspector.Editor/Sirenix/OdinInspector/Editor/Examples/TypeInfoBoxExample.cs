using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(TypeInfoBoxAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System", "Sirenix.OdinInspector.Editor.Examples" })]
	internal class TypeInfoBoxExample
	{
		[Serializable]
		[TypeInfoBox("The TypeInfoBox attribute can be put on type definitions and will result in an InfoBox being drawn at the top of a property.")]
		public class MyType
		{
			public int Value;
		}

		public MyType MyObject = new MyType();

		[InfoBox("Click the pen icon to open a new inspector for the Scripty object.", InfoMessageType.Info, null)]
		[InlineEditor(InlineEditorModes.GUIOnly, InlineEditorObjectFieldModes.Boxed)]
		public MyScriptyScriptableObject Scripty;

		[OnInspectorInit]
		private void CreateData()
		{
			Scripty = ExampleHelper.GetScriptableObject<MyScriptyScriptableObject>("Scripty");
		}

		[OnInspectorDispose]
		private void CleanupData()
		{
			if (Scripty != null)
			{
				UnityEngine.Object.DestroyImmediate(Scripty);
			}
		}
	}
}
