using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalLongArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, long> where TArray : IList
	{
		protected override long DrawElement(Rect rect, long value)
		{
			return SirenixEditorFields.LongField(rect.Padding(2f), value);
		}
	}
}
