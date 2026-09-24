using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "System" })]
	[AttributeExample(typeof(BoxGroupAttribute))]
	internal class BoxGroupExamples
	{
		[Serializable]
		public struct SomeStruct
		{
			public int One;

			public int Two;

			public int Three;
		}

		[BoxGroup("Some Title", true, false, 0f)]
		public string A;

		[BoxGroup("Some Title", true, false, 0f)]
		public string B;

		[BoxGroup("Centered Title", true, true, 0f)]
		public string C;

		[BoxGroup("Centered Title", true, false, 0f)]
		public string D;

		[BoxGroup("$G", true, false, 0f)]
		public string E = "Dynamic box title 2";

		[BoxGroup("$G", true, false, 0f)]
		public string F;

		[BoxGroup]
		public string G;

		[BoxGroup]
		public string H;

		[BoxGroup("NoTitle", false, false, 0f)]
		public string I;

		[BoxGroup("NoTitle", true, false, 0f)]
		public string J;

		[BoxGroup("A Struct In A Box", true, false, 0f)]
		[HideLabel]
		public SomeStruct BoxedStruct;

		public SomeStruct DefaultStruct;
	}
}
