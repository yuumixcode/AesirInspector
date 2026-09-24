using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalQuaternionArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Quaternion> where TArray : IList
	{
		protected override Quaternion DrawElement(Rect rect, Quaternion value)
		{
			return SirenixEditorFields.RotationField(rect.Padding(2f), value, QuaternionDrawMode.Eulers);
		}
	}
}
