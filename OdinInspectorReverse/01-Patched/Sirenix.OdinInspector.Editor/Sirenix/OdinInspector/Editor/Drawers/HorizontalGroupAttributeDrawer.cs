using System.Linq;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Drawers
{
	/// <summary>
	/// Drawer for the <see cref="T:Sirenix.OdinInspector.HorizontalGroupAttribute" />
	/// </summary>
	/// <seealso cref="T:Sirenix.OdinInspector.HorizontalGroupAttribute" />
	public class HorizontalGroupAttributeDrawer : OdinGroupDrawer<HorizontalGroupAttribute>
	{
		private struct HGroup
		{
			public LayoutSize Width;

			public LayoutSize MinWidth;

			public LayoutSize maxWidth;

			public LayoutSize MarginLeft;

			public LayoutSize PaddingLeft;

			public LayoutSize MarginRight;

			public LayoutSize PaddingRight;

			public float labelWidth;

			internal bool hasCustomLabelWidth;
		}

		private static readonly GUIScopeStack<float> labelWidthHorizontalGroupStack = new GUIScopeStack<float>();

		private ValueResolver<string> titleGetter;

		private float totalWidth;

		private HGroup[] groups;

		private bool enableAutomaticLabelWidth;

		public static void PushLabelWidthDefault(float labelWidth)
		{
			labelWidthHorizontalGroupStack.Push(labelWidth);
		}

		public static void PopLabelWidthDefault()
		{
			labelWidthHorizontalGroupStack.Pop();
		}

		protected override void Initialize()
		{
			if (base.Attribute.Title != null)
			{
				titleGetter = ValueResolver.GetForString(base.Property, base.Attribute.Title);
			}
			enableAutomaticLabelWidth = !base.Attribute.DisableAutomaticLabelWidth;
			float fallbackLabelWidth = base.Attribute.LabelWidth;
			groups = new HGroup[base.Property.Children.Count];
			for (int i = 0; i < base.Property.Children.Count; i++)
			{
				InspectorProperty child = base.Property.Children[i];
				HorizontalGroupAttribute attr = child.Children.Recurse().AppendWith(child).SelectMany((InspectorProperty a) => a.GetAttributes<HorizontalGroupAttribute>())
					.FirstOrDefault((HorizontalGroupAttribute x) => x.GroupID == base.Attribute.GroupID);
				HGroup group = default(HGroup);
				if (attr == null)
				{
					group.Width = LayoutSize.Auto;
					group.MinWidth = default(LayoutSize);
					group.maxWidth = LayoutSize.Percentage(1f);
					group.labelWidth = fallbackLabelWidth;
				}
				else
				{
					group.labelWidth = ((attr.LabelWidth > 0f) ? attr.LabelWidth : fallbackLabelWidth);
					group.MinWidth.Type = ((!(attr.MinWidth > 0f) || !(attr.MinWidth < 1f)) ? SizeMode.Pixels : SizeMode.Percentage);
					group.MinWidth.Value = ((attr.MinWidth == 0f) ? 0f : attr.MinWidth);
					group.maxWidth.Type = ((!(attr.MaxWidth >= 0f) || !(attr.MaxWidth < 1f)) ? SizeMode.Pixels : SizeMode.Percentage);
					group.maxWidth.Value = ((attr.MaxWidth == 0f) ? 1f : attr.MaxWidth);
					if (attr.Width > 0f)
					{
						group.Width.Value = attr.Width;
						group.Width.Type = ((!(attr.Width < 1f)) ? SizeMode.Pixels : SizeMode.Percentage);
					}
					if (attr.MarginLeft > 0f)
					{
						group.MarginLeft.Type = ((!(attr.MarginLeft < 1f)) ? SizeMode.Pixels : SizeMode.Percentage);
						group.MarginLeft.Value = attr.MarginLeft;
					}
					if (attr.MarginRight > 0f)
					{
						group.MarginRight.Type = ((!(attr.MarginRight < 1f)) ? SizeMode.Pixels : SizeMode.Percentage);
						group.MarginRight.Value = attr.MarginRight;
					}
					if (attr.PaddingLeft > 0f)
					{
						group.PaddingLeft.Type = ((!(attr.PaddingLeft < 1f)) ? SizeMode.Pixels : SizeMode.Percentage);
						group.PaddingLeft.Value = attr.PaddingLeft;
					}
					if (attr.PaddingRight > 0f)
					{
						group.PaddingRight.Type = ((!(attr.PaddingRight < 1f)) ? SizeMode.Pixels : SizeMode.Percentage);
						group.PaddingRight.Value = attr.PaddingRight;
					}
				}
				group.hasCustomLabelWidth = group.labelWidth > 0f;
				groups[i] = group;
			}
		}

		/// <summary>
		/// Draws the property.
		/// </summary>
		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (titleGetter != null)
			{
				if (titleGetter.HasError)
				{
					SirenixEditorGUI.MessageBox(titleGetter.ErrorMessage, MessageType.Error, GlobalConfig<GeneralDrawerConfig>.Instance.MessageBoxFontSize);
				}
				else
				{
					SirenixEditorGUI.Title(titleGetter.GetValue(), null, TextAlignment.Left, horizontalLine: false);
				}
			}
			bool hasOverrideDefaultLabelWidth = labelWidthHorizontalGroupStack.Count > 0;
			float overrideLabelWidth = 0f;
			if (hasOverrideDefaultLabelWidth)
			{
				overrideLabelWidth = labelWidthHorizontalGroupStack.Peek();
			}
			SirenixEditorGUI.BeginIndentedVertical();
			float prevFieldWidth = EditorGUIUtility.fieldWidth;
			EditorGUIUtility.fieldWidth = 40f;
			Rect rowRect = GUILayout_Internal.BeginRow();
			if (Event.current.type == EventType.Repaint)
			{
				if (totalWidth != rowRect.width)
				{
					GUIHelper.RequestRepaint();
				}
				totalWidth = rowRect.width;
			}
			int pCount = base.Property.Children.Count;
			float gap = base.Attribute.Gap;
			bool needsGap = false;
			for (int i = 0; i < pCount; i++)
			{
				InspectorProperty child = base.Property.Children[i];
				if (child.State.Visible)
				{
					if (needsGap && gap > 0f)
					{
						GUILayout_Internal.ColumnSpace((gap < 1f) ? LayoutSize.Percentage(gap) : LayoutSize.Pixels(gap));
					}
					ref HGroup group = ref groups[i];
					if (group.PaddingLeft.Type != SizeMode.Auto)
					{
						GUILayout_Internal.ColumnSpace(group.PaddingLeft);
					}
					if (group.MarginLeft.Type != SizeMode.Auto)
					{
						GUILayout_Internal.ColumnSpace(group.MarginLeft);
					}
					Rect rect = GUILayout_Internal.BeginColumn(group.Width, group.MinWidth, group.maxWidth);
					if (!group.hasCustomLabelWidth && Event.current.type == EventType.Repaint && enableAutomaticLabelWidth)
					{
						group.labelWidth = Mathf.Max(rect.width * 0.45f - 40f, 60f);
					}
					if (group.labelWidth != 0f)
					{
						GUIHelper.PushLabelWidth(hasOverrideDefaultLabelWidth ? overrideLabelWidth : group.labelWidth);
					}
					GUILayout.Space(0f);
					child.Draw(child.Label);
					GUILayout.Space(0f);
					GUILayout_Internal.EndColumn();
					if (group.labelWidth != 0f)
					{
						GUIHelper.PopLabelWidth();
					}
					if (group.PaddingRight.Type != SizeMode.Auto)
					{
						GUILayout_Internal.ColumnSpace(group.PaddingRight);
					}
					if (group.MarginRight.Type != SizeMode.Auto)
					{
						GUILayout_Internal.ColumnSpace(group.MarginRight);
					}
					needsGap = true;
				}
			}
			GUILayout_Internal.EndRow();
			EditorGUIUtility.fieldWidth = prevFieldWidth;
			SirenixEditorGUI.EndIndentedVertical();
		}
	}
}
