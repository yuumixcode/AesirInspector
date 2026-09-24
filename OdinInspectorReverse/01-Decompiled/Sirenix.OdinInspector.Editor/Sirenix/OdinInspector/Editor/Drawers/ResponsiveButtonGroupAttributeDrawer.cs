using System.Linq;
using System.Reflection;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for the ResponsiveButtonGroupAttribute.
	/// </summary>
	public class ResponsiveButtonGroupAttributeDrawer : OdinGroupDrawer<ResponsiveButtonGroupAttribute>
	{
		private Vector2[] btnSizes;

		private int[] colCounts;

		private int prevWidth = 400;

		private bool isFirstFrame = true;

		private int innerWidth;

		/// <summary>
		/// Draws the property with GUILayout support.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			InspectorProperty property = base.Property;
			ResponsiveButtonGroupAttribute attribute = base.Attribute;
			if (btnSizes == null || btnSizes.Length != property.Children.Count)
			{
				colCounts = null;
				prevWidth = 400;
				btnSizes = new Vector2[property.Children.Count];
				for (int i = 0; i < btnSizes.Length; i++)
				{
					InspectorProperty prop = property.Children[i].FindChild((InspectorProperty inspectorProperty) => inspectorProperty.Info.GetMemberInfo() is MethodInfo, includeSelf: true) ?? property.Children[i];
					ButtonAttribute btnAttr = prop.GetAttribute<ButtonAttribute>();
					int size = btnAttr?.ButtonHeight ?? ((int)attribute.DefaultButtonSize);
					if (btnAttr == null)
					{
						prop.Context.GetGlobal("ButtonHeight", 0).Value = (int)attribute.DefaultButtonSize;
					}
					size = (int)SirenixGUIStyles.Button.CalcSize(prop.Label).x;
					btnSizes[i] = new Vector2(size, btnAttr?.ButtonHeight ?? ((int)attribute.DefaultButtonSize));
				}
				if (attribute.UniformLayout)
				{
					float max = btnSizes.Max((Vector2 vector) => vector.x);
					for (int i2 = 0; i2 < btnSizes.Length; i2++)
					{
						btnSizes[i2] = new Vector2(max, btnSizes[i2].y);
					}
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				bool recalc = false;
				int width = innerWidth;
				if (isFirstFrame)
				{
					width = 999999;
					isFirstFrame = false;
					GUIHelper.RequestRepaint();
				}
				if (prevWidth != width || colCounts == null)
				{
					if (width > 0)
					{
						prevWidth = width;
					}
					recalc = true;
					width = prevWidth;
				}
				colCounts = colCounts ?? new int[property.Children.Count];
				if (recalc)
				{
					for (int i3 = 0; i3 < colCounts.Length; i3++)
					{
						colCounts[i3] = 0;
					}
					int currentCol = 0;
					Vector2 prevBtnSize = btnSizes[0];
					int btnWidth = 0;
					bool jumpRow = false;
					for (int i4 = 0; i4 < btnSizes.Length; i4++)
					{
						btnWidth = Mathf.Max((int)btnSizes[i4].x, btnWidth);
						int colWidth = btnWidth * (colCounts[currentCol] + 1);
						jumpRow = colWidth > width || (int)prevBtnSize.y != (int)btnSizes[i4].y;
						prevBtnSize = btnSizes[i4];
						if (jumpRow)
						{
							btnWidth = (int)btnSizes[i4].x;
							if (colCounts[currentCol] != 0)
							{
								currentCol++;
							}
						}
						colCounts[currentCol]++;
					}
				}
			}
			DefaultMethodDrawer.DontDrawMethodParameters = true;
			int j = 0;
			for (int y = 0; y < colCounts.Length; y++)
			{
				if (j >= property.Children.Count)
				{
					break;
				}
				GUILayout.BeginHorizontal();
				for (int x = 0; x < colCounts[y]; x++)
				{
					if (j >= property.Children.Count)
					{
						break;
					}
					InspectorProperty child = property.Children[j];
					child.Draw(child.Label);
					j++;
				}
				GUILayout.EndHorizontal();
			}
			DefaultMethodDrawer.DontDrawMethodParameters = false;
			if (Event.current.type == EventType.Repaint)
			{
				innerWidth = (int)GUIHelper.GetCurrentLayoutRect().width;
			}
		}
	}
}
