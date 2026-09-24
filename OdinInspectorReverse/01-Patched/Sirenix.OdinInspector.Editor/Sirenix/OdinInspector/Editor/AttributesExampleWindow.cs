using System;
using System.Linq;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	public class AttributesExampleWindow : OdinMenuEditorWindow
	{
		private OdinAttributeExampleItem example;

		public override Vector4 WindowPadding
		{
			get
			{
				return default(Vector4);
			}
			set
			{
			}
		}

		public static void OpenWindow()
		{
			OpenWindow(null);
		}

		protected override void DrawEditor(int index)
		{
			example?.Draw();
		}

		public static AttributesExampleWindow OpenWindow(Type attributeType)
		{
			bool isNew = Resources.FindObjectsOfTypeAll<AttributesExampleWindow>().Length == 0;
			AttributesExampleWindow w = EditorWindow.GetWindow<AttributesExampleWindow>();
			if (isNew)
			{
				w.MenuWidth = 250f;
				w.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(850f, 700f);
			}
			if (attributeType != null)
			{
				w.ForceMenuTreeRebuild();
				OdinMenuItem item = w.MenuTree.EnumerateTree().FirstOrDefault((OdinMenuItem x) => x.Value == attributeType);
				if (item != null)
				{
					w.MenuTree.Selection.Clear();
					w.MenuTree.Selection.Add(item);
				}
			}
			return w;
		}

		protected override OdinMenuTree BuildMenuTree()
		{
			OdinMenuTree tree = new OdinMenuTree();
			tree.Selection.SupportsMultiSelect = false;
			tree.Selection.SelectionChanged += SelectionChanged;
			tree.Config.DrawSearchToolbar = true;
			tree.Config.DefaultMenuStyle.Height = 22;
			AttributeExampleUtilities.BuildMenuTree(tree);
			return tree;
		}

		private void SelectionChanged(SelectionChangedType obj)
		{
			if (example != null)
			{
				example.OnDeselected();
				example = null;
			}
			Type attr = base.MenuTree.Selection.Select((OdinMenuItem i) => i.Value).FilterCast<Type>().FirstOrDefault();
			if (attr != null)
			{
				example = AttributeExampleUtilities.GetExample(attr);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (example != null)
			{
				example.OnDeselected();
				example = null;
			}
		}

		protected override void OnDestroy()
		{
			if (example != null)
			{
				example.OnDeselected();
				example = null;
			}
		}
	}
}
