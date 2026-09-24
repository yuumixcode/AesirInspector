using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	public class UnitAttributeUIntDrawer : UnitAttributeDrawer<uint>
	{
		protected override uint DrawField(FieldExpressionContext fieldExpressionContext, GUIContent label, uint value, UnitInfo baseUnitInfo, UnitInfo displayUnitInfo)
		{
			return (uint)SirenixEditorFields.SmartLongUnitField(in fieldExpressionContext, label, value, baseUnitInfo, displayUnitInfo);
		}
	}
}
