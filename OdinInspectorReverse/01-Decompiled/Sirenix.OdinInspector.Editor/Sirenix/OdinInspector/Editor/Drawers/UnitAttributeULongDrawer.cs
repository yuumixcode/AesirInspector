using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class UnitAttributeULongDrawer : UnitAttributeDrawer<ulong>
	{
		protected override ulong DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, ulong value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return (ulong)SirenixEditorFields.SmartLongUnitField(in fieldExpressionContext, label, (long)value, baseUnitInfo, displayUnitInfo);
		}
	}
}
