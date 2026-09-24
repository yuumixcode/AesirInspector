using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "System" })]
	[AttributeExample(typeof(ToggleGroupAttribute))]
	internal class ToggleGroupExamples
	{
		[Serializable]
		public class MyToggleObject
		{
			public bool Enabled;

			[HideInInspector]
			public string Title;

			public int A;

			public int B;
		}

		[Serializable]
		public class MyToggleA : MyToggleObject
		{
			public float C;

			public float D;

			public float F;
		}

		[Serializable]
		public class MyToggleB : MyToggleObject
		{
			public string Text;
		}

		[Serializable]
		public class MyToggleC
		{
			[ToggleGroup("Enabled", "$Label")]
			public bool Enabled;

			[ToggleGroup("Enabled", 0f, null)]
			public float Test;

			public string Label => Test.ToString();
		}

		[ToggleGroup("MyToggle", 0f, null)]
		public bool MyToggle;

		[ToggleGroup("MyToggle", 0f, null)]
		public float A;

		[HideLabel]
		[ToggleGroup("MyToggle", 0f, null)]
		[Multiline]
		public string B;

		[ToggleGroup("EnableGroupOne", "$GroupOneTitle")]
		public bool EnableGroupOne = true;

		[ToggleGroup("EnableGroupOne", 0f, null)]
		public string GroupOneTitle = "One";

		[ToggleGroup("EnableGroupOne", 0f, null)]
		public float GroupOneA;

		[ToggleGroup("EnableGroupOne", 0f, null)]
		public float GroupOneB;

		[Toggle("Enabled")]
		public MyToggleObject Three = new MyToggleObject();

		[Toggle("Enabled")]
		public MyToggleA Four = new MyToggleA();

		[Toggle("Enabled")]
		public MyToggleB Five = new MyToggleB();

		public MyToggleC[] ToggleList = new MyToggleC[3]
		{
			new MyToggleC
			{
				Test = 2f,
				Enabled = true
			},
			new MyToggleC
			{
				Test = 5f
			},
			new MyToggleC
			{
				Test = 7f
			}
		};
	}
}
