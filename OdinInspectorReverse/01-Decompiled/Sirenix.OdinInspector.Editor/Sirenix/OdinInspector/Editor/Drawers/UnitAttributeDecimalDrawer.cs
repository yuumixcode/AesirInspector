using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class UnitAttributeDecimalDrawer : UnitAttributeDrawer<decimal>
	{
		protected override decimal DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, decimal value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SirenixEditorFields.SmartDecimalUnitField(in fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
		}
	}
}
