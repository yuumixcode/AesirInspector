using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalVector2ArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Vector2> where TArray : IList
	{
		protected override Vector2 DrawElement(Rect rect, Vector2 value)
		{
			return SirenixEditorFields.Vector2Field(rect.Padding(2f), value);
		}
	}
}
