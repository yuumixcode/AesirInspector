using System;
using System.Collections;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalGuidArrayDrawer<TArray> : TwoDimensionalArrayDrawer<TArray, Guid> where TArray : IList
	{
		protected override Guid DrawElement(Rect rect, Guid value)
		{
			return SirenixEditorFields.GuidField(rect.Padding(2f), value);
		}
	}
}
