using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class UnitAttributeDoubleDrawer : UnitAttributeDrawer<double>
	{
		protected override double DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, double value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SirenixEditorFields.SmartDoubleUnitField(in fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
		}
	}
}
