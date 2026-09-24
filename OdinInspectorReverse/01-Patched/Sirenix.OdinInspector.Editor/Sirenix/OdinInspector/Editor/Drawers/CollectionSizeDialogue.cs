using System;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class CollectionSizeDialogue
	{
		public int Size;

		private Action<int> confirm;

		private Action cancel;

		public CollectionSizeDialogue(Action<int> confirm, Action cancel, int size)
		{
			this.confirm = confirm;
			this.cancel = cancel;
			Size = size;
		}

		[HorizontalGroup(0.5f, 0, 0, 0f)]
		[Button(ButtonSizes.Medium)]
		public void Confirm()
		{
			confirm(Size);
		}

		[Button(ButtonSizes.Medium)]
		[HorizontalGroup(0f, 0, 0, 0f)]
		public void Cancel()
		{
			cancel();
		}
	}
}
