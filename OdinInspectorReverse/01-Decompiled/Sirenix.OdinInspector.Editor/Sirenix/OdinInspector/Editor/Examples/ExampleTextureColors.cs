using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
	internal struct ExampleTextureColors
	{
		public readonly Color ColorA;

		public readonly Color ColorB;

		public readonly Color Accent;

		public readonly Color Shadow;

		public ExampleTextureColors(Color colorA, Color colorB, Color accent, Color shadow)
		{
			ColorA = colorA;
			ColorB = colorB;
			Accent = accent;
			Shadow = shadow;
		}
	}
}
