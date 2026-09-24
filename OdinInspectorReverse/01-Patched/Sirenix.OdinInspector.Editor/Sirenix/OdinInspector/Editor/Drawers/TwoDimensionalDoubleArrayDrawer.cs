using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalDoubleArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, double> where TArray : IList
	{
		protected override double DrawElement(Rect rect, double value)
		{
			return SirenixEditorFields.DoubleField(rect.Padding(2f), value);
		}
	}
}
