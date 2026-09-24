using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(GUIColorAttribute))]
	internal class GUIColorExamples
	{
		[GUIColor(0.3f, 0.8f, 0.8f, 1f)]
		public int ColoredInt1;

		[GUIColor(0.3f, 0.8f, 0.8f, 1f)]
		public int ColoredInt2;

		[GUIColor("#FF0000")]
		public int Hex1;

		[GUIColor("#FF000077")]
		public int Hex2;

		[GUIColor("RGB(0, 1, 0)")]
		public int Rgb;

		[GUIColor("RGBA(0, 1, 0, 0.5)")]
		public int Rgba;

		[GUIColor("orange")]
		public int NamedColors;

		[ButtonGroup("_DefaultGroup", 0f)]
		[GUIColor(0f, 1f, 0f, 1f)]
		private void Apply()
		{
		}

		[ButtonGroup("_DefaultGroup", 0f)]
		[GUIColor(1f, 0.6f, 0.4f, 1f)]
		private void Cancel()
		{
		}

		[GUIColor("GetButtonColor")]
		[Button("I Am Fabulous", ButtonSizes.Gigantic)]
		[InfoBox("You can also reference a color member to dynamically change the color of a property.", InfoMessageType.Info, null)]
		private static void IAmFabulous()
		{
		}

		[Button(ButtonSizes.Large)]
		[GUIColor("@Color.Lerp(Color.red, Color.green, Mathf.Abs(Mathf.Sin((float)EditorApplication.timeSinceStartup)))")]
		private static void Expressive()
		{
		}

		private static Color GetButtonColor()
		{
			GUIHelper.RequestRepaint();
			return Color.HSVToRGB(Mathf.Cos((float)EditorApplication.timeSinceStartup + 1f) * 0.225f + 0.325f, 1f, 1f);
		}
	}
}
