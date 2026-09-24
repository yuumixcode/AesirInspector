using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalLayerMaskArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, LayerMask> where TArray : IList
	{
		protected override LayerMask DrawElement(Rect rect, LayerMask value)
		{
			return SirenixEditorFields.LayerMaskField(rect.Padding(2f), value);
		}
	}
}
