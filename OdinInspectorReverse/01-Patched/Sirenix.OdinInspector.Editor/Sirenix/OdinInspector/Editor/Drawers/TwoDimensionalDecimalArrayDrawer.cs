using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalDecimalArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, decimal> where TArray : IList
	{
		protected override decimal DrawElement(Rect rect, decimal value)
		{
			return SirenixEditorFields.DecimalField(rect.Padding(2f), value);
		}
	}
}
