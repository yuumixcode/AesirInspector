using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalVector4ArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Vector4> where TArray : IList
	{
		protected override Vector4 DrawElement(Rect rect, Vector4 value)
		{
			return SirenixEditorFields.Vector4Field(rect.Padding(2f), value);
		}
	}
}
