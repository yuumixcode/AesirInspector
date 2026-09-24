using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalVector3ArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Vector3> where TArray : IList
	{
		protected override Vector3 DrawElement(Rect rect, Vector3 value)
		{
			return SirenixEditorFields.Vector3Field(rect.Padding(2f), value);
		}
	}
}
