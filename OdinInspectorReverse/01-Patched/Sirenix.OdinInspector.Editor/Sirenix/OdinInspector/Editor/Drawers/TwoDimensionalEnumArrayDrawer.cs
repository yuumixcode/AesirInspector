using System;
using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalEnumArrayDrawer<TArray, TElement> : TwoDimensionalArrayDrawer<TArray, TElement> where TArray : IList
	{
		protected override TElement DrawElement(Rect rect, TElement value)
		{
			return (TElement)(object)SirenixEditorFields.EnumDropdown(rect.Padding(4f), null, (Enum)(object)value, null);
		}
	}
}
