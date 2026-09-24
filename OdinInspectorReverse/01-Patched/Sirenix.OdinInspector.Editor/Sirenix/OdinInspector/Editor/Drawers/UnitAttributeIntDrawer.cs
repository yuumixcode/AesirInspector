using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class UnitAttributeIntDrawer : UnitAttributeDrawer<int>
	{
		protected override int DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, int value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SirenixEditorFields.SmartIntUnitField(in fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
		}
	}
}
