using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "System", "System.Collections.Generic", "Sirenix.OdinInspector.Editor.Examples" })]
	[AttributeExample(typeof(ShowInInspectorAttribute), "You can use the ShowInInspector attribute on static members to make them appear in the inspector as well.")]
	internal class StaticInspectorsExample
	{
		[Serializable]
		public struct MySomeStruct
		{
			[HideLabel]
			[PreviewField(45f)]
			[HorizontalGroup("Split", 45f, 0, 0, 0f)]
			public Texture2D Icon;

			[HorizontalGroup("Split/$Icon/Properties", 0f, 0, 0, 0f, LabelWidth = 40f)]
			[FoldoutGroup("Split/$Icon", 0f)]
			public int Foo;

			[HorizontalGroup("Split/$Icon/Properties", 0f, 0, 0, 0f)]
			public int Bar;
		}

		[ShowInInspector]
		public static List<MySomeStruct> SomeStaticField;

		[ShowInInspector]
		[PropertyRange(0.0, 0.10000000149011612)]
		public static float FixedDeltaTime
		{
			get
			{
				return Time.fixedDeltaTime;
			}
			set
			{
				Time.fixedDeltaTime = value;
			}
		}

		[Button(ButtonSizes.Large)]
		[PropertyOrder(-1f)]
		public static void AddToList()
		{
			int count = SomeStaticField.Count + 1000;
			SomeStaticField.Capacity = count;
			while (SomeStaticField.Count < count)
			{
				SomeStaticField.Add(new MySomeStruct
				{
					Icon = ExampleHelper.GetTexture(SomeStaticField.Count % 8)
				});
			}
		}

		[OnInspectorInit]
		private static void CreateData()
		{
			SomeStaticField = new List<MySomeStruct>
			{
				new MySomeStruct
				{
					Icon = ExampleHelper.GetTexture(0)
				},
				new MySomeStruct
				{
					Icon = ExampleHelper.GetTexture(1)
				},
				new MySomeStruct
				{
					Icon = ExampleHelper.GetTexture(2)
				}
			};
		}
	}
}
