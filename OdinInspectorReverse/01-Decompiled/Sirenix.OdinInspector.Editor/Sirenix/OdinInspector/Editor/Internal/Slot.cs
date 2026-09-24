using System.Collections.Generic;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	internal class Slot
	{
		public DesignerEditorNode Node;

		public int ZIndex;

		public int IndentLevel;

		public int Index;

		public Rect Rect;

		public List<DropZone> DropZonesRelative;
	}
}
