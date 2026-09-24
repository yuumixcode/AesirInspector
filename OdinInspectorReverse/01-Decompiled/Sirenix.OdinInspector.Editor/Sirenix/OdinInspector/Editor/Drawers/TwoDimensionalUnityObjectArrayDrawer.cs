using System.Collections;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	internal class TwoDimensionalUnityObjectArrayDrawer<TArray, TElement> : TwoDimensionalArrayDrawer<TArray, TElement> where TArray : IList where TElement : Object
	{
		private bool isAssetOnly;

		protected override void Initialize()
		{
			isAssetOnly = base.ValueEntry.Property.GetAttribute<AssetsOnlyAttribute>() != null;
		}

		protected override TElement DrawElement(Rect rect, TElement value)
		{
			bool editable = !base.TableMatrixAttribute.IsReadOnly;
			value = SirenixEditorFields.PreviewObjectField(rect, value, dragOnly: false, editable, editable, !isAssetOnly);
			return value;
		}

		protected override bool CompareElement(TElement a, TElement b)
		{
			return a == b;
		}
	}
}
