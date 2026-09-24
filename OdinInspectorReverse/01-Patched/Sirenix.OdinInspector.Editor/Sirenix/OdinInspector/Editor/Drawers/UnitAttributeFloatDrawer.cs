using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class UnitAttributeFloatDrawer : UnitAttributeDrawer<float>
	{
		protected override float DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, float value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SirenixEditorFields.SmartFloatUnitField(in fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
		}
	}
}
