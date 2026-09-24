using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalIntArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, int> where TArray : IList
	{
		protected override int DrawElement(Rect rect, int value)
		{
			return SirenixEditorFields.IntField(rect.Padding(2f), value);
		}
	}
}
