using System.Collections;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	[DrawerPriority(0.0, 0.0, 0.9)]
	internal class TwoDimensionalGenericArrayDrawer<TArray, TElement> : TwoDimensionalArrayDrawer<TArray, TElement> where TArray : IList
	{
		private static string drawElementErrorMessage = "Odin doesn't know how to draw a table matrix for this particular type. Make a custom DrawElementMethod via the TableMatrix attribute like so:\n\n[TableMatrix(DrawElementMethod = \"DrawElement\")]\npublic " + typeof(TElement).GetNiceName() + "[,] myTable\n\nstatic " + typeof(TElement).GetNiceName() + " DrawElement(Rect rect, " + typeof(TElement).GetNiceName() + " value)\n{\n   // Draw and modify the value in the rect provided using classes such as:\n   // GUI, EditorGUI, SirenixEditorFields and SirenixEditorGUI.\n   return newValue;\n}";

		protected internal override void OnBeforeDrawTable(IPropertyValueEntry<TArray> entry, Context context, GUIContent label)
		{
			if (context.DrawElement == null && context.ExtraErrorMessage == null)
			{
				context.ExtraErrorMessage = drawElementErrorMessage;
			}
		}

		/// <summary>
		/// Draws the element.
		/// </summary>
		protected override TElement DrawElement(Rect rect, TElement value)
		{
			return value;
		}
	}
}
