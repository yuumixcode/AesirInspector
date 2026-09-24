using System.Linq;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	[HideLabel]
	[HideReferenceObjectPicker]
	public abstract class MenuTreeBrowser
	{
		private OdinMenuTree tree;

		private ResizableColumn[] columns = new ResizableColumn[2]
		{
			ResizableColumn.FlexibleColumn(280f, 80f),
			ResizableColumn.DynamicColumn()
		};

		/// <summary>
		/// The content padding
		/// </summary>
		[HideInInspector]
		public Vector2 ContentPadding = UnityShims.Vector4.op_ImplicitVec2(new Vector4(10f, 0f));

		public ResizableColumn MenuColumn => columns[0];

		/// <summary>
		/// Gets the value selected value.
		/// </summary>
		[InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
		[VerticalGroup(0f)]
		[SuppressInvalidAttributeError]
		[DisableContextMenu(true, false)]
		[EnableGUI]
		[HideLabel]
		[ShowInInspector]
		public object Value
		{
			get
			{
				if (tree == null)
				{
					return null;
				}
				return tree.Selection.FirstOrDefault()?.Value;
			}
		}

		/// <summary>
		/// Draws the menu tree.
		/// </summary>
		[OnInspectorGUI]
		[PropertyOrder(-1f)]
		protected virtual void DrawMenuTree()
		{
			Rect rect = GUIHelper.GetCurrentLayoutRect();
			GUITableUtilities.ResizeColumns(rect, columns);
			rect = EditorGUILayout.BeginVertical(GUILayoutOptions.Width(MenuColumn.ColWidth));
			EditorGUI.DrawRect(rect, SirenixGUIStyles.DarkEditorBackground);
			tree = tree ?? BuildMenuTree();
			if (tree != null)
			{
				tree.Config.AutoHandleKeyboardNavigation = true;
				tree.DrawMenuTree();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndVertical();
			GUILayout.Space(ContentPadding.x);
			SirenixEditorGUI.DrawBorders(rect, 1);
			GUILayout.BeginVertical();
			GUILayout.Space(ContentPadding.y);
		}

		[OnInspectorGUI]
		[PropertyOrder(-1000f)]
		private void BeginDrawEditor()
		{
			GUILayout.BeginHorizontal();
		}

		[PropertyOrder(1000f)]
		[OnInspectorGUI]
		private void EndDrawEditor()
		{
			GUILayout.Space(ContentPadding.y);
			GUILayout.EndVertical();
			GUILayout.Space(ContentPadding.x);
			GUILayout.EndHorizontal();
		}

		/// <summary>
		/// Invokes BuildMenuTree.
		/// </summary>
		public void ForceRebuildMenuTree()
		{
			tree = BuildMenuTree();
		}

		public abstract OdinMenuTree BuildMenuTree();
	}
}
