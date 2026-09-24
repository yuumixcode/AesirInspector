using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalFloatArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, float> where TArray : IList
	{
		protected override float DrawElement(Rect rect, float value)
		{
			return SirenixEditorFields.FloatField(rect.Padding(2f), value);
		}
	}
}
