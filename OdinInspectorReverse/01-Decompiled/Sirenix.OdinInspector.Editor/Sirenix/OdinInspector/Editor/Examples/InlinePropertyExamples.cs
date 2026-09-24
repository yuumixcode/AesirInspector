using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(InlinePropertyAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "System" })]
	internal class InlinePropertyExamples
	{
		[Serializable]
		[InlineProperty(LabelWidth = 13)]
		public struct Vector3Int
		{
			[HorizontalGroup(0f, 0, 0, 0f)]
			public int X;

			[HorizontalGroup(0f, 0, 0, 0f)]
			public int Y;

			[HorizontalGroup(0f, 0, 0, 0f)]
			public int Z;
		}

		[Serializable]
		public struct Vector2Int
		{
			[HorizontalGroup(0f, 0, 0, 0f)]
			public int X;

			[HorizontalGroup(0f, 0, 0, 0f)]
			public int Y;
		}

		public Vector3 Vector3;

		public Vector3Int MyVector3Int;

		[InlineProperty(LabelWidth = 13)]
		public Vector2Int MyVector2Int;
	}
}
