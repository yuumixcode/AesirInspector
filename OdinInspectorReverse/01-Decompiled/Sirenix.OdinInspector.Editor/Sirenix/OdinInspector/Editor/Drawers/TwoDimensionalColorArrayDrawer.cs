using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalColorArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Color> where TArray : IList
	{
		protected override Color DrawElement(Rect rect, Color value)
		{
			return SirenixEditorFields.ColorField(rect.Padding(2f), value);
		}
	}
}
