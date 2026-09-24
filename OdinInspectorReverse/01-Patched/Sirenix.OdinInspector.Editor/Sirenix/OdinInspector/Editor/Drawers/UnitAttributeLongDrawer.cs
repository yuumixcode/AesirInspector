using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class UnitAttributeLongDrawer : UnitAttributeDrawer<long>
	{
		protected override long DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, long value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return SirenixEditorFields.SmartLongUnitField(in fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
		}
	}
}
