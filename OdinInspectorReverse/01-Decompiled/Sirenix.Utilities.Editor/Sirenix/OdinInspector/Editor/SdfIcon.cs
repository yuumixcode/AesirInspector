using System;

namespace Sirenix.OdinInspector.Editor
{
	public struct SdfIcon
	{
		public int Index;

		public char Char;

		public int Width;

		public int Height;

		public SdfUvRect Uv;

		public float EdgeOffset;

		public string Name => Enum.GetName(typeof(SdfIconType), Index);

		public SdfIcon(int index, char @char, int width, int height, SdfUvRect uv, float edgeOffset = 0f)
		{
			Index = index;
			Char = @char;
			Width = width;
			Height = height;
			Uv = uv;
			EdgeOffset = edgeOffset;
		}
	}
}
