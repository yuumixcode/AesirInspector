using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal struct DropZone
	{
		public DropDirection DropDirection;

		public Rect Rect;

		public DropZone(DropDirection dropDirection, Rect rect)
		{
			DropDirection = dropDirection;
			Rect = rect;
		}
	}
}
