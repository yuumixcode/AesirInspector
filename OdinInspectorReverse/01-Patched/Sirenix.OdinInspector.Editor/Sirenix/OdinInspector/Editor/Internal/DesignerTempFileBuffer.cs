namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct DesignerTempFileBuffer
	{
		public char[] Buffer;

		public int Length;

		public char this[int index] => Buffer[index];
	}
}
