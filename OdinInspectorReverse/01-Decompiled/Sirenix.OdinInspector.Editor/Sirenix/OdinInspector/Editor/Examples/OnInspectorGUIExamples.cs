using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	[AttributeExample(typeof(OnInspectorGUIAttribute))]
	internal class OnInspectorGUIExamples
	{
		[OnInspectorInit("@Texture = Sirenix.OdinInspector.Editor.Examples.ExampleHelper.GetTexture(220, 80, Sirenix.OdinInspector.Editor.Examples.ExampleTextureTheme.Warm)")]
		[OnInspectorGUI("DrawPreview", true)]
		public Texture2D Texture;

		private void DrawPreview()
		{
			if (!(Texture == null))
			{
				GUILayout.BeginVertical(GUI.skin.box);
				GUILayout.Label(Texture);
				GUILayout.EndVertical();
			}
		}

		[OnInspectorGUI]
		private void OnInspectorGUI()
		{
			EditorGUILayout.HelpBox("OnInspectorGUI can also be used on both methods and properties", MessageType.Info);
		}
	}
}
