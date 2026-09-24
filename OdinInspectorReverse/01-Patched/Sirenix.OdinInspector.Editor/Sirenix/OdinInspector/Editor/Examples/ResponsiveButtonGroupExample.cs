using Sirenix.OdinInspector.Editor.Examples.Internal;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(ResponsiveButtonGroupAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
	internal class ResponsiveButtonGroupExample
	{
		[GUIColor(0f, 1f, 0f, 1f)]
		[Button(ButtonSizes.Large)]
		private void OpenDockableWindowExample()
		{
			MyDockableGameDashboard window = EditorWindow.GetWindow<MyDockableGameDashboard>();
			window.WindowPadding = default(Vector4);
		}

		[OnInspectorGUI]
		private void Space1()
		{
			GUILayout.Space(20f);
		}

		[ResponsiveButtonGroup("_DefaultResponsiveButtonGroup")]
		public void Foo()
		{
		}

		[ResponsiveButtonGroup("_DefaultResponsiveButtonGroup")]
		public void Bar()
		{
		}

		[ResponsiveButtonGroup("_DefaultResponsiveButtonGroup")]
		public void Baz()
		{
		}

		[OnInspectorGUI]
		private void Space2()
		{
			GUILayout.Space(20f);
		}

		[ResponsiveButtonGroup("UniformGroup", UniformLayout = true)]
		public void Foo1()
		{
		}

		[ResponsiveButtonGroup("UniformGroup")]
		public void Foo2()
		{
		}

		[ResponsiveButtonGroup("UniformGroup")]
		public void LongesNameWins()
		{
		}

		[ResponsiveButtonGroup("UniformGroup")]
		public void Foo4()
		{
		}

		[ResponsiveButtonGroup("UniformGroup")]
		public void Foo5()
		{
		}

		[ResponsiveButtonGroup("UniformGroup")]
		public void Foo6()
		{
		}

		[OnInspectorGUI]
		private void Space3()
		{
			GUILayout.Space(20f);
		}

		[ResponsiveButtonGroup("DefaultButtonSize", DefaultButtonSize = ButtonSizes.Small)]
		public void Bar1()
		{
		}

		[ResponsiveButtonGroup("DefaultButtonSize")]
		public void Bar2()
		{
		}

		[ResponsiveButtonGroup("DefaultButtonSize")]
		public void Bar3()
		{
		}

		[Button(ButtonSizes.Large)]
		[ResponsiveButtonGroup("DefaultButtonSize")]
		public void Bar4()
		{
		}

		[Button(ButtonSizes.Large)]
		[ResponsiveButtonGroup("DefaultButtonSize")]
		public void Bar5()
		{
		}

		[ResponsiveButtonGroup("DefaultButtonSize")]
		public void Bar6()
		{
		}

		[OnInspectorGUI]
		private void Space4()
		{
			GUILayout.Space(20f);
		}

		[ResponsiveButtonGroup("SomeOtherGroup/SomeBtnGroup")]
		[FoldoutGroup("SomeOtherGroup", 0f)]
		public void Baz1()
		{
		}

		[ResponsiveButtonGroup("SomeOtherGroup/SomeBtnGroup")]
		public void Baz2()
		{
		}

		[ResponsiveButtonGroup("SomeOtherGroup/SomeBtnGroup")]
		public void Baz3()
		{
		}
	}
}
