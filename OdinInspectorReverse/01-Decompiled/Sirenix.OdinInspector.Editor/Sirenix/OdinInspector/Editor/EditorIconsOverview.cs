using System;
using System.Linq;
using System.Reflection;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
	/// <summary>
	/// Opens a window which displays a list of all icons available from <see cref="T:Sirenix.Utilities.Editor.EditorIcons" />.
	/// </summary>
	public class EditorIconsOverview : OdinSelector<object>
	{
		[PropertyRange(10.0, 34.0)]
		[ShowInInspector]
		[PropertyOrder(30f)]
		[InfoBox("This is an overview of all available icons in the Sirenix.Utilities.Editor.EditorIcons utility class.", InfoMessageType.Info, null)]
		[LabelWidth(50f)]
		private float Size
		{
			get
			{
				return base.SelectionTree.DefaultMenuStyle.IconSize;
			}
			set
			{
				base.SelectionTree.DefaultMenuStyle.IconSize = value;
				base.SelectionTree.DefaultMenuStyle.Height = (int)value + 9;
			}
		}

		/// <summary>
		/// Opens a window which displays a list of all icons available from <see cref="T:Sirenix.Utilities.Editor.EditorIcons" />.
		/// </summary>
		public static void OpenEditorIconsOverview()
		{
			OdinEditorWindow window = OdinEditorWindow.InspectObject(new EditorIconsOverview());
			window.ShowUtility();
			window.WindowPadding = default(Vector4);
		}

		/// <summary>
		/// Builds the selection tree.
		/// </summary>
		protected override void BuildSelectionTree(OdinMenuTree tree)
		{
			DrawConfirmSelectionButton = false;
			tree.Config.DrawSearchToolbar = true;
			tree.DefaultMenuStyle.Height = 25;
			tree.Config.SelectMenuItemsOnMouseDown = true;
			foreach (PropertyInfo item in from x in typeof(EditorIcons).GetProperties(BindingFlags.Static | BindingFlags.Public)
				orderby x.Name
				select x)
			{
				Type returnType = item.GetReturnType();
				if (typeof(Texture).IsAssignableFrom(returnType))
				{
					tree.Add(item.Name, item.Name, (Texture)item.GetGetMethod().Invoke(null, null));
				}
				else if (typeof(EditorIcon).IsAssignableFrom(returnType))
				{
					tree.Add(item.Name, item.Name, (EditorIcon)item.GetGetMethod().Invoke(null, null));
				}
			}
		}
	}
}
