using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
	public static class DesignerStyles
	{
		public static class Header
		{
			private static GUIStyle labelStyleBackingField;

			public static GUIStyle LabelStyle
			{
				get
				{
					if (labelStyleBackingField == null)
					{
						labelStyleBackingField = new GUIStyle(EditorStyles.label)
						{
							fontSize = 14,
							alignment = TextAnchor.MiddleCenter
						};
					}
					return labelStyleBackingField;
				}
			}
		}

		public static class Row
		{
			private static GUIStyle columnLabelStyleBackingField;

			public static GUIStyle ColumnLabelStyle
			{
				get
				{
					if (columnLabelStyleBackingField == null)
					{
						columnLabelStyleBackingField = new GUIStyle(EditorStyles.label)
						{
							fontSize = 12,
							alignment = TextAnchor.MiddleCenter
						};
					}
					return columnLabelStyleBackingField;
				}
			}
		}

		public static class Node
		{
			private static GUIStyle labelStyleBackingField;

			private static GUIStyle boldLabelStyleBackingField;

			private static GUIStyle miniLabel;

			public static GUIStyle LabelStyle
			{
				get
				{
					if (labelStyleBackingField == null)
					{
						labelStyleBackingField = new GUIStyle(SirenixGUIStyles.Label)
						{
							fontSize = 12,
							richText = true
						};
					}
					return labelStyleBackingField;
				}
			}

			public static GUIStyle BoldLabelStyle
			{
				get
				{
					if (boldLabelStyleBackingField == null)
					{
						boldLabelStyleBackingField = new GUIStyle(EditorStyles.boldLabel)
						{
							fontSize = 12,
							richText = true
						};
					}
					return boldLabelStyleBackingField;
				}
			}

			public static GUIStyle MiniLabel
			{
				get
				{
					if (miniLabel == null)
					{
						GUIStyle baseStyle = (EditorGUIUtility.isProSkin ? SirenixGUIStyles.CenteredGreyMiniLabel : SirenixGUIStyles.MiniLabelCentered);
						miniLabel = new GUIStyle(baseStyle);
					}
					return miniLabel;
				}
			}
		}

		private static GUIStyle richLabelCenteredWordWrapBackingField;

		public static GUIStyle RichLabelCenteredWordWrap
		{
			get
			{
				if (richLabelCenteredWordWrapBackingField == null)
				{
					richLabelCenteredWordWrapBackingField = new GUIStyle(SirenixGUIStyles.LabelCentered)
					{
						richText = true,
						wordWrap = true
					};
				}
				return richLabelCenteredWordWrapBackingField;
			}
		}
	}
}
