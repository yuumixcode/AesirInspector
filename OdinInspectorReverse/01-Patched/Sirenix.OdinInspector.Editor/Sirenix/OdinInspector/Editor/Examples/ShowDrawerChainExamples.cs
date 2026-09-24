using System;
using Sirenix.OdinInspector.Editor.Examples.Internal;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[ExampleAsComponentData(Namespaces = new string[] { "System" })]
	[AttributeExample(typeof(ShowDrawerChainAttribute))]
	internal class ShowDrawerChainExamples
	{
		[HideIf("ToggleHideIf", true)]
		[InfoBox("Any drawer not used will be greyed out so that you can more easily debug the drawer chain. You can see this by toggling the above toggle field.\n\nIf you have any custom drawers they will show up with green names in the drawer chain.", InfoMessageType.Info, null)]
		[PropertyOrder(2f)]
		[ShowDrawerChain]
		public GameObject SomeObject;

		[Range(0f, 10f)]
		[ShowDrawerChain]
		public float SomeRange;

		[HorizontalGroup(0f, 0, 0, 0f, Order = 1f)]
		[ShowInInspector]
		[ToggleLeft]
		public bool ToggleHideIf
		{
			get
			{
				GUIHelper.RequestRepaint();
				return EditorApplication.timeSinceStartup % 3.0 < 1.5;
			}
		}

		[ProgressBar(0.0, 1.5, 0.15f, 0.47f, 0.74f)]
		[HideLabel]
		[ShowInInspector]
		[HorizontalGroup(0f, 0, 0, 0f)]
		private double Animate => Math.Abs(EditorApplication.timeSinceStartup % 3.0 - 1.5);
	}
}
